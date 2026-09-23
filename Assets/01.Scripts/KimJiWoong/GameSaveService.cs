using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// MainScene에 미리 배치하고 필요한 참조를 인스펙터에 연결해서 사용합니다.
public class GameSaveService : Singleton<GameSaveService>
{
    // 저장 시스템이 모든 필수 참조를 확인하고 불러오기를 마쳤는지 나타냅니다.
    public bool IsReady => ready;
    public static string SavePath
    {
        get
        {
#if UNITY_EDITOR
            // 자동 검증에서는 사용자의 실제 세이브에 접근하지 않습니다.
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-guildSaveSmokeTest") >= 0)
                return Path.Combine(Application.temporaryCachePath,
                    "guild-smoke-" + System.Diagnostics.Process.GetCurrentProcess().Id + ".json");
#endif
            return Path.Combine(Application.persistentDataPath, "guild-save-v1.json");
        }
    }
    [Header("데이터")]
    [SerializeField] private UnitDatabase database;

    [Header("게임 시스템")]
    [SerializeField] private UnitSpawner spawner;
    [SerializeField] private BarracksSystem barracks;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private BarracksManager barracksManager;
    [SerializeField] private GridUnitFactory gridUnitFactory;
    [SerializeField] private ObjectPoolManager objectPoolManager;
    [SerializeField] private BattleSlotPanel battleSlotPanel;

    [Header("파티 슬롯")]
    [SerializeField] private BattleSlotUI[] party;

    private bool ready;
    private float nextSave;

    private IEnumerator Start()
    {
        // 다른 매니저의 Start 초기화가 완료된 다음 저장 데이터를 적용합니다.
        yield return null;

        if (!ValidateInspectorReferences())
            yield break;

        if (!barracks.IsInitialized)
        {
            Debug.LogError("BarracksSystem 초기화 전에 GameSaveService가 실행되었습니다.", this);
            yield break;
        }

        database.RebuildIndex();
        ready = true;
        if (File.Exists(SavePath) && !LoadNow()) ready = false;
        nextSave = Time.unscaledTime + 10;
    }

    private void Update()
    {
        if (ready && Time.unscaledTime >= nextSave)
        {
            SaveNow();
            nextSave = Time.unscaledTime + 10;
        }
    }
    private void OnApplicationPause(bool paused) { if (paused) SaveNow(); }
    protected override void OnApplicationQuit()
    {
        if (instance != this) return;
        SaveNow();
        base.OnApplicationQuit();
    }

    public bool SaveNow()
    {
        if (!ready) return false;
        // 드래그 중에는 유닛이 임시 부모로 이동하므로 다음 저장 시점까지 기다립니다.
        if (DragableUnit.IsAnyDragging) return false;
        try
        {
            var save = new GameSaveData { gold = moneyManager.currentGold,
                credit = moneyManager.currentCredit, guildLevel = spawner.GuildLevel,
                stage = waveManager.CurrentStage, upgrades = barracks.CaptureUpgrades() };
            for (int i = 0; i < spawner.GridPanel.childCount; i++)
                CaptureSlot(save, spawner.GridPanel.GetChild(i), i, false);
            foreach (var slot in party) CaptureSlot(save, slot.transform, slot.slotIndex, true);
            Validate(save);
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath + ".tmp", JsonUtility.ToJson(save, true));
            // 완성된 파일만 교체하고 직전 저장은 .bak으로 보관합니다.
            if (File.Exists(SavePath)) File.Replace(SavePath + ".tmp", SavePath, SavePath + ".bak");
            else File.Move(SavePath + ".tmp", SavePath);
            return true;
        }
        catch (Exception ex) { Debug.LogError("저장 실패: " + ex.Message); return false; }
    }

    private static void CaptureSlot(GameSaveData save, Transform slot, int index, bool isParty)
    {
        var unit = slot.GetComponentInChildren<DragableUnit>(true);
        if (unit != null && unit.myData != null)
            save.units.Add(new SavedUnitSlot { party = isParty, slot = index, unitId = unit.myData.unitId });
    }

    public bool LoadNow()
    {
        if (!ready || !File.Exists(SavePath)) return false;
        if (DragableUnit.IsAnyDragging) return false;
        try
        {
            var save = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));
            Validate(save); // 기존 슬롯을 비우기 전에 전체 데이터를 확인합니다.
            waveManager.RestoreStage(save.stage);
            for (int i = 0; i < spawner.GridPanel.childCount; i++) ClearSlot(spawner.GridPanel.GetChild(i));
            foreach (var slot in party) ClearSlot(slot.transform);
            battleSlotPanel.SyncAllBattleSlots();
            barracks.RestoreUpgrades(save.upgrades);
            foreach (var entry in save.units)
            {
                var data = database.Find(entry.unitId);
                Transform parent = entry.party ? Array.Find(party, x => x.slotIndex == entry.slot).transform
                    : spawner.GridPanel.GetChild(entry.slot);
                if (gridUnitFactory.CreateUnit(data.uiPoolName, data, parent) == null)
                    throw new InvalidOperationException("유닛 생성 실패: " + entry.unitId);
            }
            battleSlotPanel.SyncAllBattleSlots();
            moneyManager.RestoreBalance(save.gold, save.credit);
            spawner.RestoreGuildLevel(save.guildLevel);
            return true;
        }
        catch (Exception ex)
        {
            ready = false; // 복원 실패 상태가 원본 저장을 덮어쓰지 않도록 자동 저장 중지
            Debug.LogError("불러오기 실패(원본 파일 보존): " + ex.Message);
            return false;
        }
    }

    private void Validate(GameSaveData save)
{
    if (save == null)
        throw new InvalidDataException("저장 데이터가 null입니다.");

    if (save.version != 1)
        throw new InvalidDataException(
            $"지원하지 않는 저장 버전: {save.version}"
        );

    if (save.units == null)
        throw new InvalidDataException("유닛 저장 목록이 null입니다.");

    if (save.upgrades == null)
        throw new InvalidDataException("강화 저장 목록이 null입니다.");

    if (save.gold < 0)
        throw new InvalidDataException(
            $"골드가 음수입니다: {save.gold}"
        );

    if (save.credit < 0)
        throw new InvalidDataException(
            $"크레딧이 음수입니다: {save.credit}"
        );

    if (save.stage < 1)
        throw new InvalidDataException(
            $"스테이지가 잘못되었습니다: {save.stage}"
        );

    if (save.guildLevel < 1)
        throw new InvalidDataException(
            $"길드 레벨이 잘못되었습니다: {save.guildLevel}"
        );

    if (save.guildLevel > spawner.MaxGuildLevel)
    {
        throw new InvalidDataException(
            $"길드 레벨이 최대치를 초과했습니다. " +
            $"현재: {save.guildLevel}, 최대: {spawner.MaxGuildLevel}"
        );
    }
        var used = new HashSet<string>();
        foreach (var entry in save.units)
        {
            if (entry == null || !used.Add(entry.party + ":" + entry.slot) || database.Find(entry.unitId) == null ||
                (entry.party ? !Array.Exists(party, x => x.slotIndex == entry.slot)
                    : entry.slot < 0 || entry.slot >= spawner.GridPanel.childCount))
                throw new InvalidDataException("유닛 ID 또는 슬롯이 현재 게임 설정과 다릅니다.");
            var data = database.Find(entry.unitId);
            if (!objectPoolManager.canvasPools.Exists(x => x.poolName == data.uiPoolName && x.prefab != null)
                && !objectPoolManager.objList.Exists(x => x.poolName == data.uiPoolName && x.prefab != null))
                throw new InvalidDataException("유닛 UI 풀 설정 누락: " + data.uiPoolName);
        }
        var stats = new HashSet<StatType>();
        foreach (var upgrade in save.upgrades)
            if (upgrade == null || !stats.Add(upgrade.type) || !Enum.IsDefined(typeof(StatType), upgrade.type) ||
                upgrade.level < 1 || upgrade.level > 10000) throw new InvalidDataException("잘못된 강화 정보");
    }

    private void ClearSlot(Transform slot)
    {
        foreach (var unit in slot.GetComponentsInChildren<DragableUnit>(true))
        {
            unit.CompleteExternalDrop();
            objectPoolManager.ReturnObject(unit.myData.uiPoolName, unit.gameObject);
        }
    }

    private bool ValidateInspectorReferences()
    {
        var missing = new List<string>();
        if (database == null) missing.Add(nameof(database));
        if (spawner == null || spawner.GridPanel == null) missing.Add(nameof(spawner));
        if (barracks == null) missing.Add(nameof(barracks));
        if (moneyManager == null) missing.Add(nameof(moneyManager));
        if (waveManager == null) missing.Add(nameof(waveManager));
        if (barracksManager == null) missing.Add(nameof(barracksManager));
        if (gridUnitFactory == null) missing.Add(nameof(gridUnitFactory));
        if (objectPoolManager == null) missing.Add(nameof(objectPoolManager));
        if (battleSlotPanel == null) missing.Add(nameof(battleSlotPanel));
        if (party == null || party.Length == 0 || Array.Exists(party, slot => slot == null))
            missing.Add(nameof(party));

        if (missing.Count == 0) return true;

        Debug.LogError("GameSaveService 인스펙터 연결 누락: " + string.Join(", ", missing), this);
        return false;
    }
}

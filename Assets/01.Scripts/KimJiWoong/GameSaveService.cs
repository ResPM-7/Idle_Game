using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬에 미리 배치해서 사용합니다. 씬 로드 후 초기화가 끝나면 자동 복원합니다.
// 씬 이동 시 유지 여부는 부모 Singleton의 Is Dont Destroy 설정을 따릅니다.
public class GameSaveService : Singleton<GameSaveService>
{
    // 기존 대문자 Instance 호출도 부모 싱글톤의 동일한 인스턴스를 사용합니다.
    public static GameSaveService Instance => instance;
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
    private UnitDatabase database;
    private UnitSpawner spawner;
    private BarracksSystem barracks;
    private BattleSlotUI[] party;
    private bool ready;
    private float nextSave;

    private void OnEnable()
    {
        // 중복 오브젝트가 파괴되기 전 씬 이벤트를 구독하지 않도록 합니다.
        if (instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ready = false;
        StopAllCoroutines();
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return null;
        spawner = FindFirstObjectByType<UnitSpawner>();
        barracks = FindFirstObjectByType<BarracksSystem>();
        if (spawner == null || spawner.GridPanel == null || barracks == null ||
            !barracks.IsInitialized || MoneyManager.instance == null ||
            WaveManager.instance == null || BarracksManager.instance == null ||
            GridUnitFactory.instance == null || ObjectPoolManager.instance == null ||
            BattleSlotPanel.instance == null) yield break;
        party = BattleSlotPanel.instance.GetComponentsInChildren<BattleSlotUI>(true);
        database = Resources.Load<UnitDatabase>("GameData/UnitDatabase");
        if (database == null) { Debug.LogError("UnitDatabase 에셋이 없어 저장을 시작하지 않습니다."); yield break; }
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
        if (!ready || spawner == null || MoneyManager.instance == null || WaveManager.instance == null) return false;
        // 드래그 중에는 유닛이 임시 부모로 이동하므로 다음 저장 시점까지 기다립니다.
        foreach (var unit in FindObjectsByType<DragableUnit>(FindObjectsSortMode.None))
            if (unit.originalParent != null) return false;
        try
        {
            var save = new GameSaveData { gold = MoneyManager.instance.currentGold,
                credit = MoneyManager.instance.currentCredit, guildLevel = spawner.GuildLevel,
                stage = WaveManager.instance.CurrentStage, upgrades = barracks.CaptureUpgrades() };
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
        foreach (var unit in FindObjectsByType<DragableUnit>(FindObjectsSortMode.None))
            if (unit.originalParent != null) return false;
        try
        {
            var save = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));
            Validate(save); // 기존 슬롯을 비우기 전에 전체 데이터를 확인합니다.
            WaveManager.instance.RestoreStage(save.stage);
            for (int i = 0; i < spawner.GridPanel.childCount; i++) ClearSlot(spawner.GridPanel.GetChild(i));
            foreach (var slot in party) ClearSlot(slot.transform);
            BattleSlotPanel.instance.SyncAllBattleSlots();
            barracks.RestoreUpgrades(save.upgrades);
            foreach (var entry in save.units)
            {
                var data = database.Find(entry.unitId);
                Transform parent = entry.party ? Array.Find(party, x => x.slotIndex == entry.slot).transform
                    : spawner.GridPanel.GetChild(entry.slot);
                if (GridUnitFactory.instance.CreateUnit(data.uiPoolName, data, parent) == null)
                    throw new InvalidOperationException("유닛 생성 실패: " + entry.unitId);
            }
            BattleSlotPanel.instance.SyncAllBattleSlots();
            MoneyManager.instance.RestoreBalance(save.gold, save.credit);
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
        if (save == null || save.version != 1 || save.units == null || save.upgrades == null ||
            save.gold < 0 || save.credit < 0 || save.stage < 1 || save.guildLevel < 1 ||
            save.guildLevel > spawner.MaxGuildLevel) throw new InvalidDataException("지원하지 않거나 잘못된 저장 데이터");
        var used = new HashSet<string>();
        foreach (var entry in save.units)
        {
            if (entry == null || !used.Add(entry.party + ":" + entry.slot) || database.Find(entry.unitId) == null ||
                (entry.party ? !Array.Exists(party, x => x.slotIndex == entry.slot)
                    : entry.slot < 0 || entry.slot >= spawner.GridPanel.childCount))
                throw new InvalidDataException("유닛 ID 또는 슬롯이 현재 게임 설정과 다릅니다.");
            var data = database.Find(entry.unitId);
            if (!ObjectPoolManager.instance.canvasPools.Exists(x => x.poolName == data.uiPoolName && x.prefab != null)
                && !ObjectPoolManager.instance.objList.Exists(x => x.poolName == data.uiPoolName && x.prefab != null))
                throw new InvalidDataException("유닛 UI 풀 설정 누락: " + data.uiPoolName);
        }
        var stats = new HashSet<StatType>();
        foreach (var upgrade in save.upgrades)
            if (upgrade == null || !stats.Add(upgrade.type) || !Enum.IsDefined(typeof(StatType), upgrade.type) ||
                upgrade.level < 1 || upgrade.level > 10000) throw new InvalidDataException("잘못된 강화 정보");
    }

    private static void ClearSlot(Transform slot)
    {
        foreach (var unit in slot.GetComponentsInChildren<DragableUnit>(true))
        {
            unit.CompleteExternalDrop();
            ObjectPoolManager.instance.ReturnObject(unit.myData.uiPoolName, unit.gameObject);
        }
    }
}

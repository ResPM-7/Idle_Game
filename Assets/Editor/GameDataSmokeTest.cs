using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Unity 배치 실행: -executeMethod GameDataSmokeTest.Run -guildSaveSmokeTest
// 실제 사용자 저장과 분리된 임시 파일로 재화/강화/유닛 복원을 검사합니다.
[InitializeOnLoad]
public static class GameDataSmokeTest
{
    static GameDataSmokeTest() { EditorApplication.update += Tick; }
    public static void Run()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-guildSaveSmokeTest") < 0)
            throw new InvalidOperationException("테스트 저장 파일 분리 옵션이 필요합니다.");
        var db = AssetDatabase.LoadAssetAtPath<UnitDatabase>(GameDataTools.DatabasePath);
        db.RebuildIndex();
        Check(db.players.Count == 9 && db.enemies.Count == 2, "유닛 데이터 개수");
        Check(db.Find(201) != db.Find(201, true), "아군/적 ID 분리");
        foreach (var unit in db.players)
            foreach (var next in unit.nextUpgradeUnits)
                Check(next != null && db.Find(next.unitId) == next, "진화 참조 보존");
        SessionState.SetInt("GuildSmoke", 1);
        SessionState.SetFloat("GuildSmokeStart", (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/00.Scenes/MainScene.unity");
        EditorApplication.EnterPlaymode();
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("검증 실패: " + message);
    }
    private static void Tick()
    {
        int state = SessionState.GetInt("GuildSmoke", 0);
        if (state == 0) return;
        try
        {
            if (EditorApplication.timeSinceStartup - SessionState.GetFloat("GuildSmokeStart", 0) > 180)
                throw new Exception("저장 시스템 초기화 시간 초과");
            if (state == 2 && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetInt("GuildSmoke", 0);
                Debug.Log("GUILD_DATA_SMOKE_PASS");
                EditorApplication.Exit(0);
                return;
            }
            if (state == 4 && EditorApplication.isPlaying && GameSaveService.instance != null && GameSaveService.instance.IsReady)
            {
                Check(MoneyManager.instance.currentGold == 12345 && MoneyManager.instance.currentCredit == 678, "씬 재진입 재화 자동 복원");
                Check(PartyBuildManager.instance.GetActiveUnitCount() == 1, "씬 재진입 파티 자동 복원");
                Check(WaveManager.instance.CurrentStage == 3, "씬 재진입 진행도 자동 복원");
                SessionState.SetInt("GuildSmoke", 2);
                EditorApplication.ExitPlaymode();
                return;
            }
            if (state != 1 || !EditorApplication.isPlaying || GameSaveService.instance == null || !GameSaveService.instance.IsReady) return;
            SessionState.SetInt("GuildSmoke", 3);
            var service = GameSaveService.instance;
            var db = Resources.Load<UnitDatabase>("GameData/UnitDatabase");
            var spawner = UnityEngine.Object.FindFirstObjectByType<UnitSpawner>();
            WaveManager.instance.StageGiveUp();
            Check(WaveManager.instance.stageGiveUp, "빈 파티 시작 차단");
            MoneyManager.instance.RestoreBalance(12345, 678);
            var slot = spawner.GridPanel.GetChild(0);
            if (slot.childCount == 0) GridUnitFactory.instance.CreateUnit(db.Find(101).uiPoolName, db.Find(101), slot);
            var partySlots = BattleSlotPanel.instance.GetComponentsInChildren<BattleSlotUI>(true);
            Check(partySlots.Length > 0, "파티 슬롯 존재");
            GridUnitFactory.instance.CreateUnit(db.Find(102).uiPoolName, db.Find(102), partySlots[0].transform);
            BattleSlotPanel.instance.SyncAllBattleSlots();
            var barracks = UnityEngine.Object.FindFirstObjectByType<BarracksSystem>();
            var upgrades = barracks.CaptureUpgrades();
            foreach (var upgrade in upgrades) upgrade.level = 3;
            barracks.RestoreUpgrades(upgrades);
            WaveManager.instance.RestoreStage(3);
            spawner.RestoreGuildLevel(2);
            Check(service.SaveNow(), "저장");
            string expected = File.ReadAllText(GameSaveService.SavePath);
            MoneyManager.instance.RestoreBalance(1, 2);
            spawner.RestoreGuildLevel(1);
            Check(service.LoadNow(), "불러오기");
            Check(MoneyManager.instance.currentGold == 12345 && MoneyManager.instance.currentCredit == 678, "재화 복원");
            Check(WaveManager.instance.CurrentStage == 3 && WaveManager.instance.stageGiveUp, "스테이지 대기 복원");
            Check(spawner.GuildLevel == 2, "길드 등급 복원");
            Check(service.SaveNow() && File.ReadAllText(GameSaveService.SavePath) == expected, "저장 왕복 일치");
            Check(service.LoadNow(), "반복 복원");
            Check(service.SaveNow() && File.ReadAllText(GameSaveService.SavePath) == expected, "반복 복원 중복 방지");
            Check(PartyBuildManager.instance.GetActiveUnitCount() == 1, "파티 복원");
            Check(File.Exists(GameSaveService.SavePath + ".bak"), "백업 생성");
            SessionState.SetInt("GuildSmoke", 4);
            EditorSceneManager.LoadSceneInPlayMode("Assets/00.Scenes/MainScene.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
        }
        catch (Exception ex)
        {
            SessionState.SetInt("GuildSmoke", 0);
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }
}

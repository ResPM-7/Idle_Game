using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class StageMonsterSync : EditorWindow
{
    private const string stageMonsterSavePath = "Assets/03.Data/Stage/StageMonsterData.asset";

    [MenuItem("Tools/5. 구글 시트 동기화 (스테이지 몬스터)")]
    public static void SyncStageMonsterData()
    {
        var stageReq = UnityWebRequest.Get(GoogleSheetSync.stageMonsterDataUrl);
        var stageOp = stageReq.SendWebRequest();

        stageOp.completed += (op) =>
        {
            if (stageReq.result == UnityWebRequest.Result.Success)
            {
                ParseAndApplyStageMonsterData(stageReq.downloadHandler.text);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("StageMonsterData 동기화 완료!");
            }
            else
            {
                Debug.LogError("StageMonsterData 동기화 실패: " + stageReq.error);
            }

            stageReq.Dispose();
        };
    }

    private static void ParseAndApplyStageMonsterData(string csv)
    {
        //Debug.Log("=== RAW CSV START ===\n" + csv + "\n=== RAW CSV END ===");


        string folder = Path.GetDirectoryName(stageMonsterSavePath).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string folderName = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        StageMonsterDataSO so = AssetDatabase.LoadAssetAtPath<StageMonsterDataSO>(stageMonsterSavePath);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<StageMonsterDataSO>();
            AssetDatabase.CreateAsset(so, stageMonsterSavePath);
        }

        Dictionary<int, UnitDataSO> enemyDict = new Dictionary<int, UnitDataSO>();
        string[] guids = AssetDatabase.FindAssets("t:UnitDataSO", new[] { GoogleSheetSync.enemySavePath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitDataSO enemySO = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path);
            if (enemySO != null) enemyDict[enemySO.unitId] = enemySO;
        }

        List<StageMonsterEntry> newEntries = new List<StageMonsterEntry>();
        string[] lines = csv.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 4) continue;

            if (!int.TryParse(values[0], out int startStage)) continue;
            if (!int.TryParse(values[1], out int endStage)) continue;
            if (!int.TryParse(values[2], out int enemyId)) continue;
            if (!int.TryParse(values[3], out int bossId)) continue;

            StageMonsterEntry entry = new StageMonsterEntry
            {
                startStage = startStage,
                endStage = endStage,
                enemyId = enemyId,
                bossId = bossId
            };

            if (enemyDict.TryGetValue(enemyId, out UnitDataSO enemySO))
                entry.enemyData = enemySO;
            else
                Debug.LogWarning($"[StageMonsterData] {startStage}~{endStage} 구간의 EnemyID {enemyId}를 찾을 수 없습니다.");

            if (enemyDict.TryGetValue(bossId, out UnitDataSO bossSO))
                entry.bossData = bossSO;
            else
                Debug.LogWarning($"[StageMonsterData] {startStage}~{endStage} 구간의 BossID {bossId}를 찾을 수 없습니다.");

            newEntries.Add(entry);
        }

        so.entries = newEntries;
        EditorUtility.SetDirty(so);

        Debug.Log($"StageMonsterData 저장 완료! 총 {newEntries.Count}개 구간");
    }
}


using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class StageMonsterSync : EditorWindow
{
    private const string stageMonsterSavePath =
        "Assets/03.Data/Stage/StageMonsterData.asset";

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
                Debug.LogError(
                    "StageMonsterData 동기화 실패: " + stageReq.error
                );
            }

            stageReq.Dispose();
        };
    }

    private static void ParseAndApplyStageMonsterData(string csv)
    {
        Debug.Log("===== StageMonster CSV 원본 =====");
        Debug.Log(csv);
        Debug.Log("================================");

        // ---------------------------------------------------------
        // StageMonsterDataSO 저장 폴더 확인
        // ---------------------------------------------------------

        string folder = Path.GetDirectoryName(stageMonsterSavePath)
            .Replace('\\', '/');

        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = Path.GetDirectoryName(folder)
                .Replace('\\', '/');

            string folderName = Path.GetFileName(folder);

            AssetDatabase.CreateFolder(parent, folderName);
        }

        // ---------------------------------------------------------
        // StageMonsterDataSO 가져오기
        // ---------------------------------------------------------

        StageMonsterDataSO so =
            AssetDatabase.LoadAssetAtPath<StageMonsterDataSO>(
                stageMonsterSavePath
            );

        if (so == null)
        {
            so = ScriptableObject.CreateInstance<StageMonsterDataSO>();

            AssetDatabase.CreateAsset(
                so,
                stageMonsterSavePath
            );
        }

        // ---------------------------------------------------------
        // 적 UnitDataSO 찾기
        // ---------------------------------------------------------

        Dictionary<int, UnitDataSO> enemyDict =
            new Dictionary<int, UnitDataSO>();

        string[] guids =
            AssetDatabase.FindAssets("t:UnitDatabase");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            UnitDatabase database =
                AssetDatabase.LoadAssetAtPath<UnitDatabase>(path);

            if (database == null)
                continue;

            foreach (UnitDataSO enemy in database.enemies)
            {
                if (enemy == null)
                    continue;

                enemyDict[enemy.unitId] = enemy;
            }
        }

        Debug.Log(
            $"[StageMonsterData] 등록된 적 유닛 수: {enemyDict.Count}"
        );

        // ---------------------------------------------------------
        // CSV 읽기
        // ---------------------------------------------------------

        List<StageMonsterEntry> newEntries =
            new List<StageMonsterEntry>();

        string[] lines = csv.Split('\n');

        // 시트 컬럼:
        // StartStage, EndStage, EnemyID

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');

            // 최소 3개 컬럼 필요
            if (values.Length < 3)
            {
                Debug.LogWarning(
                    $"[StageMonsterData] {i}번째 줄의 컬럼 수가 부족합니다: {line}"
                );

                continue;
            }

            // -----------------------------------------------------
            // StartStage
            // -----------------------------------------------------

            if (!int.TryParse(
                    values[0].Trim(),
                    out int startStage))
            {
                Debug.LogWarning(
                    $"[StageMonsterData] StartStage 변환 실패: {values[0]}"
                );

                continue;
            }

            // -----------------------------------------------------
            // EndStage
            // -----------------------------------------------------

            if (!int.TryParse(
                    values[1].Trim(),
                    out int endStage))
            {
                Debug.LogWarning(
                    $"[StageMonsterData] EndStage 변환 실패: {values[1]}"
                );

                continue;
            }

            string[] enemyIdStrs =
                values[2].Split('/');

            List<UnitDataSO> pool =
                new List<UnitDataSO>();

            foreach (string idStr in enemyIdStrs)
            {
                if (!int.TryParse(
                        idStr.Trim(),
                        out int enemyId))
                {
                    continue;
                }

                if (enemyDict.TryGetValue(
                        enemyId,
                        out UnitDataSO enemySO))
                {
                    pool.Add(enemySO);
                }
                else
                {
                    Debug.LogWarning(
                        $"[StageMonsterData] " +
                        $"{startStage}~{endStage} 구간의 " +
                        $"EnemyID {enemyId}를 찾을 수 없습니다."
                    );
                }
            }

            // -----------------------------------------------------
            // StageMonsterEntry 생성
            // -----------------------------------------------------

            StageMonsterEntry entry =
                new StageMonsterEntry
                {
                    startStage = startStage,
                    endStage = endStage,
                    enemyPool = pool.ToArray(),
                };

            newEntries.Add(entry);

            Debug.Log(
                $"[StageMonsterData] " +
                $"{startStage}~{endStage} 구간 추가 완료 " +
                $"/ 몬스터 {pool.Count}개"
            );
        }

        // ---------------------------------------------------------
        // ScriptableObject에 저장
        // ---------------------------------------------------------

        so.entries = newEntries;

        EditorUtility.SetDirty(so);

        Debug.Log(
            $"StageMonsterData 저장 완료! " +
            $"총 {newEntries.Count}개 구간"
        );
    }
}
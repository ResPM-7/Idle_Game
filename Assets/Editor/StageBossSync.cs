using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class StageBossSync : EditorWindow
{
    private const string stageBossSavePath =
        "Assets/03.Data/Stage/StageBossData.asset";

    [MenuItem("Tools/6. 구글 시트 동기화 (스테이지 보스)")]
    public static void SyncStageBossData()
    {
        var bossReq = UnityWebRequest.Get(GoogleSheetSync.stageBossDataUrl);
        var bossOp = bossReq.SendWebRequest();

        bossOp.completed += (op) =>
        {
            if (bossReq.result == UnityWebRequest.Result.Success)
            {
                ParseAndApplyStageBossData(bossReq.downloadHandler.text);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("StageBossData 동기화 완료!");
            }
            else
            {
                Debug.LogError(
                    "StageBossData 동기화 실패: " + bossReq.error
                );
            }

            bossReq.Dispose();
        };
    }

    private static void ParseAndApplyStageBossData(string csv)
    {
        // 저장 폴더 확인
        string folder =
            Path.GetDirectoryName(stageBossSavePath).Replace('\\', '/');

        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent =
                Path.GetDirectoryName(folder).Replace('\\', '/');

            string folderName =
                Path.GetFileName(folder);

            AssetDatabase.CreateFolder(parent, folderName);
        }

        // StageBossDataSO 불러오기
        StageBossDataSO so =
            AssetDatabase.LoadAssetAtPath<StageBossDataSO>(
                stageBossSavePath
            );

        // 없으면 새로 생성
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<StageBossDataSO>();

            AssetDatabase.CreateAsset(
                so,
                stageBossSavePath
            );
        }

        Dictionary<int, UnitDataSO> bossDict =
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

                bossDict[enemy.unitId] = enemy;
            }
        }

        List<StageBossEntry> newEntries =
            new List<StageBossEntry>();

        string[] lines =
            csv.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            string[] values =
                line.Split(',');

            if (values.Length < 2)
                continue;

            if (!int.TryParse(
                    values[0].Trim(),
                    out int stage))
                continue;

            if (!int.TryParse(
                    values[1].Trim(),
                    out int bossId))
                continue;

            // 보스 ID 찾기
            if (!bossDict.TryGetValue(
                    bossId,
                    out UnitDataSO bossSO))
            {
                Debug.LogWarning(
                    $"[StageBossData] " +
                    $"Stage {stage}의 BossID {bossId}를 찾을 수 없습니다."
                );

                continue;
            }

            StageBossEntry entry =
                new StageBossEntry
                {
                    stage = stage,
                    bossData = bossSO
                };

            newEntries.Add(entry);
        }

        // 데이터 적용
        so.entries = newEntries;

        EditorUtility.SetDirty(so);

        Debug.Log(
            $"StageBossData 저장 완료! " +
            $"총 {newEntries.Count}개 보스"
        );
    }
}

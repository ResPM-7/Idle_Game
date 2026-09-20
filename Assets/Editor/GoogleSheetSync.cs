using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class GoogleSheetSync : EditorWindow
{
    // =========================================================
    // 1. 유닛 데이터 세팅 (아군 & 적군 분리)
    // =========================================================
    private const string unitDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=0&single=true&output=csv";
    private const string enemyDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=1042200275&single=true&output=csv";

    // =========================================================
    // 2. 오브젝트 풀 데이터 세팅
    // =========================================================
    private const string objectPoolDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=1927439287&single=true&output=csv";
    private const string canvasPoolDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=620845422&single=true&output=csv";

    private const string poolManagerPrefabPath = "Assets/02.Prefab/Manager/ObjectPoolManager.prefab";
    private const string poolPrefabFolder = "Assets/02.Prefab/ObjectPool";

    // =========================================================
    // 유닛 데이터 동기화 (아군 + 적군)
    // =========================================================
    [MenuItem("Tools/1. 구글 시트 동기화 (유닛 데이터 통합)")]
    public static void SyncData()
    {
        if (UnitSheetSyncController.IsBusy)
        {
            Debug.LogWarning("[유닛 시트] 현재 동기화 또는 컴파일이 진행 중입니다.");
            return;
        }

        var playerReq = UnityWebRequest.Get(unitDataUrl);
        var playerOp = playerReq.SendWebRequest();

        playerOp.completed += (op1) =>
        {
            if (playerReq.result == UnityWebRequest.Result.Success)
            {
                string playerCsv = playerReq.downloadHandler.text;

                var enemyReq = UnityWebRequest.Get(enemyDataUrl);
                var enemyOp = enemyReq.SendWebRequest();

                enemyOp.completed += (op2) =>
                {
                    if (enemyReq.result == UnityWebRequest.Result.Success)
                    {
                        UnitSheetSyncController.StartSync(
                            playerCsv,
                            enemyReq.downloadHandler.text);
                    }
                    else
                    {
                        Debug.LogError("적군 동기화 실패: " + enemyReq.error);
                    }
                    enemyReq.Dispose();
                };
            }
            else
            {
                Debug.LogError("아군 동기화 실패: " + playerReq.error);
            }
            playerReq.Dispose();
        };
    }

    // =========================================================
    // 오브젝트 풀 데이터 동기화
    // =========================================================
    [MenuItem("Tools/2. 구글 시트 동기화 (오브젝트 풀 사이즈)")]
    public static void SyncPoolData()
    {
        var requestObj = UnityWebRequest.Get(objectPoolDataUrl);
        var opObj = requestObj.SendWebRequest();

        opObj.completed += (asyncOpObj) =>
        {
            if (requestObj.result == UnityWebRequest.Result.Success)
            {
                string objCsv = requestObj.downloadHandler.text;
                var requestCanvas = UnityWebRequest.Get(canvasPoolDataUrl);
                var opCanvas = requestCanvas.SendWebRequest();

                opCanvas.completed += (asyncOpCanvas) =>
                {
                    if (requestCanvas.result == UnityWebRequest.Result.Success)
                    {
                        string canvasCsv = requestCanvas.downloadHandler.text;
                        ParseAndApplyPoolData(objCsv, canvasCsv);
                    }
                    else
                    {
                        Debug.LogError("캔버스 풀 데이터 동기화 실패: " + requestCanvas.error);
                    }
                    requestCanvas.Dispose();
                };
            }
            else
            {
                Debug.LogError("일반 오브젝트 풀 데이터 동기화 실패: " + requestObj.error);
            }
            requestObj.Dispose();
        };
    }

    private static void ParseAndApplyPoolData(string objCsv, string canvasCsv)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(poolManagerPrefabPath);
        if (prefab == null) return;

        ObjectPoolManager manager = prefab.GetComponent<ObjectPoolManager>();
        if (manager == null) return;

        // --- 1. 일반 오브젝트 풀 완전 동기화 ---
        string[] objLines = objCsv.Split('\n');
        HashSet<string> validObjPools = new HashSet<string>();

        for (int i = 2; i < objLines.Length; i++)
        {
            string line = objLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 2 || string.IsNullOrWhiteSpace(values[0])) continue;

            string sheetPoolName = values[0];
            int.TryParse(values[1], out int sheetPoolSize);
            GameObject matchingPrefab = FindPrefabByName(sheetPoolName);

            validObjPools.Add(sheetPoolName);

            bool isFound = false;
            for (int j = 0; j < manager.objList.Count; j++)
            {
                if (manager.objList[j].poolName == sheetPoolName)
                {
                    var item = manager.objList[j];
                    item.poolSize = sheetPoolSize;
                    if (matchingPrefab != null) item.prefab = matchingPrefab;
                    manager.objList[j] = item;
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                manager.objList.Add(new ObjectPoolManager.ObjectPoolItem
                {
                    poolName = sheetPoolName,
                    poolSize = sheetPoolSize,
                    prefab = matchingPrefab
                });
            }
        }

        for (int i = manager.objList.Count - 1; i >= 0; i--)
        {
            if (!validObjPools.Contains(manager.objList[i].poolName))
            {
                manager.objList.RemoveAt(i);
            }
        }

        // --- 2. 캔버스 풀 완전 동기화 ---
        string[] canvasLines = canvasCsv.Split('\n');
        HashSet<string> validCanvasPools = new HashSet<string>();

        for (int i = 2; i < canvasLines.Length; i++)
        {
            string line = canvasLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 2 || string.IsNullOrWhiteSpace(values[0])) continue;

            string sheetPoolName = values[0];
            int.TryParse(values[1], out int sheetPoolSize);
            GameObject matchingPrefab = FindPrefabByName(sheetPoolName);

            validCanvasPools.Add(sheetPoolName);

            bool isFound = false;
            for (int j = 0; j < manager.canvasPools.Count; j++)
            {
                if (manager.canvasPools[j].poolName == sheetPoolName)
                {
                    var item = manager.canvasPools[j];
                    item.poolSize = sheetPoolSize;
                    if (matchingPrefab != null) item.prefab = matchingPrefab;
                    manager.canvasPools[j] = item;
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                manager.canvasPools.Add(new ObjectPoolManager.CanvasPoolItem
                {
                    poolName = sheetPoolName,
                    poolSize = sheetPoolSize,
                    prefab = matchingPrefab
                });
            }
        }

        for (int i = manager.canvasPools.Count - 1; i >= 0; i--)
        {
            if (!validCanvasPools.Contains(manager.canvasPools[i].poolName))
            {
                manager.canvasPools.RemoveAt(i);
            }
        }

        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.Refresh();
        Debug.Log(" 오브젝트 풀 사이즈 구글 시트 동기화 완료!");
    }

    private static GameObject FindPrefabByName(string prefabName)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {prefabName}", new[] { poolPrefabFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == prefabName)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        return null;
    }
}

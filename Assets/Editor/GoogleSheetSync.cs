using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class GoogleSheetSync : EditorWindow
{
    // =========================================================
    // 1. 유닛 데이터 세팅
    // =========================================================
    private const string unitDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=0&single=true&output=csv";
    private const string savePath = "Assets/03.Data/Units";

    // =========================================================
    // 2. 오브젝트 풀 데이터 세팅
    // =========================================================
    private const string objectPoolDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=1927439287&single=true&output=csv";
    private const string canvasPoolDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=620845422&single=true&output=csv";

    private const string poolManagerPrefabPath = "Assets/02.Prefab/Manager/ObjectPoolManager.prefab";
    private const string poolPrefabFolder = "Assets/02.Prefab/ObjectPool";

    // =========================================================
    // 유닛 데이터 동기화
    // =========================================================
    [MenuItem("Tools/1. 구글 시트 동기화 (유닛 데이터)")]
    public static void SyncData()
    {
        var request = UnityWebRequest.Get(unitDataUrl);
        var operation = request.SendWebRequest();

        operation.completed += (asyncOp) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseAndApplyUnitData(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("유닛 동기화 실패: " + request.error);
            }
            request.Dispose();
        };
    }

    private static void ParseAndApplyUnitData(string csv)
    {
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            Directory.CreateDirectory(savePath);
            AssetDatabase.Refresh();
        }

        string[] guids = AssetDatabase.FindAssets("t:UnitDataSO");
        Dictionary<int, UnitDataSO> soDict = new Dictionary<int, UnitDataSO>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitDataSO so = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path);
            if (so != null) soDict[so.unitId] = so;
        }

        string[] lines = csv.Split('\n');
        Dictionary<UnitDataSO, int> upgradeLinks = new Dictionary<UnitDataSO, int>();

        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            // [오류 수정] 콤마만 있는 빈 줄(,,,,)이거나 데이터가 부족하면 무시
            if (values.Length < 12 || string.IsNullOrWhiteSpace(values[0])) continue;

            // 숫자가 아니면 무시
            if (!int.TryParse(values[0], out int id)) continue;

            string unitName = values[4];

            if (!soDict.TryGetValue(id, out UnitDataSO targetSO))
            {
                targetSO = ScriptableObject.CreateInstance<UnitDataSO>();
                targetSO.unitId = id;

                string assetPath = $"{savePath}/Unit_{id}.asset";
                AssetDatabase.CreateAsset(targetSO, assetPath);
                soDict[id] = targetSO;
            }

            targetSO.uiPoolName = values[1];
            targetSO.battlePoolName = values[2];
            targetSO.unitName = unitName;

            // [안전 장치] 빈칸이 섞여 있어도 에러가 터지지 않고 0으로 처리되도록 TryParse 적용
            int.TryParse(values[3], out targetSO.unitLevel);
            float.TryParse(values[5], out targetSO.maxHp);
            float.TryParse(values[6], out targetSO.moveSpeed);
            float.TryParse(values[7], out targetSO.attackDamage);
            float.TryParse(values[8], out targetSO.attackSpeed);
            float.TryParse(values[9], out targetSO.attackRange);
            int.TryParse(values[10], out targetSO.coin);
            int.TryParse(values[11], out targetSO.credit);

            // M열 (12): nextUpgradeUnitId
            if (values.Length > 12 && !string.IsNullOrWhiteSpace(values[12]))
            {
                if (int.TryParse(values[12], out int nextId))
                {
                    upgradeLinks[targetSO] = nextId;
                }
            }

            EditorUtility.SetDirty(targetSO);
        }

        foreach (var link in upgradeLinks)
        {
            UnitDataSO currentSO = link.Key;
            int nextUpgradeId = link.Value;

            if (soDict.TryGetValue(nextUpgradeId, out UnitDataSO nextSO))
            {
                currentSO.nextUpgradeUnit = nextSO;
                EditorUtility.SetDirty(currentSO);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("유닛 데이터 구글 시트 동기화 완료");
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
        if (prefab == null)
        {
            Debug.LogError($"경로 오류: [{poolManagerPrefabPath}] 에서 프리팹을 찾을 수 없습니다.");
            return;
        }

        ObjectPoolManager manager = prefab.GetComponent<ObjectPoolManager>();
        if (manager == null) return;

        // 1. 일반 오브젝트 풀
        string[] objLines = objCsv.Split('\n');
        for (int i = 2; i < objLines.Length; i++)
        {
            string line = objLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 2 || string.IsNullOrWhiteSpace(values[0])) continue;

            string sheetPoolName = values[0];
            int.TryParse(values[1], out int sheetPoolSize);
            GameObject matchingPrefab = FindPrefabByName(sheetPoolName);

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

        // 2. 캔버스 풀
        string[] canvasLines = canvasCsv.Split('\n');
        for (int i = 2; i < canvasLines.Length; i++)
        {
            string line = canvasLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 2 || string.IsNullOrWhiteSpace(values[0])) continue;

            string sheetPoolName = values[0];
            int.TryParse(values[1], out int sheetPoolSize);
            GameObject matchingPrefab = FindPrefabByName(sheetPoolName);

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

        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.Refresh();
        Debug.Log("오브젝트 풀 사이즈 구글 시트 동기화 완료! (일반 풀 & 캔버스 풀)");
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
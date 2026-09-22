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

    private const string playerSavePath = "Assets/03.Data/Units/Player";
    private const string enemySavePath = "Assets/03.Data/Units/Enemy";

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
        var playerReq = UnityWebRequest.Get(unitDataUrl);
        var playerOp = playerReq.SendWebRequest();

        playerOp.completed += (op1) =>
        {
            if (playerReq.result == UnityWebRequest.Result.Success)
            {
                ParseAndApplyUnitData(playerReq.downloadHandler.text, playerSavePath, "Unit_");

                var enemyReq = UnityWebRequest.Get(enemyDataUrl);
                var enemyOp = enemyReq.SendWebRequest();

                enemyOp.completed += (op2) =>
                {
                    if (enemyReq.result == UnityWebRequest.Result.Success)
                    {
                        ParseAndApplyUnitData(enemyReq.downloadHandler.text, enemySavePath, "Enemy_");

                        AssetDatabase.SaveAssets();
                        GameDataTools.Rebuild();
                        AssetDatabase.Refresh();
                        Debug.Log(" 아군 및 적군 유닛 데이터 구글 시트 동기화 완료! (배열 진화 오류 수정됨)");
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

    private static void ParseAndApplyUnitData(string csv, string targetFolderPath, string fileNamePrefix)
    {
        string groupPath = targetFolderPath + ".asset";
        UnitDatabase group = AssetDatabase.LoadAssetAtPath<UnitDatabase>(groupPath);
        if (!AssetDatabase.IsValidFolder(targetFolderPath))
        {
            string parent = Path.GetDirectoryName(targetFolderPath).Replace('\\', '/');
            string folder = Path.GetFileName(targetFolderPath);
            AssetDatabase.CreateFolder(parent, folder);
        }

        string[] guids = AssetDatabase.FindAssets("t:UnitDataSO", new[] { targetFolderPath });
        Dictionary<int, UnitDataSO> soDict = new Dictionary<int, UnitDataSO>();
        if (group != null)
        {
            foreach (var unit in fileNamePrefix == "Enemy_" ? group.enemies : group.players)
                soDict.Add(unit.unitId, unit);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitDataSO so = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path);
            if (so != null) soDict[so.unitId] = so;
        }

        string[] lines = csv.Split('\n');

        Dictionary<UnitDataSO, string> upgradeLinks = new Dictionary<UnitDataSO, string>();

        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 15 || string.IsNullOrWhiteSpace(values[0])) continue;

            if (!int.TryParse(values[0], out int id)) continue;

            string unitName = values[4];

            if (!soDict.TryGetValue(id, out UnitDataSO targetSO))
            {
                targetSO = ScriptableObject.CreateInstance<UnitDataSO>();
                targetSO.unitId = id;

                string assetPath = $"{targetFolderPath}/{fileNamePrefix}{id}.asset";
                if (group == null) AssetDatabase.CreateAsset(targetSO, assetPath);
                else
                {
                    targetSO.name = fileNamePrefix + id;
                    AssetDatabase.AddObjectToAsset(targetSO, group);
                    (fileNamePrefix == "Enemy_" ? group.enemies : group.players).Add(targetSO);
                    EditorUtility.SetDirty(group);
                }
                soDict[id] = targetSO;
            }

            targetSO.uiPoolName = values[1];
            targetSO.team = fileNamePrefix == "Enemy_" ? Team_Test.Enemy : Team_Test.Player;
            targetSO.nextUpgradeUnits = new UnitDataSO[0];
            targetSO.battlePoolName = values[2];
            targetSO.unitName = unitName;

            int.TryParse(values[3], out targetSO.unitLevel);
            float.TryParse(values[5], out targetSO.maxHp);
            float.TryParse(values[6], out targetSO.moveSpeed);
            float.TryParse(values[7], out targetSO.attackDamage);
            float.TryParse(values[8], out targetSO.attackSpeed);
            float.TryParse(values[9], out targetSO.attackRange);
            int.TryParse(values[10], out targetSO.defense);
            int.TryParse(values[11], out targetSO.criticalRate);
            float.TryParse(values[12], out targetSO.criticalDamage);

            int.TryParse(values[13], out targetSO.coin);
            int.TryParse(values[14], out targetSO.credit);

            // 다중 선택 데이터 조립 로직
            // 쉼표 때문에 쪼개져버린 15번 인덱스부터 끝까지의 모든 텍스트를 다시 하나로 묶어줍니다.
            if (values.Length > 15)
            {
                string joinedIds = string.Join(",", values, 15, values.Length - 15);
                joinedIds = joinedIds.Replace("\"", ""); // 구글 시트가 억지로 넣은 큰따옴표 제거

                if (!string.IsNullOrWhiteSpace(joinedIds))
                {
                    upgradeLinks[targetSO] = joinedIds;
                }
            }

            EditorUtility.SetDirty(targetSO);
        }

        foreach (var link in upgradeLinks)
        {
            UnitDataSO currentSO = link.Key;
            string arrayString = link.Value;

            string[] nextIds = arrayString.Split(new char[] { ' ', ',', '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<UnitDataSO> nextSOList = new List<UnitDataSO>();

            foreach (string idStr in nextIds)
            {
                if (int.TryParse(idStr.Trim(), out int nextId))
                {
                    if (soDict.TryGetValue(nextId, out UnitDataSO nextSO))
                    {
                        nextSOList.Add(nextSO);
                    }
                    else
                    {
                        Debug.LogWarning($"[동기화 경고] {currentSO.unitName}의 진화 대상인 {nextId}번 유닛을 찾을 수 없습니다.");
                    }
                }
            }

            currentSO.nextUpgradeUnits = nextSOList.ToArray();
            EditorUtility.SetDirty(currentSO);
        }
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

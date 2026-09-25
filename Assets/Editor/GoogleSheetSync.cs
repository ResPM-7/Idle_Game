using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
    public const string stageMonsterDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=340714933&single=true&output=csv";
    public const string stageBossDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRny9PnlR7YezXkx3ulR9BIlbLecmDIfGHieYOmDhXE3_t8Qw8KJGTGPC9y5G4Kh1J6qykVh89rm9by/pub?gid=2000866037&single=true&output=csv";

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
                try
                {
                    ParseAndApplyUnitData(playerReq.downloadHandler.text, playerSavePath, "Unit_");
                }
                catch (Exception exception)
                {
                    Debug.LogError("아군 데이터 적용 실패: " + exception.Message);
                    playerReq.Dispose();
                    return;
                }

                var enemyReq = UnityWebRequest.Get(enemyDataUrl);
                var enemyOp = enemyReq.SendWebRequest();

                enemyOp.completed += (op2) =>
                {
                    if (enemyReq.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            ParseAndApplyUnitData(enemyReq.downloadHandler.text, enemySavePath, "Enemy_");
                            AssetDatabase.SaveAssets();
                            GameDataTools.Rebuild();
                            AssetDatabase.Refresh();
                            Debug.Log("아군 및 적군 유닛 데이터 구글 시트 동기화 완료!");
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError("적군 데이터 적용 실패: " + exception.Message);
                        }
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
        List<string[]> rows = ParseCsv(csv);
        if (rows.Count < 2)
            throw new InvalidDataException("유닛 시트에 필드명 행과 자료형 행이 필요합니다.");

        Dictionary<string, int> columns = BuildColumnMap(rows[0]);
        RequireColumn(columns, "unitId");

        string groupPath = targetFolderPath + ".asset";
        UnitDatabase group = AssetDatabase.LoadAssetAtPath<UnitDatabase>(groupPath);
        if (group == null && !AssetDatabase.IsValidFolder(targetFolderPath))
        {
            string parent =
                Path.GetDirectoryName(targetFolderPath).Replace('\\', '/');

            string folder = Path.GetFileName(targetFolderPath);
            AssetDatabase.CreateFolder(parent, folder);
        }

        // 통합 파일 방식을 사용하면 개별 SO 폴더가 없을 수 있으므로
        // 실제 폴더가 존재할 때만 검색합니다.
        string[] guids = AssetDatabase.IsValidFolder(targetFolderPath)
                        ? AssetDatabase.FindAssets(
                        "t:UnitDataSO",
                        new[] { targetFolderPath }
                    )
                    : new string[0];
        Dictionary<int, UnitDataSO> soDict = new Dictionary<int, UnitDataSO>();
        if (group != null)
        {
            foreach (var unit in fileNamePrefix == "Enemy_" ? group.enemies : group.players)
                if (unit != null) soDict[unit.unitId] = unit;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitDataSO so = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path);
            if (so != null) soDict[so.unitId] = so;
        }

        Dictionary<UnitDataSO, string> upgradeLinks = new Dictionary<UnitDataSO, string>();

        for (int i = 2; i < rows.Count; i++)
        {
            string[] values = rows[i];
            string idText = ReadCell(values, columns, "unitId");
            if (string.IsNullOrWhiteSpace(idText)) continue;
            if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) || id <= 0)
            {
                Debug.LogWarning($"[유닛 동기화] {i + 1}행의 unitId가 올바르지 않습니다: {idText}");
                continue;
            }

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

            targetSO.team = fileNamePrefix == "Enemy_" ? Team_Test.Enemy : Team_Test.Player;
            AssignString(values, columns, "uiPoolName", value => targetSO.uiPoolName = value);
            AssignString(values, columns, "battlePoolName", value => targetSO.battlePoolName = value);
            AssignString(values, columns, "unitName", value => targetSO.unitName = value);
            AssignInt(values, columns, "unitLevel", value => targetSO.unitLevel = value, i);
            AssignFloat(values, columns, "maxHp", value => targetSO.maxHp = value, i);
            AssignFloat(values, columns, "moveSpeed", value => targetSO.moveSpeed = value, i);
            AssignFloat(values, columns, "attackDamage", value => targetSO.attackDamage = value, i);
            AssignFloat(values, columns, "attackSpeed", value => targetSO.attackSpeed = value, i);
            AssignFloat(values, columns, "searchRange", value => targetSO.searchRange = value, i);
            AssignFloat(values, columns, "attackRange", value => targetSO.attackRange = value, i);
            AssignBool(values, columns, "canMelee", value => targetSO.canMelee = value, i);
            AssignBool(values, columns, "canRanged", value => targetSO.canRanged = value, i);
            AssignString(values, columns, "projectilePoolName", value => targetSO.projectilePoolName = value);
            AssignBool(values, columns, "canHeal", value => targetSO.canHeal = value, i);
            AssignFloat(values, columns, "healRange", value => targetSO.healRange = value, i);
            AssignString(values, columns, "healProjectilePoolName", value => targetSO.healProjectilePoolName = value);
            AssignInt(values, columns, "defense", value => targetSO.defense = value, i);
            AssignInt(values, columns, "criticalRate", value => targetSO.criticalRate = Mathf.Clamp(value, 0, 100), i);
            AssignFloat(values, columns, "criticalDamage", value => targetSO.criticalDamage = value, i);
            AssignInt(values, columns, "coin", value => targetSO.coin = value, i);
            AssignInt(values, columns, "credit", value => targetSO.credit = value, i);

            if (TryReadCell(values, columns, "nextUpgradeUnitIds", out string upgradeIds)
                || TryReadCell(values, columns, "nextUpgradeUnits", out upgradeIds))
            {
                targetSO.nextUpgradeUnits = Array.Empty<UnitDataSO>();
                if (!string.IsNullOrWhiteSpace(upgradeIds)) upgradeLinks[targetSO] = upgradeIds;
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

        if (group != null)
        {
            List<UnitDataSO> list = fileNamePrefix == "Enemy_" ? group.enemies : group.players;
            list.Sort((a, b) => a == null ? 1 : b == null ? -1 : a.unitId.CompareTo(b.unitId));
            EditorUtility.SetDirty(group);
        }
    }

    private static Dictionary<string, int> BuildColumnMap(string[] headers)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i].Trim().TrimStart('\uFEFF');
            if (!string.IsNullOrEmpty(header)) result[header] = i;
        }
        return result;
    }

    private static void RequireColumn(Dictionary<string, int> columns, string name)
    {
        if (!columns.ContainsKey(name))
            throw new InvalidDataException($"유닛 시트에 필수 열 '{name}'이 없습니다.");
    }

    private static string ReadCell(string[] row, Dictionary<string, int> columns, string name)
    {
        return TryReadCell(row, columns, name, out string value) ? value : string.Empty;
    }

    private static bool TryReadCell(
        string[] row,
        Dictionary<string, int> columns,
        string name,
        out string value)
    {
        value = string.Empty;
        if (!columns.TryGetValue(name, out int index) || index >= row.Length) return false;
        value = row[index].Trim();
        return true;
    }

    private static void AssignString(
        string[] row,
        Dictionary<string, int> columns,
        string name,
        Action<string> setter)
    {
        if (TryReadCell(row, columns, name, out string value)) setter(value);
    }

    private static void AssignInt(
        string[] row,
        Dictionary<string, int> columns,
        string name,
        Action<int> setter,
        int rowIndex)
    {
        if (!TryReadCell(row, columns, name, out string text)) return;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) setter(value);
        else Debug.LogWarning($"[유닛 동기화] {rowIndex + 1}행의 {name} 값이 올바르지 않습니다: {text}");
    }

    private static void AssignFloat(
        string[] row,
        Dictionary<string, int> columns,
        string name,
        Action<float> setter,
        int rowIndex)
    {
        if (!TryReadCell(row, columns, name, out string text)) return;
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)) setter(value);
        else Debug.LogWarning($"[유닛 동기화] {rowIndex + 1}행의 {name} 값이 올바르지 않습니다: {text}");
    }

    private static void AssignBool(
        string[] row,
        Dictionary<string, int> columns,
        string name,
        Action<bool> setter,
        int rowIndex)
    {
        if (!TryReadCell(row, columns, name, out string text)) return;
        if (bool.TryParse(text, out bool value))
        {
            setter(value);
            return;
        }

        if (text == "1" || text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text.Equals("y", StringComparison.OrdinalIgnoreCase))
            setter(true);
        else if (text == "0" || text.Equals("no", StringComparison.OrdinalIgnoreCase) || text.Equals("n", StringComparison.OrdinalIgnoreCase))
            setter(false);
        else
            Debug.LogWarning($"[유닛 동기화] {rowIndex + 1}행의 {name} 값이 올바르지 않습니다: {text}");
    }

    private static List<string[]> ParseCsv(string csv)
    {
        var rows = new List<string[]>();
        var row = new List<string>();
        var cell = new StringBuilder();
        bool quoted = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char character = csv[i];
            if (character == '"')
            {
                if (quoted && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else quoted = !quoted;
            }
            else if (character == ',' && !quoted)
            {
                row.Add(cell.ToString());
                cell.Clear();
            }
            else if ((character == '\n' || character == '\r') && !quoted)
            {
                if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                row.Add(cell.ToString());
                cell.Clear();
                if (row.Count > 1 || !string.IsNullOrWhiteSpace(row[0])) rows.Add(row.ToArray());
                row.Clear();
            }
            else cell.Append(character);
        }

        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            if (row.Count > 1 || !string.IsNullOrWhiteSpace(row[0])) rows.Add(row.ToArray());
        }
        return rows;
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

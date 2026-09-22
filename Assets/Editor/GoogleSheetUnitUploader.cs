using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Unity에 저장된 UnitDataSO들을 Google Sheet로 업로드하는 에디터 전용 도구입니다.
/// 자동 업로드는 팀원의 시트 작업을 덮어쓸 수 있으므로, 메뉴를 눌렀을 때만 실행합니다.
/// </summary>
internal static class GoogleSheetUnitUploader
{
    private const string PlayerFolder = "Assets/03.Data/Units/Player";
    private const string EnemyFolder = "Assets/03.Data/Units/Enemy";

    // 접속 정보는 Git에 올라가지 않도록 프로젝트 파일이 아닌 EditorPrefs에 저장합니다.
    internal const string WebAppUrlKey = "IdleGame.GoogleSheetUpload.WebAppUrl";
    internal const string AccessTokenKey = "IdleGame.GoogleSheetUpload.AccessToken";

    private static UnityWebRequest activeRequest;
    internal static bool IsUploading => activeRequest != null;
    internal static string Status { get; private set; } = string.Empty;

    [MenuItem("Tools/3. 구글 시트 업로드 설정")]
    private static void OpenSettings()
    {
        GoogleSheetUploadSettingsWindow.Open();
    }

    [MenuItem("Tools/4. 유니티 유닛 데이터 -> 구글 시트 업로드")]
    private static void UploadUnitData()
    {
        var database = AssetDatabase.LoadAssetAtPath<UnitDatabase>(GameDataTools.DatabasePath);
        UploadDatabase(database);
    }

    internal static void UploadDatabase(UnitDatabase database)
    {
        if (database == null)
        {
            Status = "UnitDatabase를 찾을 수 없습니다.";
            Debug.LogError(Status);
            return;
        }
        if (activeRequest != null)
        {
            Debug.LogWarning("[구글 시트 업로드] 이전 업로드가 아직 진행 중입니다.");
            return;
        }

        string webAppUrl = EditorPrefs.GetString(WebAppUrlKey, string.Empty).Trim();
        string accessToken = EditorPrefs.GetString(AccessTokenKey, string.Empty);

        if (string.IsNullOrWhiteSpace(webAppUrl) || string.IsNullOrWhiteSpace(accessToken))
        {
            EditorUtility.DisplayDialog(
                "구글 시트 업로드 설정 필요",
                "먼저 Tools > 3. 구글 시트 업로드 설정에서 Apps Script 주소와 인증 토큰을 입력해 주세요.",
                "확인");
            GoogleSheetUploadSettingsWindow.Open();
            return;
        }

        if (!webAppUrl.StartsWith("https://script.google.com/", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("[구글 시트 업로드] Apps Script 배포 주소가 올바르지 않습니다.");
            return;
        }

        string targets = string.Join(", ", new[]
        {
            database.players.Count > 0 ? "UnitData (아군)" : null,
            database.enemies.Count > 0 ? "EnemyData (적)" : null
        }.Where(x => x != null));
        if (targets.Length == 0)
        {
            Status = "업로드할 유닛이 없습니다.";
            return;
        }
        if (!EditorUtility.DisplayDialog(
                "구글 시트에 업로드",
                $"현재 데이터베이스의 전체 목록으로 {targets} 탭을 갱신합니다. 시트의 기존 값이 변경됩니다. 계속할까요?",
                "업로드",
                "취소"))
        {
            return;
        }

        SheetUploadRequest payload;
        try
        {
            var sheets = new List<SheetUploadData>();
            if (database.players.Count > 0) sheets.Add(BuildSheet("UnitData", database.players));
            if (database.enemies.Count > 0) sheets.Add(BuildSheet("EnemyData", database.enemies));
            payload = new SheetUploadRequest
            {
                token = accessToken,
                sheets = sheets.ToArray()
            };
        }
        catch (Exception exception)
        {
            Status = "데이터 검사 실패: " + exception.Message;
            Debug.LogError("[구글 시트 업로드] 데이터 검사 실패: " + exception.Message);
            return;
        }

        string json = JsonUtility.ToJson(payload);
        AssetDatabase.SaveAssets();
        byte[] body = Encoding.UTF8.GetBytes(json);

        // 공개 CSV 주소는 읽기 전용이므로 Apps Script 웹 앱에 JSON을 POST합니다.
        activeRequest = new UnityWebRequest(webAppUrl, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(body),
            downloadHandler = new DownloadHandlerBuffer(),
            redirectLimit = 8,
            timeout = 30
        };
        activeRequest.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

        UnityWebRequestAsyncOperation operation = activeRequest.SendWebRequest();
        operation.completed += _ => FinishUpload();
        Status = targets + " 업로드 중…";
        Debug.Log("[구글 시트 업로드] Unity 데이터를 전송하고 있습니다...");
    }

    private static SheetUploadData BuildSheet(string sheetName, List<UnitDataSO> source)
    {
        if (source.Any(unit => unit == null || unit.unitId <= 0))
            throw new InvalidOperationException($"{sheetName}: 비어 있거나 ID가 잘못된 유닛이 있습니다.");
        List<UnitDataSO> units = source
            .OrderBy(unit => unit.unitId)
            .ToList();

        if (units.Count == 0)
            throw new InvalidOperationException($"{sheetName}에 업로드할 유닛이 없습니다.");

        int[] duplicatedIds = units
            .GroupBy(unit => unit.unitId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicatedIds.Length > 0)
            throw new InvalidOperationException($"{sheetName}에 중복 unitId가 있습니다: {string.Join(", ", duplicatedIds)}");

        foreach (var unit in units)
            if (unit.nextUpgradeUnits != null && unit.nextUpgradeUnits.Any(next => next == null || !units.Contains(next)))
                throw new InvalidOperationException($"{unit.unitId}: 진화 대상이 해당 시트 목록에 없습니다.");

        // 1행 변수명과 2행 자료형은 현재 Google Sheet 형식을 그대로 유지합니다.
        string[] headers =
        {
            "unitId", "uiPoolName", "battlePoolName", "unitLevel", "unitName",
            "maxHp", "moveSpeed", "attackDamage", "attackSpeed", "attackRange",
            "defense", "criticalRate", "criticalDamage", "coin", "credit",
            "nextUpgradeUnitIds"
        };
        string[] types =
        {
            "int", "string", "string", "int", "string",
            "float", "float", "float", "float", "float",
            "int", "int", "float", "int", "int", "UnitDataSO []"
        };

        SheetUploadRow[] rows = units.Select(ToUploadRow).ToArray();
        return new SheetUploadData
        {
            sheetName = sheetName,
            headers = headers,
            types = types,
            rows = rows
        };
    }

    private static SheetUploadRow ToUploadRow(UnitDataSO unit)
    {
        // 숫자는 PC의 언어 설정과 상관없이 소수점을 '.'으로 보내도록 InvariantCulture를 사용합니다.
        string[] nextUpgradeIds = unit.nextUpgradeUnits == null
            ? Array.Empty<string>()
            : unit.nextUpgradeUnits
                .Where(next => next != null)
                .Select(next => next.unitId.ToString(CultureInfo.InvariantCulture))
                .ToArray();

        return new SheetUploadRow
        {
            values = new[]
            {
                unit.unitId.ToString(CultureInfo.InvariantCulture),
                unit.uiPoolName ?? string.Empty,
                unit.battlePoolName ?? string.Empty,
                unit.unitLevel.ToString(CultureInfo.InvariantCulture),
                unit.unitName ?? string.Empty,
                unit.maxHp.ToString(CultureInfo.InvariantCulture),
                unit.moveSpeed.ToString(CultureInfo.InvariantCulture),
                unit.attackDamage.ToString(CultureInfo.InvariantCulture),
                unit.attackSpeed.ToString(CultureInfo.InvariantCulture),
                unit.attackRange.ToString(CultureInfo.InvariantCulture),
                unit.defense.ToString(CultureInfo.InvariantCulture),
                unit.criticalRate.ToString(CultureInfo.InvariantCulture),
                unit.criticalDamage.ToString(CultureInfo.InvariantCulture),
                unit.coin.ToString(CultureInfo.InvariantCulture),
                unit.credit.ToString(CultureInfo.InvariantCulture),
                // Google Sheet의 다중 선택 드롭다운은 선택값을 쉼표로 구분합니다.
                string.Join(", ", nextUpgradeIds)
            }
        };
    }

    private static void FinishUpload()
    {
        UnityWebRequest request = activeRequest;
        activeRequest = null;

        try
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Status = "업로드 실패: " + request.error;
                Debug.LogError($"[구글 시트 업로드] 통신 실패: {request.error}\n{request.downloadHandler?.text}");
                return;
            }

            string responseText = request.downloadHandler.text;
            SheetUploadResponse response = JsonUtility.FromJson<SheetUploadResponse>(responseText);
            if (response == null || !response.success)
            {
                string message = response == null ? responseText : response.message;
                Status = "시트 갱신 실패: " + message;
                Debug.LogError("[구글 시트 업로드] 시트 갱신 실패: " + message);
                return;
            }

            Status = "업로드 완료: " + response.message;
            Debug.Log("[구글 시트 업로드] 완료: " + response.message);
        }
        catch (Exception exception)
        {
            Status = "업로드 응답 처리 실패: " + exception.Message;
            Debug.LogError(Status);
        }
        finally
        {
            request.Dispose();
        }
    }

    [Serializable]
    private sealed class SheetUploadRequest
    {
        public string token;
        public SheetUploadData[] sheets;
    }

    [Serializable]
    private sealed class SheetUploadData
    {
        public string sheetName;
        public string[] headers;
        public string[] types;
        public SheetUploadRow[] rows;
    }

    [Serializable]
    private sealed class SheetUploadRow
    {
        public string[] values;
    }

    [Serializable]
    private sealed class SheetUploadResponse
    {
        public bool success = false;
        public string message = string.Empty;
    }
}

/// <summary>
/// Apps Script 접속 정보를 각 개발자의 PC에만 저장하는 설정 창입니다.
/// </summary>
internal sealed class GoogleSheetUploadSettingsWindow : EditorWindow
{
    private string webAppUrl;
    private string accessToken;

    internal static void Open()
    {
        GetWindow<GoogleSheetUploadSettingsWindow>("Google Sheet Upload");
    }

    private void OnEnable()
    {
        webAppUrl = EditorPrefs.GetString(GoogleSheetUnitUploader.WebAppUrlKey, string.Empty);
        accessToken = EditorPrefs.GetString(GoogleSheetUnitUploader.AccessTokenKey, string.Empty);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Unity → Google Sheet 설정", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "GoogleSheetWriteWebApp.gs.txt를 Google Sheet의 Apps Script에 배포한 다음 /exec 주소와 동일한 인증 토큰을 입력하세요. 이 값은 Git에 저장되지 않습니다.",
            MessageType.Info);

        webAppUrl = EditorGUILayout.TextField("Web App URL", webAppUrl);
        accessToken = EditorGUILayout.PasswordField("Access Token", accessToken);

        GUILayout.Space(8f);
        if (GUILayout.Button("설정 저장"))
        {
            EditorPrefs.SetString(GoogleSheetUnitUploader.WebAppUrlKey, webAppUrl.Trim());
            EditorPrefs.SetString(GoogleSheetUnitUploader.AccessTokenKey, accessToken);
            Debug.Log("[구글 시트 업로드] 접속 정보를 이 PC의 EditorPrefs에 저장했습니다.");
            Close();
        }
    }
}

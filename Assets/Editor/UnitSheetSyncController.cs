using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
internal static class UnitSheetSyncController
{
    private const string PendingKey = "UnitSheetSync.Pending.V1";
    private static bool applying;

    [Serializable]
    private sealed class PendingSync
    {
        public string playerCsv;
        public string enemyCsv;
    }

    internal static bool IsBusy =>
        applying || EditorApplication.isCompiling ||
        !string.IsNullOrEmpty(SessionState.GetString(PendingKey, string.Empty));

    static UnitSheetSyncController()
    {
        EditorApplication.delayCall += ResumeAfterCompile;
    }

    internal static void StartSync(string playerCsv, string enemyCsv)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[유닛 시트] 이전 동기화가 아직 완료되지 않았습니다.");
            return;
        }

        try
        {
            UnitSheetTable player = UnitSheetTable.Parse(playerCsv, "UnitData");
            UnitSheetTable enemy = UnitSheetTable.Parse(enemyCsv, "EnemyData");
            List<UnitSheetTable.Column> schema = UnitSheetTable.MergeSchema(player, enemy);
            ValidateReservedMembers(schema);

            PendingSync pending = new PendingSync
            {
                playerCsv = playerCsv,
                enemyCsv = enemyCsv
            };
            SessionState.SetString(PendingKey, JsonUtility.ToJson(pending));

            if (UnitDataSOScriptUpdater.UpdateFields(schema))
            {
                Debug.Log("[유닛 시트] UnitDataSO.cs의 변수 구조를 갱신했습니다. 재컴파일 후 SO 값을 자동 적용합니다.");
                AssetDatabase.ImportAsset(UnitDataSOScriptUpdater.AssetPath, ImportAssetOptions.ForceUpdate);
                return;
            }

            if (!UnitSheetAssetApplier.RuntimeSchemaMatches(schema))
            {
                Debug.Log("[유닛 시트] UnitDataSO 재컴파일 후 SO 값을 자동 적용합니다.");
                CompilationPipeline.RequestScriptCompilation();
                return;
            }

            ApplyPending(pending, player, enemy, schema);
        }
        catch (Exception exception)
        {
            SessionState.EraseString(PendingKey);
            Debug.LogError("[유닛 시트] " + exception.Message);
        }
    }

    private static void ResumeAfterCompile()
    {
        string json = SessionState.GetString(PendingKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += ResumeAfterCompile;
            return;
        }

        try
        {
            PendingSync pending = JsonUtility.FromJson<PendingSync>(json);
            UnitSheetTable player = UnitSheetTable.Parse(pending.playerCsv, "UnitData");
            UnitSheetTable enemy = UnitSheetTable.Parse(pending.enemyCsv, "EnemyData");
            List<UnitSheetTable.Column> schema = UnitSheetTable.MergeSchema(player, enemy);
            if (!UnitSheetAssetApplier.RuntimeSchemaMatches(schema))
                throw new InvalidOperationException("UnitDataSO.cs 컴파일이 완료되지 않았습니다. Console의 컴파일 오류를 확인해 주세요.");
            ApplyPending(pending, player, enemy, schema);
        }
        catch (Exception exception)
        {
            SessionState.EraseString(PendingKey);
            Debug.LogError("[유닛 시트] " + exception.Message);
        }
    }

    private static void ApplyPending(
        PendingSync pending,
        UnitSheetTable player,
        UnitSheetTable enemy,
        IReadOnlyList<UnitSheetTable.Column> schema)
    {
        applying = true;
        try
        {
            UnitSheetAssetApplier.Apply(player, enemy, schema);
            SessionState.EraseString(PendingKey);
        }
        finally
        {
            applying = false;
        }
    }

    private static void ValidateReservedMembers(IEnumerable<UnitSheetTable.Column> schema)
    {
        foreach (UnitSheetTable.Column column in schema)
        {
            if (column.IsUpgradeLink) continue;
            if (column.Name == "nextUpgradeUnits" || column.Name == "unitSprite" || column.Name == "GetNextUpgradeUnit")
                throw new FormatException($"{column.Name}: UnitDataSO가 직접 관리하는 이름이라 시트 변수명으로 사용할 수 없습니다.");
        }
    }
}

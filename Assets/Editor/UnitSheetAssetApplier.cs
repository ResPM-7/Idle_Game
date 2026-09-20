using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal static class UnitSheetAssetApplier
{
    private const string PlayerDataPath = "Assets/03.Data/Units/PlayerUnitData.asset";
    private const string EnemyDataPath = "Assets/03.Data/Units/EnemyUnitData.asset";

    internal static void Apply(
        UnitSheetTable player,
        UnitSheetTable enemy,
        IReadOnlyList<UnitSheetTable.Column> schema)
    {
        ApplyTableToDatabase(player, schema, PlayerDataPath);
        ApplyTableToDatabase(enemy, schema, EnemyDataPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[유닛 시트] 통합 데이터 동기화 완료: 아군 {player.Rows.Count}개, 적군 {enemy.Rows.Count}개");
    }

    internal static bool RuntimeSchemaMatches(IReadOnlyList<UnitSheetTable.Column> schema)
    {
        foreach (UnitSheetTable.Column column in schema)
        {
            if (column.IsUpgradeLink) continue;
            FieldInfo field = typeof(UnitData).GetField(column.Name, BindingFlags.Public | BindingFlags.Instance);
            if (field == null || field.FieldType != column.Type) return false;
        }
        return true;
    }

    private static void ApplyTableToDatabase(
        UnitSheetTable table,
        IReadOnlyList<UnitSheetTable.Column> schema,
        string assetPath)
    {
        UnitDataSO database = AssetDatabase.LoadAssetAtPath<UnitDataSO>(assetPath);
        if (database == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
                throw new InvalidOperationException($"{assetPath}에 다른 에셋이 있어서 덮어쓰지 않았습니다.");
            database = ScriptableObject.CreateInstance<UnitDataSO>();
            AssetDatabase.CreateAsset(database, assetPath);
        }

        Dictionary<int, UnitData> existing = database.Units
            .Where(unit => unit != null)
            .GroupBy(unit => unit.unitId)
            .ToDictionary(group => group.Key, group => group.First());
        List<UnitData> imported = new List<UnitData>();
        UnitSheetTable.Column linkColumn = table.Columns.FirstOrDefault(column => column.IsUpgradeLink);

        foreach (UnitSheetTable.Row row in table.Rows)
        {
            UnitData target = existing.TryGetValue(row.Id, out UnitData oldData)
                ? oldData
                : new UnitData();

            foreach (UnitSheetTable.Column column in schema)
            {
                if (column.IsUpgradeLink) continue;
                FieldInfo field = typeof(UnitData).GetField(column.Name, BindingFlags.Public | BindingFlags.Instance);
                if (field == null || field.FieldType != column.Type)
                    throw new InvalidOperationException($"UnitData.{column.Name} 필드가 컴파일 결과와 일치하지 않습니다.");
                object value = row.Cells.TryGetValue(column.Name, out string cell)
                    ? UnitSheetTable.ParseValue(cell, column.Type)
                    : DefaultValue(column.Type);
                field.SetValue(target, value);
            }

            target.nextUpgradeUnitIds = linkColumn == null
                ? Array.Empty<int>()
                : UnitSheetTable.ParseUpgradeIds(row.Cells[linkColumn.Name]);
            imported.Add(target);
        }

        database.SetUnits(imported);
        EditorUtility.SetDirty(database);
    }

    private static object DefaultValue(Type type)
    {
        if (type == typeof(string)) return string.Empty;
        if (type.IsArray) return Array.CreateInstance(type.GetElementType(), 0);
        return Activator.CreateInstance(type);
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

internal static class UnitDataSOScriptUpdater
{
    internal const string AssetPath = "Assets/01.Scripts/KimJiWoong/Data/UnitDataSO.cs";
    private const string StartMarker = "    // <sheet-fields>";
    private const string EndMarker = "    // </sheet-fields>";

    internal static bool UpdateFields(IReadOnlyList<UnitSheetTable.Column> schema)
    {
        string absolutePath = Path.Combine(Application.dataPath, AssetPath.Substring("Assets/".Length));
        string original = File.ReadAllText(absolutePath);
        int start = original.IndexOf(StartMarker, StringComparison.Ordinal);
        int end = original.IndexOf(EndMarker, StringComparison.Ordinal);

        if (start < 0 || end <= start ||
            original.IndexOf(StartMarker, start + StartMarker.Length, StringComparison.Ordinal) >= 0 ||
            original.IndexOf(EndMarker, end + EndMarker.Length, StringComparison.Ordinal) >= 0)
            throw new InvalidOperationException("UnitDataSO.cs의 sheet-fields 표시를 찾을 수 없거나 중복됩니다.");

        string newline = original.Contains("\r\n") ? "\r\n" : "\n";
        StringBuilder generated = new StringBuilder(StartMarker).Append(newline);
        foreach (UnitSheetTable.Column column in schema)
        {
            if (column.IsUpgradeLink) continue;
            generated.Append("    public ")
                .Append(column.TypeName)
                .Append(" @")
                .Append(column.Name)
                .Append(';')
                .Append(newline);
        }

        string updated = original.Substring(0, start) + generated + original.Substring(end);
        if (updated == original) return false;
        File.WriteAllText(absolutePath, updated, new UTF8Encoding(false));
        return true;
    }
}

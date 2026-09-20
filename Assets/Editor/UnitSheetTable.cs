using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

internal sealed class UnitSheetTable
{
    internal sealed class Column
    {
        public string Name;
        public string TypeName;
        public Type Type;
        public int Index;
        public bool IsUpgradeLink;
    }

    internal sealed class Row
    {
        public int Id;
        public readonly Dictionary<string, string> Cells = new Dictionary<string, string>();
    }

    internal readonly List<Column> Columns = new List<Column>();
    internal readonly List<Row> Rows = new List<Row>();

    private static readonly Dictionary<string, Type> SupportedTypes = new Dictionary<string, Type>
    {
        { "int", typeof(int) },
        { "float", typeof(float) },
        { "string", typeof(string) },
        { "bool", typeof(bool) },
        { "long", typeof(long) },
        { "double", typeof(double) }
    };

    internal static UnitSheetTable Parse(string csv, string sheetName)
    {
        List<List<string>> records = ParseCsv(csv);
        if (records.Count < 3)
            throw new FormatException($"{sheetName}: 1행 변수명, 2행 자료형, 3행부터 데이터가 필요합니다.");

        UnitSheetTable table = new UnitSheetTable();
        HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0; index < records[0].Count; index++)
        {
            string name = records[0][index].Trim().TrimStart('\uFEFF');
            if (string.IsNullOrEmpty(name))
            {
                if (records.Skip(1).Any(row => index < row.Count && !string.IsNullOrWhiteSpace(row[index])))
                    throw new FormatException($"{sheetName}: {index + 1}열에 값이 있지만 변수명이 없습니다.");
                continue;
            }

            ValidateFieldName(name, sheetName);
            if (!names.Add(name))
                throw new FormatException($"{sheetName}: 변수명 '{name}'이 중복됩니다.");

            string rawType = index < records[1].Count ? records[1][index] : string.Empty;
            bool isUpgradeLink = name == "nextUpgradeUnitId" || name == "nextUpgradeUnitIds" || name == "nextUpgradeUnits";
            string typeName = NormalizeType(rawType, isUpgradeLink);
            Type type = ResolveType(typeName);

            table.Columns.Add(new Column
            {
                Name = name,
                TypeName = typeName,
                Type = type,
                Index = index,
                IsUpgradeLink = isUpgradeLink
            });
        }

        Column idColumn = table.Columns.Find(column => column.Name == "unitId");
        if (idColumn == null || idColumn.Type != typeof(int))
            throw new FormatException($"{sheetName}: unitId의 자료형은 int여야 합니다.");
        if (table.Columns.Count(column => column.IsUpgradeLink) > 1)
            throw new FormatException($"{sheetName}: 진화 열은 nextUpgradeUnitIds 또는 nextUpgradeUnits 중 하나만 사용하세요.");

        HashSet<int> ids = new HashSet<int>();
        for (int rowIndex = 2; rowIndex < records.Count; rowIndex++)
        {
            List<string> cells = records[rowIndex];
            if (cells.All(string.IsNullOrWhiteSpace)) continue;
            if (cells.Skip(records[0].Count).Any(cell => !string.IsNullOrWhiteSpace(cell)))
                throw new FormatException($"{sheetName} {rowIndex + 1}행: 헤더보다 데이터 열이 많습니다.");

            Row row = new Row();
            foreach (Column column in table.Columns)
            {
                string cell = column.Index < cells.Count ? cells[column.Index] : string.Empty;
                row.Cells[column.Name] = cell;
                try
                {
                    if (column.IsUpgradeLink) ParseUpgradeIds(cell);
                    else ParseValue(cell, column.Type);
                }
                catch (Exception exception) when (exception is FormatException || exception is OverflowException)
                {
                    throw new FormatException($"{sheetName} {rowIndex + 1}행 {column.Name}: '{cell}' 값을 {column.TypeName}으로 읽을 수 없습니다.");
                }
            }

            object parsedId = ParseValue(row.Cells["unitId"], typeof(int));
            row.Id = (int)parsedId;
            if (row.Id <= 0 || !ids.Add(row.Id))
                throw new FormatException($"{sheetName} {rowIndex + 1}행: unitId {row.Id}가 잘못됐거나 중복됐습니다.");
            table.Rows.Add(row);
        }

        if (table.Rows.Count == 0)
            throw new FormatException($"{sheetName}: 적용할 데이터 행이 없습니다.");
        return table;
    }

    internal static List<Column> MergeSchema(UnitSheetTable player, UnitSheetTable enemy)
    {
        List<Column> result = new List<Column>();
        foreach (Column column in player.Columns.Concat(enemy.Columns))
        {
            Column existing = result.Find(item => item.Name == column.Name);
            if (existing == null)
            {
                result.Add(column);
                continue;
            }
            if (existing.TypeName != column.TypeName || existing.IsUpgradeLink != column.IsUpgradeLink)
                throw new FormatException($"{column.Name}: UnitData와 EnemyData의 자료형이 다릅니다 ({existing.TypeName} / {column.TypeName}).");
        }
        return result;
    }

    internal static object ParseValue(string text, Type type)
    {
        if (type == typeof(string)) return text ?? string.Empty;
        text = (text ?? string.Empty).Trim();

        if (type.IsArray)
        {
            Type elementType = type.GetElementType();
            if (text.StartsWith("[") && text.EndsWith("]"))
                text = text.Substring(1, text.Length - 2);
            char[] separators = elementType == typeof(string)
                ? new[] { ',', ';', '|' }
                : new[] { ',', ';', '|', '/', ' ', '\t', '\r', '\n' };
            string[] parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            Array values = Array.CreateInstance(elementType, parts.Length);
            for (int i = 0; i < parts.Length; i++)
                values.SetValue(ParseValue(parts[i].Trim(), elementType), i);
            return values;
        }

        if (text.Length == 0) return Activator.CreateInstance(type);
        if (type == typeof(bool))
        {
            if (text == "1") return true;
            if (text == "0") return false;
            return bool.Parse(text);
        }

        object value = Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
        if (value is float floatValue && (float.IsNaN(floatValue) || float.IsInfinity(floatValue)))
            throw new FormatException("유한한 숫자만 사용할 수 있습니다.");
        if (value is double doubleValue && (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue)))
            throw new FormatException("유한한 숫자만 사용할 수 있습니다.");
        return value;
    }

    internal static int[] ParseUpgradeIds(string text)
    {
        string cleaned = (text ?? string.Empty).Trim().Trim('[', ']');
        if (cleaned.Length == 0) return Array.Empty<int>();
        string[] parts = cleaned.Split(new[] { ',', ';', '|', '/', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int[] result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            result[i] = int.Parse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture);
        return result;
    }

    private static string NormalizeType(string rawType, bool isUpgradeLink)
    {
        string compact = new string((rawType ?? string.Empty).Where(character => !char.IsWhiteSpace(character)).ToArray());
        string[] chips = compact.Split(',');
        if (chips.Length == 2 && chips.Count(chip => chip == "[]") == 1)
            compact = chips.Single(chip => chip != "[]") + "[]";
        compact = compact.ToLowerInvariant();

        if (isUpgradeLink && (compact == "unitdataso" || compact == "unitdataso[]" || compact == "int" || compact == "int[]"))
            return "int[]";

        bool isArray = compact.EndsWith("[]", StringComparison.Ordinal);
        string scalar = isArray ? compact.Substring(0, compact.Length - 2) : compact;
        if (!SupportedTypes.ContainsKey(scalar))
            throw new FormatException($"지원하지 않는 자료형입니다: '{rawType}'");
        return scalar + (isArray ? "[]" : string.Empty);
    }

    private static Type ResolveType(string typeName)
    {
        bool isArray = typeName.EndsWith("[]", StringComparison.Ordinal);
        string scalar = isArray ? typeName.Substring(0, typeName.Length - 2) : typeName;
        Type type = SupportedTypes[scalar];
        return isArray ? type.MakeArrayType() : type;
    }

    private static void ValidateFieldName(string name, string sheetName)
    {
        if (!(char.IsLetter(name[0]) || name[0] == '_') || name.Any(character => !(char.IsLetterOrDigit(character) || character == '_')))
            throw new FormatException($"{sheetName}: '{name}'은 C# 변수명으로 사용할 수 없습니다.");
        if (name == "unitSprite")
            throw new FormatException($"{sheetName}: unitSprite는 Unity 에셋 참조용 수동 필드이므로 시트 변수명으로 사용할 수 없습니다.");
    }

    private static List<List<string>> ParseCsv(string csv)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> row = new List<string>();
        StringBuilder cell = new StringBuilder();
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
            else if ((character == '\r' || character == '\n') && !quoted)
            {
                if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                row.Add(cell.ToString());
                cell.Clear();
                rows.Add(row);
                row = new List<string>();
            }
            else cell.Append(character);
        }

        if (quoted) throw new FormatException("CSV의 큰따옴표가 닫히지 않았습니다.");
        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            rows.Add(row);
        }
        return rows;
    }
}

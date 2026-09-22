using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GameDataTools
{
    public const string DatabasePath = "Assets/Resources/GameData/UnitDatabase.asset";

    [MenuItem("Tools/Game Data/Rebuild Unit Database")]
    public static void Rebuild()
    {
        Directory.CreateDirectory("Assets/Resources/GameData");
        AssetDatabase.Refresh();
        var db = AssetDatabase.LoadAssetAtPath<UnitDatabase>(DatabasePath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<UnitDatabase>();
            AssetDatabase.CreateAsset(db, DatabasePath);
        }
        db.players.Clear(); db.enemies.Clear();
        foreach (string guid in AssetDatabase.FindAssets("t:UnitDataSO", new[] { "Assets/03.Data/Units" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (var unit in AssetDatabase.LoadAllAssetsAtPath(path).OfType<UnitDataSO>())
            {
                bool enemy = path.Contains("/Enemy/") || path.EndsWith("/Enemy.asset");
                (enemy ? db.enemies : db.players).Add(unit);
            }
        }
        db.players.Sort((a, b) => a.unitId.CompareTo(b.unitId));
        db.enemies.Sort((a, b) => a.unitId.CompareTo(b.unitId));
        db.RebuildIndex();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log($"UnitDatabase: 아군 {db.players.Count}, 적 {db.enemies.Count}");
    }

    // 기존 참조를 보존하기 위해 복제 → 참조 교체 → 원본 백업 순서로 실행합니다.
    // 백업은 Assets 밖에 있으므로 Unity에서 중복 데이터로 읽히지 않습니다.
    [MenuItem("Tools/Game Data/Consolidate Unit Assets")]
    public static void Consolidate()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("플레이 모드에서는 변환할 수 없습니다.");
        Rebuild();
        var db = AssetDatabase.LoadAssetAtPath<UnitDatabase>(DatabasePath);
        var originals = db.players.Concat(db.enemies).ToArray();
        var replacements = new Dictionary<string, string>();
        var sourcePaths = new List<string>();
        string backup = Path.Combine("DataMigrationBackup", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        foreach (bool enemy in new[] { false, true })
        {
            string target = "Assets/03.Data/Units/" + (enemy ? "Enemy.asset" : "Player.asset");
            if (AssetDatabase.LoadMainAssetAtPath(target) != null) continue;
            var group = ScriptableObject.CreateInstance<UnitDatabase>();
            AssetDatabase.CreateAsset(group, target);
            foreach (var original in enemy ? db.enemies : db.players)
            {
                string source = AssetDatabase.GetAssetPath(original);
                var clone = UnityEngine.Object.Instantiate(original);
                clone.name = original.name;
                AssetDatabase.AddObjectToAsset(clone, group);
                (enemy ? group.enemies : group.players).Add(clone);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(original, out string oldGuid, out long oldId);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clone, out string newGuid, out long newId);
                replacements.Add($"fileID: {oldId}, guid: {oldGuid}, type: 2", $"fileID: {newId}, guid: {newGuid}, type: 2");
                sourcePaths.Add(source);
            }
            EditorUtility.SetDirty(group);
        }
        AssetDatabase.SaveAssets();
        if (replacements.Count == 0) return;
        // Unity 텍스트 직렬화 참조만 치환합니다. 씬/프리팹/진화 참조를 모두 포함합니다.
        foreach (string file in Directory.GetFiles("Assets", "*", SearchOption.AllDirectories))
        {
            if (!new[] { ".asset", ".prefab", ".unity" }.Contains(Path.GetExtension(file))) continue;
            string text = File.ReadAllText(file);
            if (!text.StartsWith("%YAML")) continue;
            string changed = text;
            foreach (var pair in replacements) changed = changed.Replace(pair.Key, pair.Value);
            if (changed == text) continue;
            Backup(file, backup);
            File.WriteAllText(file, changed);
        }
        foreach (string source in sourcePaths.Distinct())
        {
            Backup(source, backup); Backup(source + ".meta", backup);
            AssetDatabase.DeleteAsset(source);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Rebuild();
        Debug.Log("통합 완료. 이전 파일/참조 백업: " + Path.GetFullPath(backup));
    }

    private static void Backup(string source, string root)
    {
        string target = Path.Combine(root, source);
        if (File.Exists(target)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        File.Copy(source, target);
    }

    [MenuItem("Tools/Game Data/Save Play Progress")]
    public static void Save() { if (GameSaveService.Instance != null) GameSaveService.Instance.SaveNow(); }
    [MenuItem("Tools/Game Data/Load Play Progress")]
    public static void Load() { if (GameSaveService.Instance != null) GameSaveService.Instance.LoadNow(); }
    [MenuItem("Tools/Game Data/Open Save Folder")]
    public static void OpenFolder() { EditorUtility.RevealInFinder(Application.persistentDataPath); }
}

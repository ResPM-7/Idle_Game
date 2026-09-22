using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameDataTools
{
    public const string DatabasePath = "Assets/Resources/GameData/UnitDatabase.asset";

    // 스크립트 재컴파일 후 현재 씬에 배치된 서비스가 아직 비어 있을 때만 한 번 연결합니다.
    // 연결 결과는 씬에 직렬화되므로 게임 실행 중에는 Find를 사용하지 않습니다.
    [InitializeOnLoadMethod]
    private static void ScheduleMissingSaveServiceReferences()
    {
        EditorApplication.delayCall += BindMissingSaveServiceReferences;
    }

    private static void BindMissingSaveServiceReferences()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var service = UnityEngine.Object.FindFirstObjectByType<GameSaveService>();
        if (service == null || !service.gameObject.scene.IsValid()) return;

        var serialized = new SerializedObject(service);
        if (serialized.FindProperty("database").objectReferenceValue != null &&
            serialized.FindProperty("spawner").objectReferenceValue != null &&
            serialized.FindProperty("barracks").objectReferenceValue != null &&
            serialized.FindProperty("moneyManager").objectReferenceValue != null &&
            serialized.FindProperty("waveManager").objectReferenceValue != null &&
            serialized.FindProperty("barracksManager").objectReferenceValue != null &&
            serialized.FindProperty("gridUnitFactory").objectReferenceValue != null &&
            serialized.FindProperty("objectPoolManager").objectReferenceValue != null &&
            serialized.FindProperty("battleSlotPanel").objectReferenceValue != null &&
            serialized.FindProperty("party").arraySize > 0)
            return;

        try { BindSaveServiceReferences(); }
        catch (Exception ex) { Debug.LogWarning("GameSaveService 참조 자동 연결 대기: " + ex.Message); }
    }

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

    [MenuItem("Tools/Game Data/Bind Save Service References")]
    public static void BindSaveServiceReferences()
    {
        var service = UnityEngine.Object.FindFirstObjectByType<GameSaveService>();
        if (service == null)
            throw new InvalidOperationException("현재 씬에서 GameSaveService를 찾지 못했습니다.");

        var serialized = new SerializedObject(service);
        SetReference<UnitDatabase>(serialized, "database",
            AssetDatabase.LoadAssetAtPath<UnitDatabase>(DatabasePath));
        SetReference<UnitSpawner>(serialized, "spawner");
        SetReference<BarracksSystem>(serialized, "barracks");
        SetReference<MoneyManager>(serialized, "moneyManager");
        SetReference<WaveManager>(serialized, "waveManager");
        SetReference<BarracksManager>(serialized, "barracksManager");
        SetReference<GridUnitFactory>(serialized, "gridUnitFactory");
        SetReference<ObjectPoolManager>(serialized, "objectPoolManager");
        BattleSlotPanel panel = SetReference<BattleSlotPanel>(serialized, "battleSlotPanel");

        BattleSlotUI[] slots = panel.GetComponentsInChildren<BattleSlotUI>(true)
            .OrderBy(slot => slot.slotIndex).ToArray();
        SerializedProperty party = serialized.FindProperty("party");
        party.arraySize = slots.Length;
        for (int i = 0; i < slots.Length; i++)
            party.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];

        serialized.FindProperty("isDontDestroy").boolValue = false;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(service);
        EditorSceneManager.MarkSceneDirty(service.gameObject.scene);
        EditorSceneManager.SaveScene(service.gameObject.scene);
        Debug.Log($"GameSaveService 참조 연결 완료: 파티 슬롯 {slots.Length}개");
    }

    // 자동 검증이나 CI에서도 동일한 연결 작업을 실행할 수 있도록 MainScene을 먼저 엽니다.
    public static void BindSaveServiceReferencesInMainScene()
    {
        EditorSceneManager.OpenScene("Assets/00.Scenes/MainScene.unity");
        BindSaveServiceReferences();
    }

    private static T SetReference<T>(SerializedObject serialized, string field, T value = null)
        where T : UnityEngine.Object
    {
        if (value == null) value = UnityEngine.Object.FindFirstObjectByType<T>();
        if (value == null) throw new InvalidOperationException(field + " 참조 대상을 찾지 못했습니다.");
        serialized.FindProperty(field).objectReferenceValue = value;
        return value;
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
    public static void Save() { if (GameSaveService.instance != null) GameSaveService.instance.SaveNow(); }
    [MenuItem("Tools/Game Data/Load Play Progress")]
    public static void Load() { if (GameSaveService.instance != null) GameSaveService.instance.LoadNow(); }
    [MenuItem("Tools/Game Data/Open Save Folder")]
    public static void OpenFolder() { EditorUtility.RevealInFinder(Application.persistentDataPath); }
}

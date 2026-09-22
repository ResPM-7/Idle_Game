using UnityEditor;
using UnityEngine;

// 통합 파일에서 유닛을 선택하면 세부 필드를 바로 수정할 수 있는 관리 화면입니다.
[CustomEditor(typeof(UnitDatabase))]
public class UnitDatabaseEditor : Editor
{
    private UnitDataSO selected;
    private Editor unitEditor;
    private string query = "";

    public override void OnInspectorGUI()
    {
        var db = (UnitDatabase)target;
        query = EditorGUILayout.TextField("ID / 이름 검색", query);
        foreach (var unit in db.players) Row(unit);
        foreach (var unit in db.enemies) Row(unit);
        if (selected != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("선택한 유닛", EditorStyles.boldLabel);
            CreateCachedEditor(selected, null, ref unitEditor);
            unitEditor.OnInspectorGUI();
        }
        EditorGUILayout.HelpBox("새 유닛은 구글 시트 동기화로 추가합니다. ID는 저장 데이터의 식별자이므로 출시 후 변경하지 마세요.", MessageType.Info);
        EditorGUILayout.Space(12);
        string destination = db.players.Count > 0 && db.enemies.Count > 0
            ? "UnitData + EnemyData" : db.enemies.Count > 0 ? "EnemyData" : "UnitData";
        EditorGUILayout.LabelField("구글 시트 업로드", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox($"{destination} 탭에 이 데이터베이스의 전체 목록을 전송합니다. 기존 시트에 있는 16개 열만 업로드합니다.", MessageType.Info);
        using (new EditorGUI.DisabledScope(GoogleSheetUnitUploader.IsUploading || EditorApplication.isPlaying))
        {
            if (GUILayout.Button($"변경 내용 시트로 업로드 ({destination})", GUILayout.Height(32)))
                GoogleSheetUnitUploader.UploadDatabase(db);
        }
        if (GUILayout.Button("시트 업로드 연결 설정")) GoogleSheetUploadSettingsWindow.Open();
        if (!string.IsNullOrEmpty(GoogleSheetUnitUploader.Status))
            EditorGUILayout.HelpBox(GoogleSheetUnitUploader.Status, MessageType.Info);
    }

    private void Row(UnitDataSO unit)
    {
        if (unit == null) return;
        string label = $"{unit.unitId}  Lv.{unit.unitLevel}  {unit.unitName}";
        if (query.Length > 0 && label.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) < 0) return;
        if (GUILayout.Button(label)) selected = unit;
    }
    private void OnDisable() { if (unitEditor != null) DestroyImmediate(unitEditor); }
    public override bool RequiresConstantRepaint() => GoogleSheetUnitUploader.IsUploading;
}

using System;
using UnityEngine;

// 아군/적은 같은 ID를 쓸 수 있으므로 팀과 ID를 함께 비교합니다. UI 데이터는 사용하지 않습니다.
[CreateAssetMenu(menuName = "Data/Battle Visual Catalog")]
public class BattleVisualCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public Team_Test team;
        public int unitId;
        public GameObject prefab;
        [Min(0.01f)] public float scale = 1f;
        public Vector3 offset = new Vector3(0f, -0.45f, 0f);
        public bool facesLeft = true;
        [Min(0)] public int attackAnimation;
    }

    public Entry[] entries = Array.Empty<Entry>();
    public Entry Find(UnitDataSO data)
    {
        foreach (var entry in entries)
            if (entry != null && entry.team == data.team && entry.unitId == data.unitId)
                return entry;
        return null;
    }
}

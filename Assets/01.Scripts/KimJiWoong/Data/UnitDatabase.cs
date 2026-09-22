using System;
using System.Collections.Generic;
using UnityEngine;

// 목록은 인스펙터/직렬화용, 딕셔너리는 실행 중 ID 검색용입니다.
[CreateAssetMenu(menuName = "Data/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public List<UnitDataSO> players = new List<UnitDataSO>();
    public List<UnitDataSO> enemies = new List<UnitDataSO>();
    private Dictionary<int, UnitDataSO> playerIndex;
    private Dictionary<int, UnitDataSO> enemyIndex;

    public void RebuildIndex()
    {
        playerIndex = Build(players);
        enemyIndex = Build(enemies);
    }

    private static Dictionary<int, UnitDataSO> Build(List<UnitDataSO> list)
    {
        var result = new Dictionary<int, UnitDataSO>();
        foreach (var unit in list)
        {
            if (unit == null || unit.unitId <= 0 || result.ContainsKey(unit.unitId))
                throw new InvalidOperationException("유닛 DB에 빈 데이터 또는 중복/잘못된 ID가 있습니다.");
            result.Add(unit.unitId, unit);
        }
        return result;
    }

    public UnitDataSO Find(int id, bool enemy = false)
    {
        if (playerIndex == null || enemyIndex == null) RebuildIndex();
        (enemy ? enemyIndex : playerIndex).TryGetValue(id, out var unit);
        return unit;
    }

    private void OnValidate() { playerIndex = null; enemyIndex = null; }
}

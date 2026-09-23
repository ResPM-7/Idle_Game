using System.Collections.Generic;
using UnityEngine;

/// <summary>활성 전투 유닛을 등록하여 탐색 때마다 물리 검색 배열을 만들지 않습니다.</summary>
public static class UnitTargetRegistry
{
    // 일반 전투 규모는 미리 확보합니다. 초과 시 누락하지 않고 목록을 확장합니다.
    private static readonly List<Unit_Base_Test> Units = new List<Unit_Base_Test>(256);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Units.Clear();

    public static void Register(Unit_Base_Test unit)
    {
        if (!Units.Contains(unit)) Units.Add(unit);
    }
    public static void Unregister(Unit_Base_Test unit) => Units.Remove(unit);

    public static Unit_Base_Test FindClosest(Unit_Base_Test owner, UnitCombatAction action)
    {
        float radius = Mathf.Max(0f, owner.MyData.searchRange);
        float bestDistance = radius * radius;
        Vector2 origin = owner.transform.position;
        Unit_Base_Test best = null;
        // LINQ, 임시 배열, 정렬 없이 루트 위치의 제곱 거리로 비교합니다.
        for (int i = 0; i < Units.Count; i++)
        {
            Unit_Base_Test candidate = Units[i];
            if (!action.CanTarget(owner, candidate)) continue;
            float distance = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }
        return best;
    }
}

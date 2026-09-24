using System;
using UnityEngine;

/// <summary>데이터로 자식 행동을 선택하고 타깃과 재사용 가능한 탐색 배열을 보관합니다.</summary>
public sealed class UnitCombatController
{
    private static readonly UnitCombatAction Melee = new MeleeCombatAction();
    private static readonly UnitCombatAction Ranged = new RangedCombatAction();
    private static readonly UnitCombatAction Heal = new HealCombatAction();
    private readonly Unit_Base_Test owner;
    private Collider2D[] hits;
    private Unit_Base_Test target;
    private uint targetVersion;
    private UnitCombatAction attack, healing, selected;
    public Transform Target => target != null ? target.transform : null;
    public UnitCombatAction SelectedAction => selected;

    public UnitCombatController(Unit_Base_Test unit, int capacity)
    {
        owner = unit;
        hits = new Collider2D[Mathf.Max(8, capacity)];
    }
    public void Configure()
    {
        UnitDataSO data = owner.MyData;
        healing = data.canHeal ? Heal : null;
        // 기존 정책 유지: 근접 우선, 회복할 아군이 없는 힐러는 원거리 공격합니다.
        attack = data.canMelee ? Melee : (data.canRanged || data.canHeal ? Ranged : null);
        ClearTarget();
    }
    public void ClearTarget() { target = null; selected = null; targetVersion = 0; }
    public void Reset()
    {
        ClearTarget();
        Array.Clear(hits, 0, hits.Length);
    }
    public void SetTarget(Transform value)
    {
        ClearTarget();
        if (value == null || !value.TryGetComponent<Unit_Base_Test>(out var unit)) return;
        UnitCombatAction action = healing != null && healing.CanTarget(owner, unit) ? healing : attack;
        if (action != null && action.CanTarget(owner, unit)) Assign(unit, action);
    }
    private void Assign(Unit_Base_Test value, UnitCombatAction action)
    {
        target = value;
        targetVersion = value.SpawnVersion;
        selected = action;
    }
    public bool FindTarget()
    {
        ClearTarget();
        if (healing != null)
        {
            Unit_Base_Test ally = FindClosest(healing);
            if (ally != null) { Assign(ally, healing); return true; }
        }
        if (attack == null) return false;
        Unit_Base_Test enemy = FindClosest(attack);
        if (enemy == null) return false;
        Assign(enemy, attack);
        return true;
    }
    private Unit_Base_Test FindClosest(UnitCombatAction action)
    {
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        filter.SetLayerMask(action.GetTargetLayer(owner));
        Vector2 position = owner.transform.position;
        float radius = Mathf.Max(0f, owner.MyData.searchRange);
        int count;
        // 배열이 부족할 때만 확장하고 다시 조회하여 후보를 조용히 누락하지 않습니다.
        while ((count = Physics2D.OverlapCircle(position, radius, filter, hits)) == hits.Length)
            Array.Resize(ref hits, checked(hits.Length * 2));
        Unit_Base_Test best = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hits[i];
            hits[i] = null;
            if (hit == null || !hit.TryGetComponent<Unit_Base_Test>(out var candidate)
                || !action.CanTarget(owner, candidate)) continue;
            float distance = ((Vector2)candidate.transform.position - position).sqrMagnitude;
            if (distance < bestDistance) { bestDistance = distance; best = candidate; }
        }
        return best;
    }
    public bool HasValidTarget => selected != null && target != null
        && target.SpawnVersion == targetVersion && selected.CanTarget(owner, target);
    public bool IsInRange()
    {
        if (!HasValidTarget) return false;
        float range = Mathf.Max(0f, selected.GetRange(owner));
        return ((Vector2)target.transform.position - (Vector2)owner.transform.position).sqrMagnitude <= range * range;
    }
    public void Execute()
    {
        if (!IsInRange()) return;
        float amount = owner.CurrentDamage;
        bool critical = UnityEngine.Random.Range(0, 100) < owner.CurrentCriticalRate;
        if (critical) amount *= owner.CurrentCriticalDamage;
        selected.Execute(owner, target, amount, critical);
    }
}

using UnityEngine;

/// <summary>데이터의 역할 플래그로 행동을 선택합니다. 유닛마다 한 번 생성하고 풀 재사용 시 초기화합니다.</summary>
public sealed class UnitCombatController
{
    private static readonly UnitCombatAction Melee = new MeleeCombatAction();
    private static readonly UnitCombatAction Ranged = new RangedCombatAction();
    private static readonly UnitCombatAction Heal = new HealCombatAction();
    private Unit_Base_Test owner;
    private Unit_Base_Test target;
    private uint targetVersion;
    private UnitCombatAction attack;
    private UnitCombatAction healing;
    private UnitCombatAction selected;
    public UnitCombatAction SelectedAction => selected;
    public Transform Target => target != null ? target.transform : null;
    public float StopDistance => selected != null ? Mathf.Max(0f, selected.GetRange(owner)) : 0f;

    public void Configure(Unit_Base_Test unit)
    {
        owner = unit;
        UnitDataSO data = unit.MyData;
        healing = data.canHeal ? Heal : null;
        // 기존 규칙 유지: 근접 우선, 회복할 아군이 없는 힐러는 원거리 공격합니다.
        attack = data.canMelee ? Melee : (data.canRanged || data.canHeal ? Ranged : null);
        ClearTarget();
    }

    public void ClearTarget() { target = null; selected = null; targetVersion = 0; }

    // 기존 CurrentTarget 속성에 접근하는 코드도 호환합니다. 대상 변경 시에만 컴포넌트를 확인합니다.
    public void SetTarget(Transform value)
    {
        ClearTarget();
        if (value == null || owner == null || !value.TryGetComponent<Unit_Base_Test>(out var unit)) return;
        UnitCombatAction action = healing != null && healing.CanTarget(owner, unit) ? healing : attack;
        if (action == null || !action.CanTarget(owner, unit)) return;
        Assign(unit, action);
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
            Unit_Base_Test ally = UnitTargetRegistry.FindClosest(owner, healing);
            if (ally != null) { Assign(ally, healing); return true; }
        }
        if (attack == null) return false;
        Unit_Base_Test enemy = UnitTargetRegistry.FindClosest(owner, attack);
        if (enemy == null) return false;
        Assign(enemy, attack);
        return true;
    }

    public bool HasValidTarget => selected != null && target != null
        && target.SpawnVersion == targetVersion && selected.CanTarget(owner, target);

    public bool IsInRange()
    {
        if (!HasValidTarget) return false;
        float range = StopDistance;
        return ((Vector2)target.transform.position - (Vector2)owner.transform.position).sqrMagnitude <= range * range;
    }

    public void Execute()
    {
        if (!IsInRange()) return;
        float amount = owner.CurrentDamage;
        bool critical = Random.Range(0f, 100f) < owner.CurrentCriticalRate;
        if (critical) amount *= owner.CurrentCriticalDamage;
        selected.Execute(owner, target, amount, critical);
    }
}

using UnityEngine;

public class UnitMoveState : IUnitState
{
    public void Enter(Unit_Base_Test unit) => unit.SetMoveDirection(Vector2.zero);
    public void Exit(Unit_Base_Test unit) => unit.SetMoveDirection(Vector2.zero);
    public void Execute(Unit_Base_Test unit)
    {
        unit.AttackTimer += Time.deltaTime;
        unit.SetMoveDirection(Vector2.zero);
        if (!unit.Combat.HasValidTarget)
        {
            unit.CurrentTarget = null;
            unit.ChangeState(unit.idleState);
            return;
        }
        // 사거리와 대상 조건은 선택된 근접·원거리·회복 행동이 담당합니다.
        if (unit.Combat.IsInRange())
        {
            if (unit.AttackTimer >= unit.CurrentAttackSpeed) unit.ChangeState(unit.attackState);
        }
        else
        {
            Vector2 direction = unit.CurrentTarget.position - unit.transform.position;
            unit.SetMoveDirection(direction.normalized);
        }
    }
}

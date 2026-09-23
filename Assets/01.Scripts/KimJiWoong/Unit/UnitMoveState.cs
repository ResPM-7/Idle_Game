using UnityEngine;

public class UnitMoveState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.SetMoveDirection(Vector2.zero);
    }

    public void Exit(Unit_Base_Test unit)
    {
        unit.SetMoveDirection(Vector2.zero);
    }

    public void Execute(Unit_Base_Test unit)
    {
        unit.AttackTimer += Time.deltaTime;
        unit.SetMoveDirection(Vector2.zero);

        if (unit.CurrentTarget == null || unit.CurrentTarget.GetComponent<Unit_Base_Test>().CurrentHp <= 0)
        {
            unit.ChangeState(unit.idleState);
            return;
        }

        bool isTargetAlly = ((1 << unit.CurrentTarget.gameObject.layer) & unit.AllyLayer.value) != 0;

        // 타겟이 아군이면 healRange, 적이면 attackRange를 정지 거리로 사용
        float stopDistance = isTargetAlly ? unit.MyData.healRange : unit.MyData.attackRange;

        float dist = Vector2.Distance(unit.transform.position, unit.CurrentTarget.position);

        if (dist <= stopDistance)
        {
            if (unit.AttackTimer >= unit.CurrentAttackSpeed)
            {
                unit.ChangeState(unit.attackState);
            }
        }
        else
        {
            Vector2 dir = (unit.CurrentTarget.position - unit.transform.position).normalized;
            // 실제 이동은 Unit_Base_Test.FixedUpdate에서 Rigidbody2D로 처리합니다.
            unit.SetMoveDirection(dir);
        }
    }
}

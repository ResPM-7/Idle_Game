using UnityEngine;

public class UnitMoveState : IUnitState
{
    public void Enter(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }

    public void Execute(Unit_Base_Test unit)
    {
        unit.AttackTimer += Time.deltaTime;

        if (unit.CurrentTarget == null || unit.CurrentTarget.GetComponent<Unit_Base_Test>().CurrentHp <= 0)
        {
            unit.ChangeState(unit.idleState);
            return;
        }

        bool isTargetAlly = ((1 << unit.CurrentTarget.gameObject.layer) & unit.AllyLayer.value) != 0;
        float stopDistance = 0f;

        if (isTargetAlly)
        {
            stopDistance = unit.MyData.healRange;
        }
        else
        {
            // 원거리 공격이 가능하면 원거리 사거리에서 정지, 근접만 가능하면 근접 사거리까지 접근
            if (unit.MyData.canRanged) stopDistance = unit.MyData.rangedRange;
            else if (unit.MyData.canMelee) stopDistance = unit.MyData.meleeRange;
        }

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
            unit.transform.Translate(dir * unit.MyData.moveSpeed * Time.deltaTime);
        }
    }
}
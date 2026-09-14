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

        float dist = Vector2.Distance(unit.transform.position, unit.CurrentTarget.position);

        if (dist <= unit.MyData.attackRange)
        {
            if (unit.AttackTimer >= unit.CurrentAttackSpeed)
            {
                unit.ChangeState(unit.attackState); // 공격 사거리 진입 시 Attack 전환
            }
        }
        else
        {
            Vector2 dir = (unit.CurrentTarget.position - unit.transform.position).normalized;
            unit.transform.Translate(dir * unit.MyData.moveSpeed * Time.deltaTime);
        }
    }
}
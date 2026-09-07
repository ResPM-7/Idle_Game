using UnityEngine;

public class UnitMoveState : IUnitState
{
    public void Enter(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }

    public void Execute(Unit_Base_Test unit)
    {
        unit.attackTimer += Time.deltaTime;

        if (unit.currentTarget == null || unit.currentTarget.GetComponent<Unit_Base_Test>().currentHp <= 0)
        {
            unit.ChangeState(unit.idleState);
            return;
        }

        float dist = Vector2.Distance(unit.transform.position, unit.currentTarget.position);

        if (dist <= unit.myData.attackRange)
        {
            if (unit.attackTimer >= unit.myData.attackCooldown)
            {
                unit.ChangeState(unit.attackState); // 공격 사거리 진입 시 Attack 전환
            }
        }
        else
        {
            Vector2 dir = (unit.currentTarget.position - unit.transform.position).normalized;
            unit.transform.Translate(dir * unit.myData.moveSpeed * Time.deltaTime);
        }
    }
}
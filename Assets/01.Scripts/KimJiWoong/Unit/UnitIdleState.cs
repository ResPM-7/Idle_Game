using UnityEngine;

public class UnitIdleState : IUnitState
{
    public void Enter(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }

    public void Execute(Unit_Base_Test unit)
    {
        unit.searchTimer += Time.deltaTime;
        if (unit.searchTimer < 0.2f) return;
        unit.searchTimer = 0f;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(unit.transform.position, unit.myData.attackRange * 2f, unit.targetLayer);
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            Unit_Base_Test enemy = col.GetComponent<Unit_Base_Test>();

            if (enemy != null && enemy.currentHp > 0)
            {
                float dist = Vector2.Distance(unit.transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTarget = enemy.transform;
                }
            }
        }

        if (closestTarget != null)
        {
            unit.currentTarget = closestTarget;
            unit.ChangeState(unit.moveState); // 타겟 발견 시 Move 상태로 전환
        }
    }
}
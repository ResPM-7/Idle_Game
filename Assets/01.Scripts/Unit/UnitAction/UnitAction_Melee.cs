using UnityEngine;

public class UnitAction_Melee : UnitAction_Base
{
    public float damage = 10f;
    public float aoeRadius = 0f; // 0이면 단일 타격, 0보다 크면 주변 적들도 타격
    public float searchRadius = 20f; // 적을 찾는 최대 시야 거리

    public override Transform FindTarget(Unit_Base self)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            Unit_Base enemy = col.GetComponent<Unit_Base>();

            if (enemy != null && enemy.currentState != UnitState.Destroyed && enemy.team != self.team)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTarget = enemy.transform;
                }
            }
        }
        return closestTarget;
    }

    public override void Execute(Unit_Base self, Transform target)
    {
        currentCooldown = actionCooldown;

        if (aoeRadius > 0f) // 광역 공격
        {
            // 타겟 주변의 모든 콜라이더 탐색
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(target.position, aoeRadius);
            foreach (Collider2D col in hitEnemies)
            {
                Unit_Base enemy = col.GetComponent<Unit_Base>();

                // 나와 팀이 다른 유닛에게만 데미지
                if (enemy != null && enemy.team != self.team)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
        else // 단일 공격
        {
            Unit_Base enemy = target.GetComponent<Unit_Base>();
            if (enemy != null && enemy.team != self.team)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, actionRange); // 공격 사거리(빨간색)

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius); // 시야 사거(노란색)
    }
}
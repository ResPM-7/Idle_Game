using UnityEngine;

public class MeleeAction : UnitAction_Base
{
    public float damage = 10f;
    public float aoeRadius = 0f; // 0이면 단일 타격, 0보다 크면 주변 적들도 타격(스플래시)
    public LayerMask targetLayer; // 적을 인식할 레이어

    public override Transform FindTarget(Unit_Base self)
    {
        // 범위 안의 모든 적 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 20f, targetLayer);

        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        // 그 중 가장 가까운 적 찾기
        foreach (Collider2D col in colliders)
        {
            Unit_Base enemy = col.GetComponent<Unit_Base>();
            // 아군이 아니고 살아있는 유닛만 타겟팅
            if (enemy != null && enemy.team != self.team && enemy.currentState != UnitState.Destroyed)
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
        currentCooldown = actionCooldown; // 쿨타임 초기화

        if (aoeRadius > 0f) // 광역 공격일 경우
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(target.position, aoeRadius, targetLayer);
            foreach (Collider2D col in hitEnemies)
            {
                Unit_Base enemy = col.GetComponent<Unit_Base>();
                if (enemy != null && enemy.team != self.team)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
        else // 단일 공격일 경우
        {
            Unit_Base enemy = target.GetComponent<Unit_Base>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    // 근접 사거리
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, actionRange);
    }
}
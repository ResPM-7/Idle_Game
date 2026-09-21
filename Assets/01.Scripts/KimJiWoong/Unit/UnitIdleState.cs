using UnityEngine;

public class UnitIdleState : IUnitState
{
    public void Enter(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }

    public void Execute(Unit_Base_Test unit)
    {
        if (WaveManager.instance != null && WaveManager.instance.stageGiveUp) return;

        unit.SearchTimer += Time.deltaTime;
        if (unit.SearchTimer < 0.2f) return;
        unit.SearchTimer = 0f;

        // À¯´ÖÀÌ °¡Áø ´É·Â Áß °¡Àå ±ä »ç°Å¸®ºÎÅÍ ÆÄ¾Ç
        float maxSearchRange = 0f;
        if (unit.MyData.canMelee) maxSearchRange = Mathf.Max(maxSearchRange, unit.MyData.meleeRange);
        if (unit.MyData.canRanged) maxSearchRange = Mathf.Max(maxSearchRange, unit.MyData.rangedRange);
        if (unit.MyData.canHeal) maxSearchRange = Mathf.Max(maxSearchRange, unit.MyData.healRange);

        float searchRadius = maxSearchRange * 2f; // Å½»ö ¹Ý°æ ³Ë³ËÇÏ°Ô 2¹è·Î ÇØµÒ
        Transform closestTarget = null;

        if (unit.MyData.canHeal)
        {
            closestTarget = FindTarget(unit, unit.AllyLayer, searchRadius, true);
        }

        if (closestTarget == null && (unit.MyData.canMelee || unit.MyData.canRanged))
        {
            closestTarget = FindTarget(unit, unit.TargetLayer, searchRadius, false);
        }

        if (closestTarget != null)
        {
            unit.CurrentTarget = closestTarget;
            unit.ChangeState(unit.moveState);
        }
    }

    private Transform FindTarget(Unit_Base_Test unit, LayerMask searchLayer, float radius, bool findWoundedAlly)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(unit.transform.position, radius, searchLayer);
        Transform bestTarget = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            Unit_Base_Test targetUnit = col.GetComponent<Unit_Base_Test>();
            if (targetUnit != null && targetUnit.CurrentHp > 0)
            {
                if (findWoundedAlly && targetUnit.CurrentHp >= targetUnit.CurrentMaxHp) continue;

                float dist = Vector2.Distance(unit.transform.position, targetUnit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestTarget = targetUnit.transform;
                }
            }
        }
        return bestTarget;
    }
}
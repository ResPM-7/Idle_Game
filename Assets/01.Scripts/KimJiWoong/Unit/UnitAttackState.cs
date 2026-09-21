using UnityEngine;

public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.AttackTimer = 0f;
        Unit_Base_Test targetUnit = unit.CurrentTarget.GetComponent<Unit_Base_Test>();

        if (targetUnit != null)
        {
            float finalAmount = unit.CurrentDamage;
            bool isCrit = false;

            if (Random.Range(0, 100) < unit.CurrentCriticalRate)
            {
                finalAmount *= unit.CurrentCriticalDamage;
                isCrit = true;
            }

            float dist = Vector2.Distance(unit.transform.position, targetUnit.transform.position);
            bool isTargetAlly = ((1 << targetUnit.gameObject.layer) & unit.AllyLayer.value) != 0;

            if (isTargetAlly)
            {
                // 아군이고 힐 사거리 내부일 때 투사체 발사
                if (unit.MyData.canHeal && dist <= unit.MyData.healRange)
                {
                    FireProjectile(unit, targetUnit, finalAmount, true, unit.MyData.healProjectilePoolName);
                }
            }
            else
            {
                // 적일 경우 거리에 따라 공격 방식 선택
                if (unit.MyData.canMelee && dist <= unit.MyData.meleeRange)
                {
                    targetUnit.TakeDamage(finalAmount, isCrit);
                }

                else if (unit.MyData.canRanged && dist <= unit.MyData.rangedRange)
                {
                    FireProjectile(unit, targetUnit, finalAmount, false, unit.MyData.projectilePoolName);
                }
            }
        }
        unit.ChangeState(unit.idleState);
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }

    private void FireProjectile(Unit_Base_Test unit, Unit_Base_Test target, float amount, bool isHeal, string poolName, bool isCrit = false)
    {
        if (string.IsNullOrEmpty(poolName)) return;

        GameObject projectileObj = ObjectPoolManager.instance.GetObject(poolName);
        if (projectileObj != null)
        {
            projectileObj.transform.position = unit.transform.position;
            Projectile proj = projectileObj.GetComponent<Projectile>();
            if (proj != null) proj.Setup(target.transform, amount, isHeal, poolName, isCrit);
        }
    }
}
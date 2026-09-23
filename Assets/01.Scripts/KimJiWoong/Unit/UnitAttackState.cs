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
            float validRange = isTargetAlly ? unit.MyData.healRange : unit.MyData.attackRange;

            if (dist <= validRange)
            {
                if (isTargetAlly)
                {
                    // 아군 타겟 = 힐
                    if (unit.MyData.canHeal)
                    {
                        FireProjectile(unit, targetUnit, finalAmount, true, unit.MyData.healProjectilePoolName, isCrit);
                    }
                }
                else
                {
                    if (unit.MyData.canMelee)
                    {
                        targetUnit.TakeDamage(finalAmount, isCrit);
                    }
                    else if (unit.MyData.canRanged || unit.MyData.canHeal)
                    {
                        FireProjectile(unit, targetUnit, finalAmount, false, unit.MyData.projectilePoolName, isCrit);
                    }
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

        ObjectPoolManager.instance.Spawn(poolName, projectileObj =>
        {
            projectileObj.transform.position = unit.transform.position;
            Projectile proj = projectileObj.GetComponent<Projectile>();
            if (proj == null)
                throw new System.InvalidOperationException($"{projectileObj.name} 프리팹에 Projectile 컴포넌트가 없습니다.");
            proj.Setup(target.transform, amount, isHeal, poolName, isCrit);
        });
    }
}

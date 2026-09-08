public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.attackTimer = 0f;
        Unit_Base_Test enemy = unit.currentTarget.GetComponent<Unit_Base_Test>();

        if (enemy != null)
        {
            float finalDamage = BarracksManager.instance.GetUpgradedAttack(unit.myData.attackDamage);
            unit.currentDamage = finalDamage;
            enemy.TakeDamage(finalDamage);
        }

        unit.ChangeState(unit.idleState);
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}
public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.attackTimer = 0f;
        Unit_Base_Test enemy = unit.currentTarget.GetComponent<Unit_Base_Test>();

        if (enemy != null)
        {
            // 매니저에게 물어보지 않고, 유닛 각자에게 저장된 최종 데미지로 공격합니다!
            enemy.TakeDamage(unit.currentDamage);
        }

        unit.ChangeState(unit.idleState);
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}
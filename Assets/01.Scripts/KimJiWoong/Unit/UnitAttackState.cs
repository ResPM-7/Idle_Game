public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.attackTimer = 0f; // 공격 진입 시 쿨타임 초기화

        Unit_Base_Test enemy = unit.currentTarget.GetComponent<Unit_Base_Test>();
        if (enemy != null)
        {
            enemy.TakeDamage(unit.myData.attackDamage);
        }

        // 공격 애니메이션이 끝난 후 Idle로 돌아가는 처리를 임시로 즉시 전환하도록 작성
        unit.ChangeState(unit.idleState);
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}
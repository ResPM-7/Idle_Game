/// <summary>종류별 처리는 자식 행동에 위임하고 공격 상태 전환만 담당합니다.</summary>
public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.AttackTimer = 0f;
        unit.Combat.Execute();
        unit.CurrentTarget = null;
        if (unit.isActiveAndEnabled && unit.IsCombatReady && unit.CurrentHp > 0f)
            unit.ChangeState(unit.idleState);
    }
    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}

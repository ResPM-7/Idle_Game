/// <summary>공격 종류를 직접 판단하지 않고 선택된 자식 행동에 실행을 요청합니다.</summary>
public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.AttackTimer = 0f;
        unit.Combat.Execute();
        unit.CurrentTarget = null;
        if (unit.isActiveAndEnabled && unit.CurrentHp > 0f) unit.ChangeState(unit.idleState);
    }
    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}

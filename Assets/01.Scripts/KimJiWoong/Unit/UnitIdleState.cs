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
        if (unit.Combat.FindTarget()) unit.ChangeState(unit.moveState);
    }
}

public class UnitDestroyedState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.CurrentTarget = null;

        if (unit.MyData != null && MoneyManager.instance != null)
        {
            MoneyManager.instance.AddGold(unit.MyData.coin);
            MoneyManager.instance.AddCredit(unit.MyData.credit);
        }

        // 풀링 매니저가 있다면 반환, 없다면 SetActive(false)
        if (!string.IsNullOrEmpty(unit.MyData.battlePoolName))
        {
            ObjectPoolManager.instance.ReturnObject(unit.MyData.battlePoolName, unit.gameObject);
        }
        else
        {
            unit.gameObject.SetActive(false);
        }
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}

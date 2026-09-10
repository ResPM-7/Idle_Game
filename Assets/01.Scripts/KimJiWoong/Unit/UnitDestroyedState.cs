public class UnitDestroyedState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.currentTarget = null;

        if (unit.myData != null && MoneyManager.instance != null)
        {
            MoneyManager.instance.AddGold(unit.myData.coin);
            MoneyManager.instance.AddCredit(unit.myData.credit);
        }

        // 풀링 매니저가 있다면 반환, 없다면 SetActive(false)
        if (!string.IsNullOrEmpty(unit.myData.battlePoolName))
        {
            ObjectPoolManager.instance.ReturnObject(unit.myData.battlePoolName, unit.gameObject);
        }
        else
        {
            unit.gameObject.SetActive(false);
        }

        WaveManager.instance.EnemyKilled();
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}

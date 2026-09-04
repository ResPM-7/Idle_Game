public class UnitDestroyedState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.currentTarget = null;

        // 풀링 매니저가 있다면 반환, 없다면 SetActive(false)
        if (!string.IsNullOrEmpty(unit.myData.battlePoolName))
        {
            ObjectPoolManager.instance.ReturnObject(unit.myData.battlePoolName, unit.gameObject);
        }
        else
        {
            unit.gameObject.SetActive(false);
        }
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}

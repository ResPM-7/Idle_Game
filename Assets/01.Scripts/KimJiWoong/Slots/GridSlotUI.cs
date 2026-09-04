
public class GridSlotUI : BaseSlot
{
    // 인벤토리만의 합성 규칙 구현
    protected override void HandleMerge(DragableUnit droppedUnit, DragableUnit myUnit)
    {
        UnitDataSO nextData = myUnit.myData.nexUpdateUnit;

        // 부모 연결 끊기
        myUnit.transform.SetParent(null);
        droppedUnit.transform.SetParent(null);

        ObjectPoolManager.instance.ReturnObject(myUnit.myData.uiPoolName, myUnit.gameObject);
        ObjectPoolManager.instance.ReturnObject(droppedUnit.myData.uiPoolName, droppedUnit.gameObject);

        //인벤토리에서는 합성된 상위 유닛을 지금 내 자리(transform)에 그대로 생성합니다!
        GridUnitFactory.instance.CreateUnit(nextData.uiPoolName, nextData, transform);

        OnAfterDrop();
    }

    protected override void OnAfterDrop()
    {
        // 인벤토리에서 유닛이 합쳐지거나 움직였어도, 배틀 패널을 한 번 동기화해 줍니다.
        // (배틀 필드에 있던 유닛이 인벤토리로 스왑되어 밀려왔을 수도 있기 때문입니다)
        BattleSlotPanel.instance.SyncAllBattleSlots();
    }
}
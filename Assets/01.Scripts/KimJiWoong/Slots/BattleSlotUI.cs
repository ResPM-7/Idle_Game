using UnityEngine;

// MonoBehaviour 대신 방금 만든 BaseSlot을 상속받습니다!
public class BattleSlotUI : BaseSlot
{
    [Header("슬롯 번호 (0 ~ 4)")]
    public int slotIndex;

    // 배틀 슬롯만의 특별한 합성 규칙 구현
    protected override void HandleMerge(DragableUnit droppedUnit, DragableUnit myUnit)
    {
        UnitDataSO nextData = myUnit.myData.nexUpdateUnit;

        // 부모 연결 끊기 (고스트 데이터 방지)
        myUnit.transform.SetParent(null);
        droppedUnit.transform.SetParent(null);

        // 기존 유닛 2개 풀로 반환
        ObjectPoolManager.instance.ReturnObject(myUnit.myData.uiPoolName, myUnit.gameObject);
        ObjectPoolManager.instance.ReturnObject(droppedUnit.myData.uiPoolName, droppedUnit.gameObject);

        // 배틀 슬롯에서 합성되면 인벤토리(originalParent)로 튕겨냅니다!
        GridUnitFactory.instance.CreateUnit(nextData.uiPoolName, nextData, droppedUnit.originalParent);

        OnAfterDrop();
    }

    // 드롭이 완료된 후 처리할 작업
    protected override void OnAfterDrop()
    {
        // 배틀 슬롯에 변화가 생겼으니 5칸 전체 동기화 실행
        BattleSlotPanel.instance.SyncAllBattleSlots();
    }
}
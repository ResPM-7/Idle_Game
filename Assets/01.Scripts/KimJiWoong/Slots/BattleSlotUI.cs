using UnityEngine;

// MonoBehaviour 대신 방금 만든 BaseSlot을 상속받습니다!
public class BattleSlotUI : BaseSlot
{
    [Header("슬롯 번호 (0 ~ 4)")]
    public int slotIndex;

    // 편성 슬롯에서는 합체하지 않고 두 유닛의 자리만 교환합니다.
    protected override void HandleMerge(DragableUnit droppedUnit, DragableUnit myUnit)
    {
        HandleSwap(droppedUnit, myUnit);
    }

    // 드롭이 완료된 후 처리할 작업
    protected override void OnAfterDrop()
    {
        // 배틀 슬롯에 변화가 생겼으니 5칸 전체 동기화 실행
        BattleSlotPanel.instance.SyncAllBattleSlots();
        // 유닛 배치시 사운드 재생
        SoundManager.instance?.PlaySfx("ui_put");
    }
}

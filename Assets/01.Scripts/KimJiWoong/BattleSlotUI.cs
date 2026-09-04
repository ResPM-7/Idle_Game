using UnityEngine;
using UnityEngine.EventSystems;

public class BattleSlotUI : MonoBehaviour, IDropHandler
{
    [Header("슬롯 번호 (0 ~ 4)")]
    public int slotIndex;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        DragableUnit droppedUnit = droppedObj.GetComponent<DragableUnit>();

        if (droppedUnit != null)
        {
            if (transform.childCount == 0) // 1. 내가 빈 칸일 때 -> 단순 이동 및 배치
            {
                droppedUnit.transform.SetParent(transform);
                droppedUnit.transform.position = transform.position;

                // 파티 매니저에게 실제 전투 유닛 소환 명령!
                PartyManager.instance.DeployUnit(slotIndex, droppedUnit.myData);
            }
            else // 내 칸에 이미 누가 있을 때 -> 스왑(자리 바꾸기)
            {
                DragableUnit myUnit = transform.GetChild(0).GetComponent<DragableUnit>();

                // 배틀 슬롯에서는 합성이 일어나지 않으므로 머지 판정 없이 무조건 스왑합니다.
                myUnit.transform.SetParent(droppedUnit.originalParent);
                myUnit.transform.position = droppedUnit.originalParent.position;

                droppedUnit.transform.SetParent(transform);
                droppedUnit.transform.position = transform.position;

                // 새롭게 들어온 유닛의 데이터로 전투 필드 갱신!
                PartyManager.instance.DeployUnit(slotIndex, droppedUnit.myData);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

public class PartySlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Info")]
    public int partyIndex = 1; // 1 ~ 5 파티
    public int slotIndex = 1;  // 1 ~ 5 슬롯

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        DragableUnit droppedUnit = droppedObj.GetComponent<DragableUnit>();
        if (droppedUnit == null) return;

        // 1. 내가 빈 슬롯일 때
        if (transform.childCount == 0)
        {
            // 부모를 현재 슬롯으로 변경 및 위치 초기화
            droppedUnit.transform.SetParent(transform);
            droppedUnit.transform.localPosition = Vector3.zero;

            // 파티 데이터 갱신
            if (PartyManager.instance != null)
            {
                PartyManager.instance.SetUnitToParty(partyIndex, slotIndex, droppedUnit);
            }
        }
        // 2. 이미 슬롯에 다른 유닛이 있을 때 (스왑 처리)
        else
        {
            DragableUnit existingUnit = transform.GetChild(0).GetComponent<DragableUnit>();

            // 기존 유닛을 끌고 들어온 유닛의 이전 슬롯으로 이동
            existingUnit.transform.SetParent(droppedUnit.originalParent);
            existingUnit.transform.localPosition = Vector3.zero;

            // 드래그해온 유닛을 현재 슬롯에 배치
            droppedUnit.transform.SetParent(transform);
            droppedUnit.transform.localPosition = Vector3.zero;

            // 파티 데이터 갱신
            if (PartyManager.instance != null)
            {
                PartyManager.instance.SetUnitToParty(partyIndex, slotIndex, droppedUnit);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

// 각 16개의 빈 칸(Slot)에 붙는 스크립트
public class MergeSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 나에게 드롭된 녀석(드래그 중이던 녀석) 가져오기
        GameObject droppedObj = eventData.pointerDrag;
        DragableUnit droppedUnit = droppedObj.GetComponent<DragableUnit>();

        if (droppedUnit != null)
        {
            if (transform.childCount == 0) // 1. 내가 빈 칸일 때 -> 단순 이동
            {
                droppedUnit.transform.SetParent(transform);
                droppedUnit.transform.position = transform.position;
            }
            else // 2. 내 칸에 이미 누가 있을 때 -> 머지 또는 스왑 판정
            {
                DragableUnit myUnit = transform.GetChild(0).GetComponent<DragableUnit>();

                if (myUnit.unitLevel == droppedUnit.unitLevel) // 머지 성공!
                {
                    Destroy(droppedObj); // 드래그해 온 녀석은 삭제
                    myUnit.LevelUp();
                    // TODO: 레벨업 외형 변경 및 이펙트 연출 호출
                }
                else // 레벨이 다르면 스왑
                {
                    myUnit.transform.SetParent(droppedUnit.originalParent);
                    myUnit.transform.position = droppedUnit.originalParent.position;

                    droppedUnit.transform.SetParent(transform);
                    droppedUnit.transform.position = transform.position;
                }
            }
        }
    }
}
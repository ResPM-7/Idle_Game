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

                if (myUnit.myData.unitLevel == droppedUnit.myData.unitLevel && myUnit.myData.nexUpdateUnit != null)
                {
                    UnitDataSO nextData = myUnit.myData.nexUpdateUnit;
                    string nextPoolName = nextData.uiPoolName; // 다음 레벨 프리팹의 풀 이름

                    // 1. 기존에 있던 1레벨 유닛 2개는 각자의 풀로 돌려보내서 화면에서 삭제합니다.
                    ObjectPoolManager.instance.ReturnObject(myUnit.myData.uiPoolName, myUnit.gameObject);
                    ObjectPoolManager.instance.ReturnObject(droppedUnit.myData.uiPoolName, droppedObj);

                    // 2. 팩토리를 통해 아예 '새로운 2레벨 프리팹'을 이 자리에 소환합니다.
                    GridUnitFactory.instance.CreateUnit(nextPoolName, nextData, transform);
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
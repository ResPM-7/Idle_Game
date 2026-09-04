using UnityEngine;
using UnityEngine.EventSystems;

// MonoBehaviour와 IDropHandler를 상속받는 '추상(abstract)' 부모 클래스입니다.
public abstract class BaseSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        DragableUnit droppedUnit = droppedObj.GetComponent<DragableUnit>();

        if (droppedUnit == null) return;

        // 부모 클래스 타입(BaseSlot)으로 찾으면, 배틀이든 인벤이든 다 찾아냅니다!
        BaseSlot oldSlot = droppedUnit.originalParent.GetComponentInParent<BaseSlot>();

        // 제자리 드롭 방지
        if (oldSlot != null && oldSlot == this)
        {
            droppedUnit.transform.SetParent(transform);
            droppedUnit.transform.position = transform.position;
            return;
        }

        if (transform.childCount == 0)
        {
            HandleEmptySlot(droppedUnit);
        }
        else
        {
            DragableUnit myUnit = transform.GetChild(0).GetComponent<DragableUnit>();

            // 레벨이 같고 다음 단계가 있다면 합성(Merge), 아니면 교체(Swap)
            if (myUnit.myData.unitLevel == droppedUnit.myData.unitLevel && myUnit.myData.nexUpdateUnit != null)
            {
                HandleMerge(droppedUnit, myUnit);
            }
            else
            {
                HandleSwap(droppedUnit, myUnit);
            }
        }
    }

    // 1. 빈 칸 이동 로직 (공통)
    protected virtual void HandleEmptySlot(DragableUnit droppedUnit)
    {
        droppedUnit.transform.SetParent(transform);
        droppedUnit.transform.position = transform.position;
        OnAfterDrop();
    }

    // 2. 스왑 로직 (공통)
    protected virtual void HandleSwap(DragableUnit droppedUnit, DragableUnit myUnit)
    {
        myUnit.transform.SetParent(droppedUnit.originalParent);
        myUnit.transform.position = droppedUnit.originalParent.position;

        droppedUnit.transform.SetParent(transform);
        droppedUnit.transform.position = transform.position;
        OnAfterDrop();
    }

    // 3. 합성 로직 (추상 메서드: 배틀슬롯과 인벤토리의 합성 결과가 다르므로 자식들이 직접 구현하게 강제합니다)
    protected abstract void HandleMerge(DragableUnit droppedUnit, DragableUnit myUnit);

    // 4. 드롭이 끝난 후 실행될 추가 작업 (가상 메서드: 필요할 때 덮어쓰기)
    protected virtual void OnAfterDrop() { }
}
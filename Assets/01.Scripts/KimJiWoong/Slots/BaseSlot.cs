using UnityEngine;
using UnityEngine.EventSystems;

// MonoBehaviour와 IDropHandler를 상속받는 '추상(abstract)' 부모 클래스입니다.
public abstract class BaseSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (this is BattleSlotUI &&
            WaveManager.instance != null &&
            !WaveManager.instance.stageGiveUp)
        {
            Debug.Log("전투 중에는 배틀 슬롯의 유닛을 조작할 수 없습니다!");
            return;
        }

        // 드래그가 실제로 시작되지 않았거나 이미 해제된 경우 방어
        if (eventData == null || eventData.pointerDrag == null)
            return;

        GameObject droppedObj = eventData.pointerDrag;
        DragableUnit droppedUnit = droppedObj.GetComponent<DragableUnit>();

        // 원래 슬롯 정보가 없으면 이동/교체/머지를 처리할 수 없음
        if (droppedUnit == null || droppedUnit.originalParent == null)
            return;

        BaseSlot oldSlot =
            droppedUnit.originalParent.GetComponentInParent<BaseSlot>();

        if (oldSlot != null && oldSlot == this)
        {
            droppedUnit.transform.SetParent(transform);
            droppedUnit.transform.position = transform.position;
            return;
        }

        // 아래 기존 코드 유지
        if (transform.childCount == 0)
        {
            HandleEmptySlot(droppedUnit);
        }
        else
        {
            DragableUnit myUnit =
                transform.GetChild(0).GetComponent<DragableUnit>();

            if (myUnit == null || myUnit.myData == null || droppedUnit.myData == null)
                return;

            if (myUnit.myData.unitLevel == droppedUnit.myData.unitLevel &&
                myUnit.myData.nextUpgradeUnitIds != null &&
                myUnit.myData.nextUpgradeUnitIds.Length > 0)
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

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 그리드 인벤토리 유닛을 드롭하면 크레딧으로 판매하는 쓰레기통 영역입니다.
/// 파티 편성 슬롯에서 시작한 유닛은 판매하지 않습니다.
/// </summary>
public class UnitSellDropZone : MonoBehaviour, IDropHandler
{
    [Header("레벨별 판매 크레딧")]
    [Tooltip("배열의 0번은 Lv.1, 1번은 Lv.2 판매 가격입니다.")]
    [SerializeField] private int[] sellCreditsByLevel = { 2, 3, 4, 5, 6 };

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null)
            return;

        DragableUnit draggedUnit =
            eventData.pointerDrag.GetComponent<DragableUnit>();

        if (draggedUnit == null
            || draggedUnit.myData == null
            || draggedUnit.originalParent == null)
        {
            return;
        }

        // 드래그 중에는 유닛이 Canvas 루트로 이동하므로 현재 부모가 아니라
        // 드래그 시작 위치(originalParent)가 그리드 슬롯인지 확인해야 합니다.
        GridSlotUI originalGridSlot =
            draggedUnit.originalParent.GetComponent<GridSlotUI>();

        if (originalGridSlot == null)
        {
            Debug.Log("파티 편성 슬롯의 유닛은 판매할 수 없습니다.");
            return;
        }

        if (MoneyManager.instance == null)
        {
            Debug.LogWarning("MoneyManager를 찾을 수 없어 유닛을 판매할 수 없습니다.", this);
            return;
        }

        if (ObjectPoolManager.instance == null)
        {
            Debug.LogWarning("ObjectPoolManager를 찾을 수 없어 유닛을 판매할 수 없습니다.", this);
            return;
        }

        UnitDataSO unitData = draggedUnit.myData;
        if (string.IsNullOrEmpty(unitData.uiPoolName))
        {
            Debug.LogWarning($"{unitData.unitName}의 uiPoolName이 비어 있어 판매할 수 없습니다.", this);
            return;
        }

        if (!TryGetSellCredit(unitData.unitLevel, out int refundCredit))
        {
            Debug.LogWarning(
                $"Lv.{unitData.unitLevel}의 판매 가격이 UnitSellDropZone에 설정되지 않았습니다.",
                this);
            return;
        }

        // OnEndDrag가 판매된 유닛을 원래 슬롯으로 되돌리지 않도록 먼저 완료 처리합니다.
        draggedUnit.CompleteExternalDrop();

        MoneyManager.instance.AddCredit(refundCredit);
        ObjectPoolManager.instance.ReturnObject(
            unitData.uiPoolName,
            draggedUnit.gameObject);

        SoundManager.instance?.PlaySfx("ui_sell");

        Debug.Log($"Lv.{unitData.unitLevel} {unitData.unitName} 판매: {refundCredit} 크레딧 획득");
    }

    /// <summary>
    /// 유닛 레벨에 해당하는 판매 가격을 가져옵니다.
    /// 예: 배열의 0번은 Lv.1, 배열의 4번은 Lv.5 가격입니다.
    /// </summary>
    private bool TryGetSellCredit(int unitLevel, out int sellCredit)
    {
        int priceIndex = unitLevel - 1;

        if (sellCreditsByLevel == null
            || priceIndex < 0
            || priceIndex >= sellCreditsByLevel.Length)
        {
            sellCredit = 0;
            return false;
        }

        sellCredit = Mathf.Max(0, sellCreditsByLevel[priceIndex]);
        return true;
    }
}

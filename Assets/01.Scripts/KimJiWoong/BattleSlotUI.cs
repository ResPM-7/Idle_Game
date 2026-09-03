using UnityEngine;
using UnityEngine.UI;

public class BattleSlotUI : MonoBehaviour
{
    public int slotIndex;          // 이 슬롯의 번호 (0 ~ 4)
    public bool isEmpty = true;    // 현재 슬롯이 비어있는지 여부

    private Image slotImage;
    public UnitDataSO currentUnitData;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    // 머지 보드에서 유닛을 넘겨받을 때 호출되는 함수
    public void SetUnit(UnitDataSO data)
    {
        currentUnitData = data;
        isEmpty = false;

        // TODO: data.unitIcon 등이 있다면 slotImage.sprite에 적용하여 시각적 업데이트
        slotImage.color = Color.white; // 예시: 슬롯에 유닛이 들어오면 색상 변경
    }

    // 슬롯에서 유닛을 뺄 때 호출되는 함수
    public void ClearSlot()
    {
        currentUnitData = null;
        isEmpty = true;

        // 시각적 초기화 (원래의 파란색/하얀색 네모로 복구)
        slotImage.color = Color.blue;
    }
}
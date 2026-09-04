using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager instance { get; private set; }

    // 5개 파티 x 5개 슬롯 (총 25개 저장 공간)
    // 실제 유닛 데이터 나 레벨 정보를 담는 배열
    private DragableUnit[,] partyData = new DragableUnit[5, 5];

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 슬롯에 유닛이 등록/변경될 때 호출
    public void SetUnitToParty(int partyIndex, int slotIndex, DragableUnit unit)
    {
        // 0~4 인덱스 기준으로 변환 (인스펙터에서 1~5로 입력했을 경우 대응)
        int pIdx = partyIndex - 1;
        int sIdx = slotIndex - 1;

        if (pIdx < 0 || pIdx >= 5 || sIdx < 0 || sIdx >= 5) return;

        partyData[pIdx, sIdx] = unit;
        Debug.Log($"{partyIndex}파티 {slotIndex}번 슬롯에 Lv.{unit?.unitLevel} 유닛 배치 완료");
    }

    // 슬롯에서 유닛이 빠질 때 호출
    public void RemoveUnitFromParty(int partyIndex, int slotIndex)
    {
        SetUnitToParty(partyIndex, slotIndex, null);
    }
}

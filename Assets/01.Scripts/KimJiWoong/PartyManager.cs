using UnityEngine;

public class PartyManager : Singleton<PartyManager>
{
    [Header("파티 배치 슬롯 (최대 5명)")]
    public UnitDataSO[] partySlots = new UnitDataSO[5];


    // 특정 슬롯(0~4)에 플레이어 유닛 데이터를 할당하는 함수
    public void AssignUnitToSlot(int slotIndex, UnitDataSO unitData)
    {
        if (slotIndex >= 0 && slotIndex < partySlots.Length)
        {
            partySlots[slotIndex] = unitData;
            Debug.Log($"{slotIndex + 1}번 슬롯에 {unitData.unitName} 배치 완료!");

            // TODO: 할당과 동시에 전투 맵(상단 화면)에 해당 유닛 프리팹을 스폰하는 로직 추가
        }
    }

    // 파티 슬롯에서 유닛을 해제하는 함수
    public void RemoveUnitFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < partySlots.Length)
        {
            partySlots[slotIndex] = null;
            Debug.Log($"{slotIndex + 1}번 슬롯 비움");
        }
    }
}
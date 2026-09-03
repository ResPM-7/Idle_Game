using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("설정")] 
    public Transform gridPanel;
    public UnitDataSO baseUnitData;

    private List<Transform> gridSlots = new List<Transform>();

    private void Awake()
    {
        // 그리드 하위의 모든 슬롯을 찾아 리스트에 넣습니다.
        foreach (Transform child in gridPanel)
        {
            gridSlots.Add(child);
        }

        Debug.Log($"초기화 완료: 총 {gridSlots.Count}개의 그리드 슬롯이 리스트에 저장되었습니다.");
    }


    public void SpawnTestUnit()
    {
        Transform targetSlot = null;

        // 미리 저장해둔 리스트만 빠르게 검사합니다.
        foreach (Transform slot in gridSlots)
        {
            if (slot.childCount == 0) // 자식이 없다면 빈 칸
            {
                targetSlot = slot;
                break; // 첫 번째 빈 칸을 찾았으니 즉시 반복문 탈출!
            }
        }

        // 빈 칸을 찾았으면 소환, 못 찾았으면 꽉 찬 상태
        if (targetSlot != null)
        {
            GridUnitFactory.instance.CreateUnit(baseUnitData.uiPoolName, baseUnitData, targetSlot);
        }
        else
        {
            Debug.Log("그리드가 꽉 찼습니다! 소환 불가.");
        }
    }
}
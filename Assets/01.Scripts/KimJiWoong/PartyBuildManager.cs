using UnityEngine;

public class PartyBuildManager : Singleton<PartyBuildManager>
{
    [Header("전투 구역 스폰 위치")]
    public Transform[] spawnPoints = new Transform[5];

    // 현재 전장에 나가 있는 실제 전투 유닛들을 추적
    private GameObject[] activeBattleUnits = new GameObject[5];

    private UnitData[] activeUnitDatas = new UnitData[5];

    public void DeployUnit(int slotIndex, UnitData data)
    {
        // 이 자리에 이미 똑같은 데이터의 유닛이 있다면 다시 소환할 필요가 없습니다
        if (activeUnitDatas[slotIndex] == data) return;

        if (activeBattleUnits[slotIndex] != null)
        {
            UnitData oldData = activeUnitDatas[slotIndex];

            // [더블 풀링 방지] 유닛이 아직 맵에 살아서 활성화되어 있을 때만 풀로 돌려보냅니다!
            if (activeBattleUnits[slotIndex].activeInHierarchy)
            {
                activeBattleUnits[slotIndex].SetActive(false);
                if (oldData != null && !string.IsNullOrEmpty(oldData.battlePoolName))
                {
                    ObjectPoolManager.instance.ReturnObject(oldData.battlePoolName, activeBattleUnits[slotIndex]);
                }
            }

            // 이미 죽었든 살아서 반환됐든, 매니저의 추적 리스트에서는 깔끔하게 지워줍니다.
            activeBattleUnits[slotIndex] = null;
            activeUnitDatas[slotIndex] = null;
        }

        GameObject newBattleUnit = BattleUnitFactory.instance.CreateBattleUnit(data, spawnPoints[slotIndex]);

        if (newBattleUnit != null)
        {
            activeBattleUnits[slotIndex] = newBattleUnit;
            activeUnitDatas[slotIndex] = data;
            Debug.Log($"{slotIndex + 1}번 자리에 [{data.unitName}] 출전 완료!");
        }
    }

    public void RemoveUnit(int slotIndex)
    {
        // 해당 자리에 유닛이 있다면 풀로 반환하고 비웁니다.
        if (activeBattleUnits[slotIndex] != null)
        {
            UnitData oldData = activeUnitDatas[slotIndex];

            // [더블 풀링 방지] 여기도 동일하게 살아있을 때만 반환!
            if (activeBattleUnits[slotIndex].activeInHierarchy)
            {
                activeBattleUnits[slotIndex].SetActive(false);
                if (oldData != null && !string.IsNullOrEmpty(oldData.battlePoolName))
                {
                    ObjectPoolManager.instance.ReturnObject(oldData.battlePoolName, activeBattleUnits[slotIndex]);
                }
            }

            activeBattleUnits[slotIndex] = null;
            activeUnitDatas[slotIndex] = null;
            Debug.Log($"{slotIndex + 1}번 자리 비움!");
        }
    }

    //배틀 할수 있다면 편성된 파티의 수를 확인하기위한 코드 웨이브매니저에게 집어넣어서 maxPlayerDeathCount가 변경되게
    public int GetActiveUnitCount()
    {
        int count = 0;
        for (int i = 0; i < activeBattleUnits.Length; i++)
        {
            if (activeBattleUnits[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    public void ResetAllBattleUnits()
    {
        // 1. 전장에 남아있거나 죽어있는 모든 유닛을 싹 비웁니다.
        for (int i = 0; i < activeBattleUnits.Length; i++)
        {
            if (activeBattleUnits[i] != null)
            {
                UnitData oldData = activeUnitDatas[i];

                // 유닛이 안 죽고 살아서 활성화되어 있다면 풀로 되돌려줍니다.
                // (이미 죽어서 비활성화된 유닛은 에러 방지를 위해 중복 반환하지 않음)
                if (activeBattleUnits[i].activeInHierarchy)
                {
                    activeBattleUnits[i].SetActive(false);
                    if (oldData != null && !string.IsNullOrEmpty(oldData.battlePoolName))
                    {
                        ObjectPoolManager.instance.ReturnObject(oldData.battlePoolName, activeBattleUnits[i]);
                    }
                }

                // 매니저의 추적 데이터 초기화 (이게 버그 해결의 핵심입니다!)
                activeBattleUnits[i] = null;
                activeUnitDatas[i] = null;
            }
        }

        // 2. 패널(UI)에 올려져 있는 유닛들을 기준으로 다시 쌩쌩한 새 유닛들을 소환!
        if (BattleSlotPanel.instance != null)
        {
            BattleSlotPanel.instance.SyncAllBattleSlots();
        }
    }
}

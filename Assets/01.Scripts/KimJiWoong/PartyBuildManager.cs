using UnityEngine;

public class PartyBuildManager : Singleton<PartyBuildManager>
{
    [Header("전투 구역 스폰 위치")]
    public Transform[] spawnPoints = new Transform[5];

    // 현재 전장에 나가 있는 실제 전투 유닛들을 추적
    private GameObject[] activeBattleUnits = new GameObject[5];

    private UnitDataSO[] activeUnitDatas = new UnitDataSO[5];

    public void DeployUnit(int slotIndex, UnitDataSO data)
    {
        // 이 자리에 이미 똑같은 데이터의 유닛이 있다면 다시 소환할 필요가 없습니다
        if (activeUnitDatas[slotIndex] == data) return;

        if (activeBattleUnits[slotIndex] != null)
        {
            UnitDataSO oldData = activeUnitDatas[slotIndex];
            activeBattleUnits[slotIndex].SetActive(false);

            if (oldData != null && !string.IsNullOrEmpty(oldData.battlePoolName))
            {
                ObjectPoolManager.instance.ReturnObject(oldData.battlePoolName, activeBattleUnits[slotIndex]);
            }

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
            UnitDataSO oldData = activeUnitDatas[slotIndex];

            activeBattleUnits[slotIndex].SetActive(false);

            if (oldData != null && !string.IsNullOrEmpty(oldData.battlePoolName))
            {
                ObjectPoolManager.instance.ReturnObject(oldData.battlePoolName, activeBattleUnits[slotIndex]);
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
}

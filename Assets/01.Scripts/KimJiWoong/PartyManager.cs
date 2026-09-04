using UnityEngine;

public class PartyManager : Singleton<PartyManager>
{
    [Header("전투 구역 스폰 위치")]
    public Transform[] spawnPoints = new Transform[5];

    // 현재 전장에 나가 있는 실제 전투 유닛들을 추적
    private GameObject[] activeBattleUnits = new GameObject[5];

    private UnitDataSO[] activeUnitDatas = new UnitDataSO[5];

    public void DeployUnit(int slotIndex, UnitDataSO data)
    {
        // 1. 해당 자리에 이미 소환된 전투 유닛이 있다면 원래의 풀로 반환(Return)합니다.
        if (activeBattleUnits[slotIndex] != null)
        {
            UnitDataSO oldData = activeUnitDatas[slotIndex];

            if (oldData != null && !string.IsNullOrEmpty(oldData.battlePoolName))
            {
                // 오브젝트 풀 매니저를 통해 정확한 풀로 돌려보냅니다!
                ObjectPoolManager.instance.ReturnObject(oldData.battlePoolName, activeBattleUnits[slotIndex]);
            }
            else
            {
                // 만약 데이터가 유실되었거나 풀 이름이 없다면 안전하게 그냥 비활성화 처리
                activeBattleUnits[slotIndex].SetActive(false);
            }

            activeBattleUnits[slotIndex] = null;
            activeUnitDatas[slotIndex] = null;
        }

        // 2. 팩토리를 통해 새로운 전투 유닛을 스폰합니다.
        GameObject newBattleUnit = BattleUnitFactory.instance.CreateBattleUnit(data, spawnPoints[slotIndex]);

        // 3. 성공적으로 생성되었다면 추적 배열에 유닛과 데이터를 각각 저장합니다.
        if (newBattleUnit != null)
        {
            activeBattleUnits[slotIndex] = newBattleUnit;
            activeUnitDatas[slotIndex] = data; // 다음 교체 때 풀 이름을 알기 위해 데이터 저장!

            Debug.Log($"{slotIndex + 1}번 자리에 [{data.unitName}] 출전 완료!");
        }
    }
}

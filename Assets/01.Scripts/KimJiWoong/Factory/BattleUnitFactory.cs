using UnityEngine;

public class BattleUnitFactory : Singleton<BattleUnitFactory>
{
    /// <summary>
    /// 풀에서 전투 유닛을 가져와 지정된 스폰 위치에 배치하고 초기화합니다.
    /// </summary>
    public GameObject CreateBattleUnit(UnitDataSO data, Transform spawnPoint)
    {
        if (string.IsNullOrEmpty(data.battlePoolName))
        {
            Debug.LogWarning($"[{data.unitName}]의 battlePoolName이 비어있습니다!");
            return null;
        }

        // 1. 풀에서 전투 유닛 가져오기
        GameObject battleUnit = ObjectPoolManager.instance.GetObject(data.battlePoolName);

        if (battleUnit != null)
        {
            // 2. 지정된 스폰 위치로 이동 및 초기화
            battleUnit.transform.position = spawnPoint.position;
            battleUnit.transform.rotation = Quaternion.identity;

            // 3. 전투 유닛에 데이터(SO) 주입
            Unit_Base_Test unitScript = battleUnit.GetComponent<Unit_Base_Test>();
            if (unitScript != null)
            {
                unitScript.Init(data);
            }
            else
            {
                Debug.LogWarning($"{battleUnit.name} 프리팹에 Unit_Base_Test 컴포넌트가 없습니다!");
            }
        }

        return battleUnit; // 생성된 유닛 반환
    }
}
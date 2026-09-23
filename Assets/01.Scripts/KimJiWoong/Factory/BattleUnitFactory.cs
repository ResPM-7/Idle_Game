using UnityEngine;

public class BattleUnitFactory : Singleton<BattleUnitFactory>
{
    /// <summary>
    /// 풀에서 전투 유닛을 가져와 지정된 스폰 위치에 배치하고 초기화합니다.
    /// </summary>
    public GameObject CreateBattleUnit(UnitDataSO data, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("전투 유닛 생성 위치가 비어있습니다.", this);
            return null;
        }

        return CreateBattleUnit(data, spawnPoint.position);
    }

    public GameObject CreateBattleUnit(UnitDataSO data, Vector3 spawnPosition)
    {
        if (data == null || string.IsNullOrEmpty(data.battlePoolName))
        {
            Debug.LogWarning("전투 유닛 데이터 또는 battlePoolName이 비어있습니다!", this);
            return null;
        }

        GameObject battleUnit = ObjectPoolManager.instance.GetObject(data.battlePoolName);

        if (battleUnit == null)
        {
            Debug.LogWarning($"전투 유닛 풀에서 가져오기 실패: {data.battlePoolName}", this);
            return null;
        }

        battleUnit.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

        Unit_Base_Test unitScript = battleUnit.GetComponent<Unit_Base_Test>();
        if (unitScript == null)
        {
            Debug.LogWarning($"{battleUnit.name} 프리팹에 Unit_Base_Test 컴포넌트가 없습니다!", battleUnit);
            ObjectPoolManager.instance.ReturnObject(data.battlePoolName, battleUnit);
            return null;
        }

        unitScript.Init(data);
        return battleUnit;
    }
}

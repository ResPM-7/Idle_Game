
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [Header("생성할 적 데이터")]
    [SerializeField] private UnitDataSO enemyData;

    public bool SpawnEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemySpawn의 Enemy Data가 비어 있습니다.");
            return false;
        }

        GameObject enemy = BattleUnitFactory.instance.CreateBattleUnit(
            enemyData,
            transform
        );

        if (enemy == null)
        {
            Debug.LogWarning(
                $"몬스터 생성 실패: {enemyData.battlePoolName}"
            );
            return false;
        }

        return true;
    }

    public bool SpawnEnemy(UnitDataSO data)
    {
        return SpawnEnemy(data, 1f);
    }

    public bool SpawnEnemy(UnitDataSO data, float statMultiplier)
    {
        if (data == null)
        {
            Debug.LogWarning("생성할 Enemy Data가 비어 있습니다.");
            return false;
        }

        Debug.Log($"[스폰] {data.unitName} (ID: {data.unitId}) 배율: {statMultiplier}");

        GameObject enemy = BattleUnitFactory.instance.CreateBattleUnit(
            data,
            transform
        );

        if (enemy == null)
        {
            Debug.LogWarning($"몬스터 생성 실패: {data.battlePoolName}");
            return false;
        }

        // 배율 증가
        if (!Mathf.Approximately(statMultiplier, 1f))
        {
            Unit_Base_Test unit = enemy.GetComponent<Unit_Base_Test>();
            if (unit != null)
            {
                unit.CurrentMaxHp *= statMultiplier;
                unit.CurrentHp = unit.CurrentMaxHp;
                unit.CurrentDamage *= statMultiplier;
                unit.CurrentDefense = Mathf.RoundToInt(unit.CurrentDefense * statMultiplier);
                unit.StatMultiplier = statMultiplier;
            }
        }

        return true;
    }
}
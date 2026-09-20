using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [Header("적 통합 데이터")]
    [SerializeField] private UnitDataSO enemyUnitData;
    [SerializeField] private int enemyUnitId = 201;

    public bool SpawnEnemy()
    {
        if (enemyUnitData == null)
        {
            Debug.LogWarning("EnemySpawn의 EnemyUnitData가 비어 있습니다.");
            return false;
        }

        UnitData enemyData = enemyUnitData.GetById(enemyUnitId);
        if (enemyData == null)
        {
            Debug.LogWarning($"EnemyUnitData에서 unitId {enemyUnitId}를 찾을 수 없습니다.");
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
}

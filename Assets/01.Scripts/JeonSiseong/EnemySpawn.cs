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
}
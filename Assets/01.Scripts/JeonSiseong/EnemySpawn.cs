using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private string poolName = "Enemy";

     public bool SpawnEnemy()  
    {
        GameObject enemy = ObjectPoolManager.instance.GetObject(poolName);

        if(enemy == null )
        {
            Debug.LogWarning("몬스터 생성 실패 :" + poolName);
            return false;
        }

        enemy.transform.position = transform.position;
        enemy.transform.rotation = Quaternion.identity;

        return true;

    }

}

using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    //[SerializeField] GameObject enemyPrefab;   // 몬스터 프리팹

    [SerializeField] private string poolName = "Enemy";




     public void SpawnEnemy()  // 몬스터 스폰
    {
        //Instantiate(enemyPrefab,transform.position,Quaternion.identity);

        GameObject enemy = ObjectPoolManager.instance.GetObject(poolName);

        if(enemy == null )
        {
            Debug.LogWarning(poolName + " 풀을 찾을 수 없습니다");
            return;
        }

        enemy.transform.position = transform.position;
        enemy.transform.rotation = Quaternion.identity;

    }



    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}

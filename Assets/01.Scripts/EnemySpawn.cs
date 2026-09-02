using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;   // 몬스터 프리팹






     public void SpawnEnemy()  // 몬스터 스폰
    {
        Instantiate(enemyPrefab,transform.position,Quaternion.identity);

    }



    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}

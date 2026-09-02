using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }




    [SerializeField] GameObject bossPrefab;    // 보스 몬스터 프리팹
    [SerializeField] Transform bossSpawnPoint;  // 보스 스폰 포인트


    [SerializeField] EnemySpawn[] spawnPoints;    //  몬스터 스폰 포인트 배열

    [SerializeField] float spawnInterval = 1.2f;  // 몬스터 스폰 속도

    [SerializeField] int maxAliveCount = 6;   // 최대 생존 몬스터 수 

    [SerializeField] int maxWave = 10;         

    [SerializeField] int maxKillCount =10;

    [SerializeField] float bossTimeLimit = 30f;    // 보스 클리어 시간제한
    float bossTimer;



    int aliveCount = 0;
    int killCount = 0;
    int currentWave = 1;



    bool isBossBattle = false;
    bool waitingForBoss = false;

    bool bossFinish = false;




    public void SpawnEnemy() 
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);  // 스폰 포인트를 랜덤으로 뽑음

        spawnPoints[randomIndex].SpawnEnemy();    
        
        aliveCount++;
    }


    IEnumerator Spawn()
    {

        while(!isBossBattle)
        {
            if(!waitingForBoss && aliveCount < maxAliveCount)     
            {
                SpawnEnemy();
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }




    void Start()
    {
        StartCoroutine(Spawn());
    }



    public void EnemyKilled()
    {
        aliveCount--;
        killCount++;

        if(waitingForBoss && aliveCount <= 0)
        {
            StartBossBattle();
            return;
        }





        if(killCount >=  maxKillCount)
        {
            NextWave();
        }
    }

    void NextWave()
    {
        if(currentWave >= maxWave)
        {
            waitingForBoss = true;
            
            return;
        }
        currentWave++;
        killCount = 0;
    }

    void StartBossBattle()
    {
        isBossBattle = true;

        Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

        StartCoroutine(BossTimer());
    }
    

    IEnumerator BossTimer()
    {
        bossTimer = bossTimeLimit;

        while(bossTimer > 0 && !bossFinish)
        {
            bossTimer -= Time.deltaTime;

            yield return null;
        }

        if(!bossFinish)
        {
            BossTimeOut();
        }
    }

    public void BossKilled()
    {
        if(bossFinish)    // 보스 사망 함수가 두번 호출 됐을 때, 스테이지 두번 올라가는 것을 방지
        {
            return;
        }

        bossFinish = true;

        NextWave();
    }


    void NextStage()
    {
        isBossBattle = false;

        currentWave = 1;
        killCount = 0;
        aliveCount = 0;
    }


    void BossTimeOut()
    {
        bossFinish = true;

        isBossBattle = false;

        currentWave = 1;
        aliveCount = 0;
        killCount = 0;
    }


    void Update()
    {
        
    }
}

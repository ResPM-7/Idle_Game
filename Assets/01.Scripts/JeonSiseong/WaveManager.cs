using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class WaveManager : Singleton<WaveManager>
{
    
    [Header("Boss")]
    [SerializeField] GameObject bossPrefab;    // 보스 몬스터 프리팹
    [SerializeField] Transform bossSpawnPoint;  // 보스 스폰 포인트

    [Header("Monster Spawn")]
    [SerializeField] EnemySpawn[] spawnPoints;    //  몬스터 스폰 포인트 배열
    
    [Header("Wave")]
    [SerializeField] int maxWave = 5;            // 스테이지 당 웨이브 수
    
    float bossTimer;

    [Header("UI")]
    [SerializeField] TMP_Text waveCountText;        
    [SerializeField] TMP_Text bossTimerText;       

    [Header("Next Wave/Stage Delay")]
    [SerializeField] float nextWaveDelay = 3f;    //  다음 웨이브/스테이지 진입 딜레이 시간


    [Header("Wave Data")]
    [SerializeField] WaveData waveData;     

    int aliveCount = 0;     // 현재 생존 중인 몬스터 수
    int killCount = 0;      // 현재 웨이브 처치 수 
    int spawnedCount = 0;   // 현재 웨이브에서 이미 스폰한 수

    bool bossFinish = false;

    int currentWave = 1;
    int currentStage = 1;

    private WaveState currentState = WaveState.NormalWave;

    public enum WaveState
    {
        NormalWave,
        WaitingNextWave,
        WaitingBoss,
        BossBattle,
        WaitingNextStage
    }

    GameObject currentBoss;

   

    Coroutine bossTimerRoutine;
    Coroutine spawnRoutine;
    Coroutine nextWaveRoutine;
    Coroutine bossDelayRoutine;
    Coroutine nextStageRoutine;

    int GetKillCountForWave(int wave)
    {
      
        int baseCount = waveData.baseKillCountPerWave[wave - 1];
        int growth = (currentStage / waveData.stageGrowthInterval) * waveData.killCountGrowthPerInterval;
        return baseCount + growth;
    }

    void StopRoutine(ref  Coroutine routine)
    {
        if(routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }


    void StartSpawn()
    {

        if(spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(Spawn());
    }

    void StopSpawn()
    {
        if(spawnRoutine != null)
        {
           StopRoutine(ref spawnRoutine);
        }
    }





    public bool SpawnEnemy() 
    {
        if(spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.Log("스폰 포인트가 없습니다");
            return false;
        }


        int randomIndex = Random.Range(0, spawnPoints.Length);  // 스폰 포인트를 랜덤으로 뽑음

        bool success = spawnPoints[randomIndex].SpawnEnemy();

        if(success)
        {
            aliveCount++;
        }

        return success;
        
    }


    IEnumerator Spawn()
    {

        while(currentState == WaveState.NormalWave) 
        {

            int target = GetKillCountForWave(currentWave);


            if(aliveCount < waveData.maxAliveCount && spawnedCount < target)     
            {
               if(SpawnEnemy())
                {
                    spawnedCount++;
                }
            }
            
            yield return new WaitForSeconds(waveData.spawnInterval);
        }

        spawnRoutine = null;
    }




    void Start()
    {
        currentState = WaveState.NormalWave;

        StartSpawn();

        bossTimerText.gameObject.SetActive(false);
    }



    public void EnemyKilled()
    {
        aliveCount--;

        if(aliveCount < 0)
        {
            aliveCount = 0;
        }

        killCount++;

        if(currentState == WaveState.WaitingBoss)
        {
            if(aliveCount <= 0 && bossDelayRoutine == null)
            {
                bossDelayRoutine = StartCoroutine(BossDelay());
            }

            return;
        }

        //일반 웨이브 처치 목표 달성
        if(killCount >= GetKillCountForWave(currentWave))
        {

            // 마지막 웨이브
            if(currentWave >= maxWave)
            {
               currentState = WaveState.WaitingBoss;

                StopSpawn();

                if(aliveCount <= 0 && bossDelayRoutine==null)
                {
                    bossDelayRoutine= StartCoroutine(BossDelay());
                }
            }
            // 일반 다음 웨이브
            else
            {
                currentState = WaveState.WaitingNextWave;

                StopSpawn();

                if(nextWaveRoutine == null)
                {
                    nextWaveRoutine = StartCoroutine(NextWaveDelay());
                }
            }

        }

    }

    IEnumerator BossDelay()
    {
       StopSpawn();


        yield return new WaitForSeconds(nextWaveDelay);

        bossDelayRoutine = null;

        if(currentState == WaveState.WaitingBoss)
        {
            StartBossBattle();
        }
        
    }



    IEnumerator NextWaveDelay()
    {

        yield return new WaitForSeconds(nextWaveDelay);

        nextWaveRoutine = null;

        NextWave();
    }





    void NextWave()
    {
        
        currentWave++;

        killCount = 0;
        spawnedCount = 0;

        currentState = WaveState.NormalWave;

        StartSpawn();
    }

    void StartBossBattle()
    {
        Debug.Log("스타트 보스 배틀 호출됨 ");

        if(currentState == WaveState.BossBattle)
        {
            Debug.Log("이미 보스전 중");
            return;
        }

        if(currentBoss != null)
        {
            Debug.Log("이미 보스 존재");
            return;
        }


        Debug.Log("보스출현");

       currentState = WaveState.BossBattle;
        bossFinish = false;

        bossTimerText.gameObject.SetActive(true);

        currentBoss =
            ObjectPoolManager.instance.GetObject("Boss");

        if(currentBoss != null)
        {
            currentBoss.transform.position = bossSpawnPoint.position;
            currentBoss.transform.rotation = Quaternion.identity;
        }


        bossTimerRoutine = StartCoroutine(BossTimer());
    }
    

    IEnumerator BossTimer()
    {
        bossTimer = waveData.baseBossTimeLimit;

        while(bossTimer > 0 && !bossFinish)
        {
            bossTimer -= Time.deltaTime;

            bossTimerText.text =  Mathf.Ceil(bossTimer).ToString(); 

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

        Debug.Log("보스처치 다음 스테이지 시작");

        currentState = WaveState.WaitingNextStage;

        bossTimerText.gameObject.SetActive(false);


        // 보스 타이머 정지
       StopRoutine(ref bossTimerRoutine);

        //보스전 상태 종료를 미리 설정

        currentBoss = null;

        if(nextStageRoutine==null)
        {
            nextStageRoutine = StartCoroutine(NextStageDelay());
        }

        
    }

    IEnumerator NextStageDelay()
    {
        yield return new WaitForSeconds(nextWaveDelay);

        nextStageRoutine = null;

        NextStage();
    }



    void NextStage()
    {
       
        Debug.Log("다음 스테이지 시작");

        currentStage++;

        currentWave = 1;

        killCount = 0;
        aliveCount = 0;
        spawnedCount = 0;

        currentBoss = null;

        StopSpawn();

        //코루틴 초기화

        StopRoutine(ref spawnRoutine);
        StopRoutine(ref nextWaveRoutine);
        StopRoutine(ref bossDelayRoutine);
        StopRoutine(ref bossTimerRoutine);

        currentState = WaveState.NormalWave;

        StartSpawn();
    }


    void BossTimeOut()
    {
        if (bossFinish)
            return;

        bossFinish = true;

        bossTimerText.gameObject.SetActive(false);

        if(currentBoss !=  null)
        {
           
            ObjectPoolManager.instance.ReturnObject("Boss",currentBoss);
            currentBoss = null;
        }

        if(bossTimerRoutine != null)
        {
            StopCoroutine(bossTimerRoutine);
            bossTimerRoutine = null;
        }

        currentWave = 1;
        aliveCount = 0;
        killCount = 0;
        spawnedCount = 0;

        bossFinish = false;

        StartSpawn() ;
        
    }


    void Update()
    {
        if(currentState == WaveState.BossBattle)
        {
            waveCountText.text = "Stage" + currentStage + "-Boss";
        }
        else
        {
            waveCountText.text = "Stage" + currentStage + "-" + currentWave;
        }

    }
}

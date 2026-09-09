using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    private void Awake()
    {
        if (instance == null)   // 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }



    [Header("Boss")]
    [SerializeField] GameObject bossPrefab;    // 보스 몬스터 프리팹
    [SerializeField] Transform bossSpawnPoint;  // 보스 스폰 포인트

    [Header("Monster Spawn")]
    [SerializeField] EnemySpawn[] spawnPoints;    //  몬스터 스폰 포인트 배열
    //[SerializeField] float spawnInterval = 1.2f;  // 몬스터 스폰 간격
    //[SerializeField] int maxAliveCount = 6;   // 동시 최대 생존 몬스터 수

    [Header("Wave")]
    [SerializeField] int maxWave = 5;            // 스테이지 당 웨이브 수
    //[SerializeField] int maxKillCount =10;  


    //[Header("Boss Timer")]
    //[SerializeField] float bossTimeLimit = 30f;    // 보스 클리어 제한 시간
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


    int currentWave = 1;
    int currentStage = 1;      

    



    bool isBossBattle = false;
    bool waitingForBoss = false;
    GameObject currentBoss;

    bool bossFinish = false;

    Coroutine bossTimerRoutine;
    Coroutine spawnRoutine;
    Coroutine nextWaveRoutine;
    Coroutine bossDelayRoutine;
    Coroutine nextStageRoutine;

    int GetKillCountForWave(int wave)
    {
        //if(wave <= 2)    // 1,2 웨이브 :3마리
        //{  return 3; }  

        //if(wave <= 4)   // 3,4 웨이브 :4마리
        //{ return 4; }

        //return 5;       // 5웨이브 : 5마리


        int baseCount = waveData.baseKillCountPerWave[wave - 1];
        int growth = (currentStage / waveData.stageGrowthInterval) * waveData.killCountGrowthPerInterval;
        return baseCount + growth;
    }


    void StartSpawn()
    {

        if(spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(Spawn());
    }







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

            int target = GetKillCountForWave(currentWave);


            if(!waitingForBoss && aliveCount < waveData.maxAliveCount && spawnedCount < target)     
            {
                SpawnEnemy();
                spawnedCount++;
            }
            
            yield return new WaitForSeconds(waveData.spawnInterval);
        }
    }




    void Start()
    {
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

        if(waitingForBoss && aliveCount <= 0)
        {

            if(bossDelayRoutine == null)
            {
                bossDelayRoutine = StartCoroutine(BossDelay());
            }
            
            return;
        }





        if(killCount >= GetKillCountForWave(currentWave))
        {


            if(currentWave >= maxWave)
            {
                waitingForBoss = true;

                if(spawnRoutine != null)
                {
                    StopCoroutine(spawnRoutine);
                    spawnRoutine = null;
                }

                if(aliveCount <= 0 && bossDelayRoutine==null)
                {
                    bossDelayRoutine= StartCoroutine(BossDelay());
                }
            }
            else
            {
                if(nextWaveRoutine == null)
                {
                    nextWaveRoutine = StartCoroutine(NextWaveDelay());
                }
            }

        }



        Debug.Log("현재 웨이브 : " + currentWave);
        Debug.Log("처치 수 : " + killCount);
        Debug.Log("생존 몬스터 : " + aliveCount);


    }

    IEnumerator BossDelay()
    {
        if(spawnRoutine !=  null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }


        yield return new WaitForSeconds(nextWaveDelay);

        bossDelayRoutine = null;

        StartBossBattle();
    }



    IEnumerator NextWaveDelay()
    {

        if(spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        yield return new WaitForSeconds(nextWaveDelay);

        nextWaveRoutine = null;

        NextWave();
    }





    void NextWave()
    {
        if(currentWave >= maxWave)
        {
            waitingForBoss = true;

            if(aliveCount <= 0 && bossDelayRoutine == null)
            {

                bossDelayRoutine = StartCoroutine(BossDelay());
                
            }
            

            return;
        }
        currentWave++;
        killCount = 0;
        spawnedCount = 0;

        StartSpawn();
    }

    void StartBossBattle()
    {
        Debug.Log("스타트 보스 배틀 호출됨 ");



        if(isBossBattle)
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

        isBossBattle = true;
        waitingForBoss = true;
        bossFinish = false;

        bossTimerText.gameObject.SetActive(true);


        currentBoss =
        Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

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


        bossTimerText.gameObject.SetActive(false);


        // 보스 타이머 정지
        if(bossTimerRoutine != null)
        {
            StopCoroutine(bossTimerRoutine);
            bossTimerRoutine = null;
        }

        //보스전 상태 종료를 미리 설정

        //isBossBattle = false;
        //waitingForBoss = false;

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

        //보스 상태 초기화

        isBossBattle = false;
        waitingForBoss = false;
        bossFinish = false;

        //보스 참조 초기화

        currentBoss = null;

        //웨이브 초기화 + 스테이지 증가

        currentStage++;
        currentWave = 1;
        killCount = 0;
        aliveCount = 0;
        spawnedCount = 0;

        //코루틴 초기화

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        if(nextWaveRoutine != null)
        {
            StopCoroutine(nextWaveRoutine);
            nextWaveRoutine = null;
        }

        if(bossDelayRoutine != null)
        {
            StopCoroutine(bossDelayRoutine);
            bossDelayRoutine = null;
        }


        if(bossTimerRoutine !=null)
        {
            StopCoroutine (bossTimerRoutine);
            bossTimerRoutine = null;
        }


        StartSpawn();
    }


    void BossTimeOut()
    {
        bossFinish = true;

        bossTimerText.gameObject.SetActive(false);

        if(currentBoss !=  null)
        {
            Destroy(currentBoss);
            currentBoss = null;
        }

        if(bossTimerRoutine != null)
        {
            StopCoroutine(bossTimerRoutine);
            bossTimerRoutine = null;
        }


        waitingForBoss = false;
        isBossBattle = false;

        currentWave = 1;
        aliveCount = 0;
        killCount = 0;
        spawnedCount = 0;

        StartSpawn() ;
        bossFinish = false;
    }


    void Update()
    {
        if(isBossBattle)
        {
            waveCountText.text = "Stage" + currentStage + "-Boss";
        }
        else
        {
            waveCountText.text = "Stage" + currentStage + "-" + currentWave;
        }

    }
}

using System.Collections;
using UnityEngine;
using TMPro;

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



    [Header("Boss")]
    [SerializeField] GameObject bossPrefab;    // 보스 몬스터 프리팹
    [SerializeField] Transform bossSpawnPoint;  // 보스 스폰 포인트

    [Header("Monster Spawn")]
    [SerializeField] EnemySpawn[] spawnPoints;    //  몬스터 스폰 포인트 배열
    [SerializeField] float spawnInterval = 1.2f;  // 몬스터 스폰 속도
    [SerializeField] int maxAliveCount = 6;   // 최대 생존 몬스터 수 

    [Header("Wave")]
    [SerializeField] int maxWave = 10;         
    [SerializeField] int maxKillCount =10;


    [Header("Boss Timer")]
    [SerializeField] float bossTimeLimit = 30f;    // 보스 클리어 시간제한
    float bossTimer;

    [Header("UI")]
    [SerializeField] TMP_Text waveCountText;        //     텍스트 추가
    [SerializeField] TMP_Text bossTimerText;        //     텍스트 추가

    [Header("Next Wave/Stage Delay")]
    [SerializeField] float nextWaveDelay = 3f;    //  다음 웨이브 딜레이 시간


    int aliveCount = 0;
    int killCount = 0;
    int currentWave = 1;

    



    bool isBossBattle = false;
    bool waitingForBoss = false;
    GameObject currentBoss;

    bool bossFinish = false;

    Coroutine bossTimerRoutine;
    Coroutine spawnRoutine;

    Coroutine nextWaveRoutine;
    Coroutine bossDelayRoutine;
    Coroutine nextStageRoutine;


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
            if(!waitingForBoss && aliveCount < maxAliveCount)     
            {
                SpawnEnemy();
            }
            
            yield return new WaitForSeconds(spawnInterval);
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





        if(killCount >=  maxKillCount)
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

        StartSpawn();
    }

    void StartBossBattle()
    {

        if(isBossBattle)
        {
            return;
        }

        if(currentBoss != null)
        {
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
        bossTimer = bossTimeLimit;

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

        if(bossTimerRoutine !=null)
        {
            StopCoroutine (bossTimerRoutine);
            bossTimerRoutine = null;
        }

        currentBoss=null;

        isBossBattle = false;

        currentWave = 1;
        killCount = 0;
        aliveCount = 0;

        waitingForBoss = false;
        bossFinish = false;

        if(spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
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

        StartSpawn() ;
        bossFinish = false;
    }


    void Update()
    {
        waveCountText.text = "WAVE" + currentWave;
    }
}

using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class WaveManager : Singleton<WaveManager>
{

    [Header("Boss")]
    [SerializeField] private UnitDataSO bossData;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("Monster Spawn")]
    [SerializeField] EnemySpawn[] spawnPoints;    //  몬스터 스폰 포인트 배열

    [Header("Wave")]
    [SerializeField] int maxWave = 5;            // 스테이지 당 웨이브 수

    float bossTimer;

    [Header("UI")]
    [SerializeField] TMP_Text waveCountText;
    [SerializeField] TMP_Text bossTimerText;
    [SerializeField] TMP_Text giveUpButtonText;

    [Header("Next Wave/Stage Delay")]
    [SerializeField] float nextWaveDelay = 3f;    //  다음 웨이브/스테이지 진입 딜레이 시간

    [Header("Game Over Delay")]
    [SerializeField] float gameOverDelay = 2f;


    [Header("Wave Data")]
    [SerializeField] WaveData waveData;

    int aliveCount = 0;     // 현재 생존 중인 몬스터 수
    int killCount = 0;      // 현재 웨이브 처치 수
    int spawnedCount = 0;   // 현재 웨이브에서 이미 스폰한 수

    [Header("임시 게임오버 플레이어 사망 횟수")]
    [SerializeField] public int maxPlayerDeathCount;

    int playerDeathCount = 0;   // 플레이어 사망 횟수

    bool bossFinish = false;

    public bool stageGiveUp = false;

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
    List<Unit_Base_Test> activeEnemies = new List<Unit_Base_Test>();  // 현재 살아있는 일반 몬스터



    Coroutine bossTimerRoutine;
    Coroutine spawnRoutine;
    Coroutine nextWaveRoutine;
    Coroutine bossDelayRoutine;
    Coroutine nextStageRoutine;
    Coroutine gameOverRoutine;

    public int CurrentStage => currentStage;
    public void RestoreStage(int stage)
    {
        // 저장된 스테이지의 첫 웨이브에서 사용자가 시작 버튼을 누르도록 대기합니다.
        if (!stageGiveUp) StageGiveUp();
        currentStage = Mathf.Max(1, stage);
        ResetWave();
        OnStageChanged?.Invoke(currentStage);
    }
    public event System.Action<int> OnStageChanged;

    //웨이브 초기화
    void ResetWave()
    {
        currentWave = 1;

        aliveCount = 0;
        killCount = 0;
        spawnedCount = 0;
    }


    int GetKillCountForWave(int wave)
    {

        int baseCount = waveData.baseKillCountPerWave[wave - 1];
        int growth = (currentStage / waveData.stageGrowthInterval) * waveData.killCountGrowthPerInterval;
        return baseCount + growth;
    }

    void StopRoutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void OnEnable()
    {
        Unit_Base_Test.OnUnitSpawned += RegisterEnemy;
        Unit_Base_Test.OnUnitDespawned += UnregisterEnemy;

    }

    private void OnDisable()
    {
        Unit_Base_Test.OnUnitSpawned -= RegisterEnemy;
        Unit_Base_Test.OnUnitDespawned -= UnregisterEnemy;
    }

    void RegisterEnemy(Unit_Base_Test unit)
    {
        if (unit.GetComponent<Enemy>() == null)
            return;

        if (!activeEnemies.Contains(unit))
        {
            activeEnemies.Add(unit);
        }
    }

    void UnregisterEnemy(Unit_Base_Test unit)
    {
        activeEnemies.Remove(unit);
    }

    void FinishBoss()
    {
        bossTimerText.gameObject.SetActive(false);

        StopRoutine(ref bossTimerRoutine);

        currentBoss = null;

        if (BossHUDPresenter.instance != null)
        {
            BossHUDPresenter.instance.EndBossBattle();
        }
    }

    void StartSpawn()
    {

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(Spawn());
    }

    void StopSpawn()
    {
        if (spawnRoutine != null)
        {
            StopRoutine(ref spawnRoutine);
        }
    }





    public bool SpawnEnemy()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.Log("스폰 포인트가 없습니다");
            return false;
        }


        int randomIndex = Random.Range(0, spawnPoints.Length);  // 스폰 포인트를 랜덤으로 뽑음

        bool success = spawnPoints[randomIndex].SpawnEnemy();

        if (success)
        {
            aliveCount++;
        }

        return success;

    }


    IEnumerator Spawn()
    {

        while (currentState == WaveState.NormalWave)
        {

            int target = GetKillCountForWave(currentWave);


            if (aliveCount < waveData.maxAliveCount && spawnedCount < target)
            {
                if (SpawnEnemy())
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
        currentState = WaveState.WaitingNextStage;

        stageGiveUp = true;

        bossTimerText.gameObject.SetActive(false);

        giveUpButtonText.text = "START";
    }



    public void EnemyKilled()
    {
        aliveCount--;

        if (aliveCount < 0)
        {
            aliveCount = 0;
        }

        killCount++;

        if (currentState == WaveState.WaitingBoss)
        {
            if (aliveCount <= 0 && bossDelayRoutine == null)
            {
                bossDelayRoutine = StartCoroutine(BossDelay());
            }

            return;
        }

        //일반 웨이브 처치 목표 달성
        if (killCount >= GetKillCountForWave(currentWave))
        {

            // 마지막 웨이브
            if (currentWave >= maxWave)
            {
                currentState = WaveState.WaitingBoss;

                StopSpawn();

                if (aliveCount <= 0 && bossDelayRoutine == null)
                {
                    bossDelayRoutine = StartCoroutine(BossDelay());
                }
            }
            // 일반 다음 웨이브
            else
            {
                currentState = WaveState.WaitingNextWave;

                StopSpawn();

                if (nextWaveRoutine == null)
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

        if (currentState == WaveState.WaitingBoss)
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

        //이미 보스전이면 중복 실행 방지
        if (currentState == WaveState.BossBattle)
        {
            Debug.Log("이미 보스전 중");
            return;
        }

        //이미 보스가 존재하면 중복 생성 방지
        if (currentBoss != null)
        {
            Debug.Log("이미 보스 존재");
            return;
        }

        if (bossData == null)
        {
            Debug.LogWarning("WaveManager의 Boss Data가 비어 있습니다.");
            currentState = WaveState.WaitingBoss;
            return;
        }

        GameObject boss = BattleUnitFactory.instance.CreateBattleUnit(
            bossData,
            bossSpawnPoint
        );

        if (boss == null)
        {
            Debug.LogWarning(
                $"보스 생성 실패: {bossData.battlePoolName}"
            );

            currentState = WaveState.WaitingBoss;
            return;
        }

        currentBoss = boss;

        //보스전 시작
        currentState = WaveState.BossBattle;
        bossFinish = false;

        bossTimerText.gameObject.SetActive(true);

        Debug.Log("보스출현");

        //보스 생성 성공 후에만 타이머 시작
        Unit_Base_Test bossUnit = currentBoss.GetComponent<Unit_Base_Test>();
        if (bossUnit != null && BossHUDPresenter.instance != null)
        {
            BossHUDPresenter.instance.BeginBossBattle(
                bossUnit.MyData.unitName,
                bossUnit.CurrentHp,
                bossUnit.MyData.maxHp,
                waveData.baseBossTimeLimit,
                waveData.baseBossTimeLimit
            );
        }

        bossTimerRoutine = StartCoroutine(BossTimer());
    }


    IEnumerator BossTimer()
    {
        bossTimer = waveData.baseBossTimeLimit;

        while (bossTimer > 0 && !bossFinish)
        {
            bossTimer -= Time.deltaTime;

            bossTimerText.text = Mathf.Ceil(bossTimer).ToString();

            if (BossHUDPresenter.instance != null)
            {
                BossHUDPresenter.instance.UpdateBossTimer(bossTimer, waveData.baseBossTimeLimit);
            }

            yield return null;
        }

        if (!bossFinish)
        {
            BossTimeOut();
        }
    }

    public void BossKilled()
    {

        if (bossFinish)    // 보스 사망 함수가 두번 호출 됐을 때, 스테이지 두번 올라가는 것을 방지
        {
            return;
        }

        bossFinish = true;

        Debug.Log("보스처치 다음 스테이지 시작");

        currentState = WaveState.WaitingNextStage;

        FinishBoss();

        if (nextStageRoutine == null)
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
        OnStageChanged?.Invoke(currentStage);

        ResetWave();

        currentBoss = null;

        StopSpawn();

        //코루틴 초기화
        StopRoutine(ref spawnRoutine);
        StopRoutine(ref nextWaveRoutine);
        StopRoutine(ref bossDelayRoutine);
        StopRoutine(ref bossTimerRoutine);

        currentState = WaveState.NormalWave;

        // 아군 유닛 체력 초기화
        if(PartyBuildManager.instance != null)
        {
            PartyBuildManager.instance.ResetAllBattleUnits();
        }

        StartSpawn();
    }

    public void GameOverRestart()
    {
        // 현재 진행 중인 모든 코루틴 정지
        StopRoutine(ref spawnRoutine);
        StopRoutine(ref nextWaveRoutine);
        StopRoutine(ref bossDelayRoutine);
        StopRoutine(ref bossTimerRoutine);
        StopRoutine(ref nextStageRoutine);

        // 현재 보스가 있으면 풀로 반환
        if (currentBoss != null)
        {
            ObjectPoolManager.instance.ReturnObject("Boss", currentBoss);
            currentBoss = null;
        }

        // 현재 살아있는 일반 몬스터 전부 풀로 반환
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Unit_Base_Test enemy = activeEnemies[i];

            if (enemy != null && enemy.gameObject.activeSelf)
            {
                ObjectPoolManager.instance.ReturnObject(
                    enemy.MyData.battlePoolName,
                    enemy.gameObject);
            }
        }

        activeEnemies.Clear();

        // 보스 관련 UI 종료
        FinishBoss();

        // 현재 스테이지는 유지, 웨이브만 1로 초기화
        ResetWave();

        bossFinish = false;

        // 일반 웨이브로 상태 변경
        currentState = WaveState.NormalWave;

        //게임오버로 인한 재시작 시에도 아군 체력 리셋 및 사망 횟수 갱신
        PartyBuildManager.instance.ResetAllBattleUnits();
        maxPlayerDeathCount = PartyBuildManager.instance.GetActiveUnitCount();
        playerDeathCount = 0;

        // 다시 1웨이브 시작
        StartSpawn();
    }

    public void StageGiveUp()
    {
        if (stageGiveUp && (PartyBuildManager.instance == null ||
            PartyBuildManager.instance.GetActiveUnitCount() == 0))
        {
            Debug.Log("파티에 유닛을 1명 이상 편성해야 합니다.");
            return;
        }
        // 아직 게임 시작 전이라면
        if(stageGiveUp)
        {
            stageGiveUp = false;

            ResetWave();
            bossFinish = false;
            currentState = WaveState.NormalWave;

            giveUpButtonText.text = "GIVE UP";

            maxPlayerDeathCount = PartyBuildManager.instance.GetActiveUnitCount();
            playerDeathCount = 0;

            Debug.Log("게임 시작");

            StartSpawn();
            return;
        }


        // 이미 포기한 상태라면 다시 시작
        if (stageGiveUp)
        {
            stageGiveUp = false;

            ResetWave();

            bossFinish = false;

            currentState = WaveState.NormalWave;

            giveUpButtonText.text = "GIVE UP";

            maxPlayerDeathCount = PartyBuildManager.instance.GetActiveUnitCount();
            playerDeathCount = 0; // 누적 사망 횟수도 0으로 초기화

            Debug.Log($"Stage{currentStage}-1 재시작 (최대 사망 허용: {maxPlayerDeathCount}명)");

            StartSpawn();

            return;
        }

        //처음 누른 경우에는 포기
        Debug.Log("현재 웨이브 포기");

        stageGiveUp = true;

        // 현재 실행 중인 코루틴 전부 정지
        StopRoutine(ref spawnRoutine);
        StopRoutine(ref nextWaveRoutine);
        StopRoutine(ref bossDelayRoutine);
        StopRoutine(ref bossTimerRoutine);
        StopRoutine(ref nextStageRoutine);

        // 현재 보스가 있으면 풀로 반환
        if (currentBoss != null)
        {
            ObjectPoolManager.instance.ReturnObject("Boss", currentBoss);
            currentBoss = null;
        }

        // 현재 살아 있는 일반 몬스터 전부 풀로 반환
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Unit_Base_Test enemy = activeEnemies[i];

            if (enemy != null && enemy.gameObject.activeSelf)
            {
                ObjectPoolManager.instance.ReturnObject(enemy.MyData.battlePoolName, enemy.gameObject);
            }
        }

        // 리스트 정리
        activeEnemies.Clear();

        // 보스 UI 종료
        FinishBoss();

        PartyBuildManager.instance.ResetAllBattleUnits();

        // 전투 중지
        currentState = WaveState.WaitingNextStage;

        Debug.Log($"Stage{currentStage} 중지 상태");

        //버튼 글자 변경
        giveUpButtonText.text = "RESTART";

        Debug.Log($"Stage {currentStage} 포기 상태");


    }
    public void PlayerDied()
    {
        playerDeathCount++;

        Debug.Log($"플레이어 사망 : {playerDeathCount} / {maxPlayerDeathCount}");

        if (playerDeathCount >= maxPlayerDeathCount)
        {
            Debug.Log("게임오버");

            if(gameOverRoutine == null)
            {
                gameOverRoutine = StartCoroutine(GameOverDelay());
            }

        }
    }

    IEnumerator GameOverDelay()
    {
        // 딜레이 도는 동안 웨이브 진행이 멈추도록 정지
        StopRoutine(ref spawnRoutine);
        StopRoutine(ref nextWaveRoutine);
        StopRoutine(ref bossDelayRoutine);
        StopRoutine(ref bossTimerRoutine);
        StopRoutine(ref nextStageRoutine);

        yield return new WaitForSeconds(gameOverDelay);

        gameOverRoutine = null;

        GameOverRestart();
        playerDeathCount = 0;

    }


    void BossTimeOut()
    {
        if (bossFinish)
            return;

        bossFinish = true;

        Debug.Log("보스 제한시간 종료");

        bossTimerText.gameObject.SetActive(false);

        //보스 풀로 반환
        if (currentBoss != null)
        {

            ObjectPoolManager.instance.ReturnObject("Boss", currentBoss);
        }

        FinishBoss();

        ResetWave();

        currentState = WaveState.NormalWave;

        bossFinish = false;

        //일반 웨이브 다시 시작
        StartSpawn();

    }


    void Update()
    {
        if (stageGiveUp)
        {
            waveCountText.text = "Stage" + currentStage;
            return;
        }


        if (currentState == WaveState.BossBattle || currentState == WaveState.WaitingNextStage)
        {
            waveCountText.text = "Stage" + currentStage + "-Boss";
        }
        else
        {
            waveCountText.text = "Stage" + currentStage + "-" + currentWave;
        }

    }
}

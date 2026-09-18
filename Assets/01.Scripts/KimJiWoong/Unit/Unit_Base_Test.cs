using System;
using UnityEngine;

public interface IUnitState
{
    void Enter(Unit_Base_Test unit);    // 상태에 진입할 때 1회 호출
    void Execute(Unit_Base_Test unit);  // Update처럼 매 프레임 호출
    void Exit(Unit_Base_Test unit);     // 상태를 빠져나갈 때 1회 호출
}


public class Unit_Base_Test : MonoBehaviour, ISkillDamageable
{
    //유닛 업그레이드 결합도를 낮추기위해 델리게이트
    public static event Action<Unit_Base_Test> OnUnitSpawned;
    public static event Action<Unit_Base_Test> OnUnitDespawned;

    public event Action OnDeathEvent;
    public event Action<Unit_Base_Test, float, float, float, bool> OnHpChanged;

    [Header("기본 설정")]
    [SerializeField] private UnitDataSO myData;
    //본인의 외형
    [SerializeField] private SpriteRenderer spriteRenderer;
    public LayerMask TargetLayer { get; private set; }
    public LayerMask AllyLayer { get; private set; }

    public UnitDataSO MyData => myData;
    public float CurrentAttackSpeed { get; set; }
    public float CurrentMaxHp { get; set; }
    public float CurrentHp { get; set; }
    public float CurrentDamage { get; set; }
    public int CurrentDefense { get; set; }
    public float CurrentCriticalRate { get; set; }
    public float CurrentCriticalDamage { get; set; }
    public float AttackTimer { get; set; }
    public float SearchTimer { get; set; }
    public Transform CurrentTarget { get; set; }

    // FSM 관련 변수
    private IUnitState currentState;

    // 상태 객체를 미리 생성
    public IUnitState idleState = new UnitIdleState();
    public IUnitState moveState = new UnitMoveState();
    public IUnitState attackState = new UnitAttackState();
    public IUnitState destroyedState = new UnitDestroyedState();

    private void Start()
    {
        if (myData != null) Init(myData);
        else Debug.LogWarning($"{gameObject.name}에 데이터(UnitDataSO)가 비어있습니다!");
    }

    public void Init(UnitDataSO data)
    {
        myData = data;

        int playerLayerIdx = LayerMask.NameToLayer("Player");
        int enemyLayerIdx = LayerMask.NameToLayer("Enemy");

        if (playerLayerIdx == -1 || enemyLayerIdx == -1)
        {
            Debug.LogError("유니티 설정에 'Player' 또는 'Enemy' 레이어가 없습니다! Add Layer를 해주세요.");
        }

        if (myData.team == Team_Test.Player)
        {
            gameObject.layer = playerLayerIdx;            // 내 레이어를 Player로
            AllyLayer = 1 << playerLayerIdx;              // 아군 탐색용 레이어 = Player
            TargetLayer = 1 << enemyLayerIdx;             // 적 탐색용 레이어 = Enemy
        }
        else if (myData.team == Team_Test.Enemy)
        {
            gameObject.layer = enemyLayerIdx;             // 내 레이어를 Enemy로
            AllyLayer = 1 << enemyLayerIdx;               // 아군 탐색용 레이어 = Enemy
            TargetLayer = 1 << playerLayerIdx;            // 적 탐색용 레이어 = Player
        }

        if (spriteRenderer != null && myData.unitSprite != null)
        {
            spriteRenderer.sprite = myData.unitSprite;
        }

        CurrentMaxHp = myData.maxHp;
        CurrentHp = CurrentMaxHp;
        CurrentDamage = myData.attackDamage;
        CurrentAttackSpeed = myData.attackSpeed;
        CurrentDefense = myData.defense;
        CurrentCriticalRate = myData.criticalRate;
        CurrentCriticalDamage = myData.criticalDamage;
        AttackTimer = 0f;
        SearchTimer = 0f;
        CurrentTarget = null;


        ChangeState(idleState);
        //소환될때 유닛 체력바가 제대로 출력되게
        OnHpChanged?.Invoke(this, CurrentHp, myData.maxHp, 0f, false);
        // 매니저를 직접 찾지 않고 스폰되었다는 방송만 송출합니다
        OnUnitSpawned?.Invoke(this);
    }

    private void OnDisable()
    {
        if (gameObject.scene.isLoaded)
        {
            // 유닛이 죽거나 풀로 돌아갈 때 방송 송출
            OnUnitDespawned?.Invoke(this);
        }
    }

    void Update()
    {
        // 현재 상태의 Execute 로직을 매 프레임 실행
        if (currentState != null)
        {
            currentState.Execute(this);
        }
    }

    // 상태 전환을 처리하는 핵심 함수
    public void ChangeState(IUnitState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = newState;
        currentState.Enter(this);
    }

    public void TakeDamage(float amount, bool isCritical = false)
    {
        if (currentState == destroyedState) return;

        //방어력 차감 방어력이 높아서 데미지가 0이되어도 이벤트가 나오게 구현
        float finalDamage = Mathf.Max(0f, amount - CurrentDefense);

        CurrentHp -= finalDamage;

        //UI나 이펙트 쪽에 '최종 계산된 데미지(finalDamage)'를 넘겨줍니다.
        OnHpChanged?.Invoke(this, CurrentHp, CurrentMaxHp, finalDamage, isCritical);

        if (CurrentHp <= 0)
        {
            OnDeathEvent?.Invoke();
            ChangeState(destroyedState);
        }
    }

    public void TakeHeal(float amount)
    {
        if (currentState == destroyedState) return;

        CurrentHp = Mathf.Min(CurrentHp + amount, CurrentMaxHp);
        // UI 업데이트
        OnHpChanged?.Invoke(this, CurrentHp, CurrentMaxHp, -1f, false);
    }

    public void TakeSkillDamage(float damage)
    {
        TakeDamage(damage);
    }

    public void ResetAndStopCombat()
    {
        // 1. 체력을 최대치로 100% 회복
        CurrentHp = myData.maxHp;

        // 2. UI 체력바 갱신
        OnHpChanged?.Invoke(this, CurrentHp, myData.maxHp, 0f, false);

        // 3. 타겟팅 초기화 및 타이머 리셋
        CurrentTarget = null;
        AttackTimer = 0f;
        SearchTimer = 0f;

        // 4. 현재 공격 중이거나 이동 중이더라도 강제로 대기(Idle) 상태로 정지!
        ChangeState(idleState);
    }

    private void OnDrawGizmosSelected()
    {
        // 데이터가 아직 안 들어왔다면 그리지 않음 (에러 방지)
        if (myData == null) return;

        // 1. 실제 공격 사거리 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, myData.attackRange);

        // 2. 적 탐색 범위 (노란색) - 현재 코드에서 사거리의 2배로 탐색 중이시죠!
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, myData.attackRange * 2f);
    }
}
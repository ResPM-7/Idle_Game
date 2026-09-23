using System;
using UnityEngine;

public interface IUnitState
{
    void Enter(Unit_Base_Test unit);    // 상태에 진입할 때 1회 호출
    void Execute(Unit_Base_Test unit);  // Update처럼 매 프레임 호출
    void Exit(Unit_Base_Test unit);     // 상태를 빠져나갈 때 1회 호출
}


public class Unit_Base_Test : MonoBehaviour, ISkillDamageable, IPoolable
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
    // 외부 강화는 기본 공격력을 갱신하고, 일시 버프는 읽을 때 배율로 합성합니다.
    private float damageWithoutBuff;
    public float CurrentDamage
    {
        get => damageWithoutBuff * statusEffects.DamageMultiplier;
        set => damageWithoutBuff = value;
    }
    public int CurrentDefense { get; set; }
    public float CurrentCriticalRate { get; set; }
    public float CurrentCriticalDamage { get; set; }
    public float AttackTimer { get; set; }
    public float SearchTimer { get; set; }
    public float StatMultiplier { get; set; } = 1f;
    public UnitCombatController Combat { get; } = new UnitCombatController();
    private readonly UnitStatusEffects statusEffects = new UnitStatusEffects();
    public Transform CurrentTarget
    {
        get => Combat.Target;
        set => Combat.SetTarget(value);
    }
    public Vector2 MoveDirection => moveDirection;
    public bool IsMoving => !IsFrozen
        && currentState == moveState
        && moveDirection.sqrMagnitude > 0.000001f;

    // FSM 관련 변수
    private IUnitState currentState;

    // 상태 객체를 미리 생성
    public IUnitState idleState = new UnitIdleState();
    public IUnitState moveState = new UnitMoveState();
    public IUnitState attackState = new UnitAttackState();
    public IUnitState destroyedState = new UnitDestroyedState();

    private float baseDamage;
    private bool isInitialized;
    private bool referencesReady;
    private bool pendingSpawn;
    private bool spawnAnnounced;
    public uint SpawnVersion { get; private set; }
    private BattleUnitVisual battleVisual;
    private Rigidbody2D rigidBody;
    private Collider2D targetingCollider;
    public bool CanBeTargeted => isActiveAndEnabled && targetingCollider != null
        && targetingCollider.enabled && (rigidBody == null || rigidBody.simulated);
    private Vector2 moveDirection;

    public bool IsFrozen => statusEffects.IsFrozen;
    private UnitVisualEffect visualEffect;

    private void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (referencesReady) return;
        referencesReady = true;
        // 프리팹에 미리 부착한 BattleUnitVisual을 한 번만 가져옵니다.
        battleVisual = GetComponent<BattleUnitVisual>();
        rigidBody = GetComponent<Rigidbody2D>();
        targetingCollider = GetComponent<Collider2D>();
        visualEffect = GetComponent<UnitVisualEffect>();

        if (rigidBody == null)
            Debug.LogError($"{gameObject.name} 프리팹에 Rigidbody2D가 없습니다.", this);
    }

    private void Start()
    {
        // 팩토리에서 이미 Init을 호출했다면 Start에서 기본 능력치로 다시 덮어쓰지 않습니다.
        if (isInitialized) return;

        if (myData != null) Init(myData);
        else Debug.LogWarning($"{gameObject.name}에 데이터(UnitDataSO)가 비어있습니다!");
    }

    public void Init(UnitDataSO data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data), "유닛 초기화 데이터가 비어 있습니다.");
        // 미리 생성한 비활성 객체는 Awake가 아직 실행되지 않았을 수 있어 참조를 직접 준비합니다.
        EnsureReferences();
        statusEffects.Reset();
        myData = data;
        Combat.Configure(this);
        isInitialized = true;
        pendingSpawn = true;
        SpawnVersion++;
        moveDirection = Vector2.zero;

        int playerLayerIdx = LayerMask.NameToLayer("Player");
        int enemyLayerIdx = LayerMask.NameToLayer("Enemy");

        if (playerLayerIdx == -1 || enemyLayerIdx == -1)
        {
            Debug.LogError("유니티 설정에 'Player' 또는 'Enemy' 레이어가 없습니다! 레이어 설정에서 추가해 주세요.");
        }

        if (myData.team == Team_Test.Player)
        {
            gameObject.tag = "Player";
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
        baseDamage = myData.attackDamage;
        CurrentAttackSpeed = myData.attackSpeed;
        CurrentDefense = myData.defense;
        CurrentCriticalRate = myData.criticalRate;
        CurrentCriticalDamage = myData.criticalDamage;
        AttackTimer = 0f;
        SearchTimer = 0f;
        CurrentTarget = null;

        StatMultiplier = 1f;

        ChangeState(idleState);
        // 풀의 데이터 준비는 비활성 상태에서 진행합니다. 생성 알림은 활성화 이후에만 보냅니다.
        if (isActiveAndEnabled) OnSpawned();
    }

    public void OnSpawned()
    {
        if (!isInitialized)
        {
            if (myData == null) throw new InvalidOperationException("전투 유닛을 생성하기 전에 Init(data)로 유닛 데이터를 설정해야 합니다.");
            Init(myData);
            return;
        }
        if (!pendingSpawn) return;
        pendingSpawn = false;
        spawnAnnounced = true;
        // 능력치는 이미 준비되었습니다. 애니메이터 조작은 오브젝트가 활성화된 뒤에 실행합니다.
        if (battleVisual != null)
        {
            battleVisual.Apply(this);
        }

        statusEffects.BindVisual(visualEffect);
        UnitTargetRegistry.Register(this);
        OnUnitSpawned?.Invoke(this);
        OnHpChanged?.Invoke(this, CurrentHp, CurrentMaxHp, 0f, false);
    }

    // 풀 반환 시 이전 전투의 타깃·이동·버프 타이머가 다음 사용에 남지 않도록 정리합니다.
    public void OnDespawned()
    {
        UnitTargetRegistry.Unregister(this);
        statusEffects.Reset();
        CurrentTarget = null;
        moveDirection = Vector2.zero;
        AttackTimer = 0f;
        SearchTimer = 0f;
        CurrentDamage = baseDamage;
        currentState = null;
        isInitialized = false;
        pendingSpawn = false;
        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// 웨이브 배율을 현재 유닛 능력치에 적용하고 체력 UI까지 즉시 갱신합니다.
    /// Init 직후 한 번 호출하며, SO 원본 데이터는 변경하지 않습니다.
    /// </summary>
    public void ApplyStatMultiplier(float multiplier)
    {
        if (myData == null)
        {
            Debug.LogWarning($"{gameObject.name}은 데이터가 없어 능력치 배율을 적용할 수 없습니다.");
            return;
        }

        multiplier = Mathf.Max(0f, multiplier);
        StatMultiplier = multiplier;

        CurrentMaxHp = myData.maxHp * multiplier;
        CurrentHp = CurrentMaxHp;

        baseDamage = myData.attackDamage * multiplier;
        CurrentDamage = baseDamage;
        CurrentDefense = Mathf.RoundToInt(myData.defense * multiplier);

        // 배율 적용이 끝난 실제 최대 체력을 모든 체력 UI에 전달합니다.
        OnHpChanged?.Invoke(this, CurrentHp, CurrentMaxHp, 0f, false);
    }

    private void OnDisable()
    {
        UnitTargetRegistry.Unregister(this);
        Combat.ClearTarget();
        statusEffects.Reset();
        if (spawnAnnounced && gameObject.scene.isLoaded)
        {
            spawnAnnounced = false;
            // 유닛이 죽거나 풀로 돌아갈 때 방송 송출
            OnUnitDespawned?.Invoke(this);
        }
    }

    void Update()
    {
        statusEffects.Tick(Time.deltaTime);
        if (IsFrozen) return;

        // 현재 상태의 Execute 로직을 매 프레임 실행
        if (currentState != null)
        {
            currentState.Execute(this);
        }
    }

    private void FixedUpdate()
    {
        if (rigidBody == null || myData == null || IsFrozen || currentState != moveState)
            return;

        Vector2 nextPosition = rigidBody.position
            + moveDirection * myData.moveSpeed * Time.fixedDeltaTime;

        rigidBody.MovePosition(nextPosition);
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction;
    }

    // 상태 전환을 처리하는 핵심 함수
    public void ChangeState(IUnitState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = newState;
        if (newState == attackState && battleVisual != null) battleVisual.PlayAttack();
        currentState.Enter(this);
    }

    public void TakeDamage(float amount, bool isCritical = false)
    {
        if (currentState == destroyedState) return;

        //방어력 차감 방어력이 높아서 데미지가 0이되어도 이벤트가 나오게 구현
        float finalDamage = Mathf.Max(0f, amount - CurrentDefense);

        CurrentHp -= finalDamage;

        if(amount >0f)
        {
            SoundManager.instance?.PlaySfx(finalDamage <= 0f ? "blocked" : "takedamaged");
        }

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
        CurrentHp = CurrentMaxHp;

        // 2. UI 체력바 갱신
        OnHpChanged?.Invoke(this, CurrentHp, CurrentMaxHp, 0f, false);

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

        // 2. 데이터에 설정된 실제 탐색 범위입니다.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, myData.searchRange);
    }

    // 같은 효과를 다시 적용하면 최신 배율과 지속 시간으로 갱신합니다.
    public void ApplyDamageBuff(float multiplier, float duration)
    {
        if (!isActiveAndEnabled || CurrentHp <= 0f || currentState == destroyedState) return;
        statusEffects.ApplyDamageBuff(multiplier, duration);
    }

    public void ApplyFreeze(float duration)
    {
        if (!isActiveAndEnabled || CurrentHp <= 0f || currentState == destroyedState) return;
        statusEffects.ApplyFreeze(duration);
    }
}

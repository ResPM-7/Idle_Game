using UnityEngine;

public interface IUnitState
{
    void Enter(Unit_Base_Test unit);    // 상태에 진입할 때 1회 호출
    void Execute(Unit_Base_Test unit);  // Update처럼 매 프레임 호출
    void Exit(Unit_Base_Test unit);     // 상태를 빠져나갈 때 1회 호출
}


public class Unit_Base_Test : MonoBehaviour
{
    [Header("기본 설정")]
    public UnitDataSO myData;

    [Header("타겟팅 설정")]
    public LayerMask targetLayer;

    public float currentHp;
    [HideInInspector] public float attackTimer;
    [HideInInspector] public float searchTimer;
    [HideInInspector] public Transform currentTarget;

    // FSM 관련 변수
    private IUnitState currentState;

    // 가비지 컬렉션(GC) 방지를 위해 상태 객체를 미리 생성해둡니다.
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
        currentHp = myData.maxHp;
        attackTimer = 0f;
        searchTimer = 0f;
        currentTarget = null;

        // 초기 상태 진입
        ChangeState(idleState);
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

    public void TakeDamage(float amount)
    {
        if (currentState == destroyedState) return;

        currentHp -= amount;
        if (currentHp <= 0)
        {
            ChangeState(destroyedState);
        }
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
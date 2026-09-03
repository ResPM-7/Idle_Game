using UnityEngine;

public class Unit_Base_Test : MonoBehaviour
{
    public UnitDataSO myData;
    private float currentHp;
    private float attackTimer;
    private float searchTimer;

    public UnitState currentState { get; private set; }
    private Transform currentTarget;

    private void Start()
    {
        if (myData != null)
        {
            Init(myData);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}에 데이터(UnitDataSO)가 비어있습니다!");
        }
    }

    public void Init(UnitDataSO data)
    {
        myData = data;
        currentHp = myData.maxHp;
        currentState = UnitState.Idle;
        attackTimer = 0f;
    }

    void Update()
    {
        if (currentState == UnitState.Destroyed || myData == null) return;

        attackTimer += Time.deltaTime;
        searchTimer += Time.deltaTime;

        switch (currentState)
        {
            case UnitState.Idle:
                SearchTarget();
                break;
            case UnitState.Move:
                MoveToTarget();
                break;
            case UnitState.Attack:
                // 공격 애니메이션 대기 상태 (애니메이션 이벤트로 Idle 복귀)
                break;
        }
    }

    private void SearchTarget()
    {
        // 0.2초마다 한 번씩만 물리 탐색을 실행 (최적화 핵심)
        if (searchTimer < 0.2f) return;
        searchTimer = 0f;

        // 기존 팀원 분의 OverlapCircleAll 로직 활용 (범위는 SO 데이터 사용)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, myData.attackRange * 2f);
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            Unit_Base_Test enemy = col.GetComponent<Unit_Base_Test>();
            if (enemy != null && enemy.currentState != UnitState.Destroyed)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTarget = enemy.transform;
                }
            }
        }

        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            currentState = UnitState.Move; // 타겟을 찾으면 이동 상태로 전환
        }
    }

    private void MoveToTarget()
    {
        if (currentTarget == null || currentTarget.GetComponent<Unit_Base>().currentState == UnitState.Destroyed)
        {
            currentState = UnitState.Idle;
            return;
        }

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= myData.attackRange)
        {
            // 사거리 안쪽이면 공격
            if (attackTimer >= myData.attackCooldown)
            {
                PerformAttack();
            }
        }
        else
        {
            // 사거리 밖이면 이동
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            transform.Translate(dir * myData.moveSpeed * Time.deltaTime);
        }
    }

    private void PerformAttack()
    {
        currentState = UnitState.Attack;
        attackTimer = 0f;

        // 단일 공격 로직
        Unit_Base enemy = currentTarget.GetComponent<Unit_Base>();
        if (enemy != null)
        {
            enemy.TakeDamage(myData.attackDamage);
        }

        // 광역 로직이 필요하다면 myData.aoeRadius를 확인하여 추가 처리
        currentState = UnitState.Idle; // 임시로 즉시 Idle 복귀
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            currentState = UnitState.Destroyed;
            gameObject.SetActive(false); // Destroy 대신 풀링 반환 처리
        }
    }
}
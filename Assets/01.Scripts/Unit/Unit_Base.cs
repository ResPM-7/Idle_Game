using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// 유닛의 진영
public enum Team
{
    Player,
    Monster
}

// 유닛의 현재 상태
public enum UnitState
{
    Idle, // 대기
    Move, // 이동
    Attack, // 공격
    Skill, // 스킬 사용
    Stunned, // 기절
    Destroyed // 처치됨
}

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public abstract class Unit_Base : MonoBehaviour
{
    public Team unitTeam;
    public UnitState currentState;

    public float maxHp;
    public float currentHp;
    public float attackPower;
    public float defense;
    public float moveSpeed;
    public float attackRange; // 근접은 짧게, 원거리는 길게
    public float attackCooldown; // 공격 속도

    protected float currentAttackTimer = 0f;

    protected Unit_Base currentTarget;

    protected Animator anim;
    protected SpriteRenderer spriteRenderer;

    // UI 업데이트나 사망 처리용 이벤트
    public UnityAction<Unit_Base> OnDeath;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        currentHp = maxHp;
        ChangeState(UnitState.Idle);
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                UpdateIdle();
                break;
            case UnitState.Move:
                UpdateMove();
                break;
            case UnitState.Attack:
                UpdateAttack();
                break;
            case UnitState.Skill:
                UpdateSkill();
                break;
            case UnitState.Stunned:
                // 스턴 상태일 때는 아무것도 하지 않음
                break;
            case UnitState.Destroyed:
                // 작성 중
                break;
        }
    }

    #region FSM 상태별 업데이트 로직

    protected virtual void UpdateIdle()
    {
        FindTarget();

        if (currentTarget != null)
        {
            ChangeState(UnitState.Move);
        }
    }

    protected virtual void UpdateMove()
    {
        if (currentTarget == null || currentTarget.currentState == UnitState.Destroyed)
        {
            ChangeState(UnitState.Idle);
            return;
        }

        // 타겟과의 거리 계산
        float distance = Vector2.Distance(transform.position, currentTarget.transform.position);

        if (distance <= attackRange)
        {
            // 사거리 내에 들어오면 공격 상태로 전환
            ChangeState(UnitState.Attack);
        }
        else
        {
            // 타겟을 향해 이동
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            // 방향 바라보기
            FlipSprite(direction.x);
        }
    }

    protected virtual void UpdateAttack()
    {
        if (currentTarget == null || currentTarget.currentState == UnitState.Destroyed)
        {
            ChangeState(UnitState.Idle);
            return;
        }

        // 사거리에서 벗어나면 다시 이동
        float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
        if (distance > attackRange)
        {
            ChangeState(UnitState.Move);
            return;
        }

        // 공격 쿨타임 계산
        currentAttackTimer += Time.deltaTime;
        if (currentAttackTimer >= attackCooldown)
        {
            currentAttackTimer = 0f;
            ExecuteAttack();
        }
    }

    protected virtual void UpdateSkill()
    {
        // 작성 중
    }

    #endregion

    #region 핵심 액션 메서드

    // 상태 변경 및 애니메이션 처리
    public virtual void ChangeState(UnitState newState)
    {
        if (currentState == UnitState.Destroyed) return;

        currentState = newState;

        anim.SetBool("IsMoving", false);

        switch (newState)
        {
            case UnitState.Idle:
                break;
            case UnitState.Move:
                anim.SetBool("IsMoving", true);
                break;
            case UnitState.Attack:
                anim.SetTrigger("DoAttack");
                break;
            case UnitState.Destroyed:
                anim.SetTrigger("DoDestroy");
                break;
        }
    }

    public virtual void ExecuteAttack()
    {
        if (currentTarget != null)
        {
            float damage = Mathf.Max(1, attackPower - currentTarget.defense);
            // 크리티컬 작성 중
            currentTarget.TakeDamage(damage);
        }
    }

    // 피격 처리
    public virtual void TakeDamage(float damage)
    {
        if (currentState == UnitState.Destroyed) return;

        currentHp -= damage;
        // 데미지 텍스트 팝업 작성 중
        if (currentHp <= 0)
        {
            currentHp = 0;
            DestroyUnit();
        }
        else
        {
            anim.SetTrigger("DoHit");
        }
    }

    protected virtual void DestroyUnit()
    {
        ChangeState(UnitState.Destroyed);

        // 충돌체 끄기
        GetComponent<Collider2D>().enabled = false;

        // 처치 이벤트 발생 (웨이브 매니저나 머지 보드 등에서 감지)
        OnDeath?.Invoke(this);

        // 오브젝트 풀링 할 거면 바꿔야됨
        Destroy(gameObject, 2f);
    }

    // 타겟 탐색 (오버라이드 가능하도록 가상 함수로)
    protected virtual void FindTarget()
    {
        // Physics2D.OverlapCircleAll 같은 걸 쓸 수도 있지만, 보통 방치형 게임에서는 BattleManager 같은 걸로 씬에 있는 모든 유닛 리스트를 들고 있고, 여기서 가장 가까운 적을 찾아오는 방식이 성능상 훨씬 좋아요. 그래서 현재는 비워둡니다.
    }

    protected void FlipSprite(float xDirection)
    {
        if (xDirection > 0) spriteRenderer.flipX = false; // 오른쪽
        else if (xDirection < 0) spriteRenderer.flipX = true; // 왼쪽
    }

    #endregion
}
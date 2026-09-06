using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class Unit_Base : MonoBehaviour
{
    public Team team;
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 3f;

    private List<UnitAction_Base> myActions = new List<UnitAction_Base>();

    public UnitState currentState { get; private set; }
    private UnitAction_Base currentActionToPerform;
    private Transform currentTarget;

    void Start()
    {
        currentHp = maxHp;

        // 내 오브젝트에 붙은 모든 UnitAction을 가져옴
        myActions = GetComponents<UnitAction_Base>().ToList();

        // 우선순위가 높은 순서대로 내림차순 정렬
        // 예 : 근접공격(2) 이후 원거리공격(1)
        myActions = myActions.OrderByDescending(a => a.priority).ToList();

        currentState = UnitState.Idle;
    }

    void Update()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                EvaluateActions();
                break;
            case UnitState.Move:
                MoveToTarget();
                break;
            case UnitState.Attack:
                // 애니메이션 이벤트나 별도 타이머로 상태를 Idle로 되돌려야 함
                break;
        }
    }

    private void EvaluateActions()
    {
        UnitAction_Base actionToMove = null;
        Transform targetForMove = null;

        // 우선순위가 높은 행동 조각부터 하나씩 검사
        foreach (UnitAction_Base action in myActions)
        {
            // 쿨타임이 다 차서 쓸 수 있는 상태인가?
            if (action.CanExecute())
            {
                // 원하는 타겟을 찾음(공격은 적, 힐은 아군)
                Transform target = action.FindTarget(this);
                if (target != null)
                {
                    // 타겟과의 거리 계산
                    float dist = Vector2.Distance(transform.position, target.position);

                    // 타겟이 사거리 안에 들어왔다면 실행
                    if (dist <= action.actionRange)
                    {
                        currentActionToPerform = action;
                        currentTarget = target;
                        currentState = UnitState.Attack;
                        PerformCurrentAction();
                        return;
                    }

                    if (actionToMove == null)
                    {
                        actionToMove = action;
                        targetForMove = target;
                    }
                }
            }
        }
        if (actionToMove != null)
        {
            currentActionToPerform = actionToMove;
            currentTarget = targetForMove;
            currentState = UnitState.Move;
        }
        else
        {
            // 쿨타임이거나 맵에 타겟이 없으면 대기
            currentState = UnitState.Idle;
        }
    }

    private void MoveToTarget()
    {
        // 타겟이 죽었거나 사라지면 대기
        if (currentTarget == null)
        {
            currentState = UnitState.Idle;
            return;
        }

        EvaluateActions();

        if (currentState == UnitState.Move && currentTarget != null)
        {
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            transform.Translate(dir * moveSpeed * Time.deltaTime);
        }
    }

    private void PerformCurrentAction()
    {
        if (currentTarget != null && currentActionToPerform != null)
        {
            currentActionToPerform.Execute(this, currentTarget);
        }

        currentState = UnitState.Idle;
    }

    public void TakeDamage(float amount)
    {
        float actualDamage = amount;

        currentHp -= actualDamage;
        if (currentHp <= 0)
        {
            currentState = UnitState.Destroyed;
            Destroy(gameObject);
        }
    }
}
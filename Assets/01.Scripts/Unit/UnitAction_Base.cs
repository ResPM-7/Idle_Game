using UnityEngine;

public abstract class UnitAction_Base : MonoBehaviour
{
    public float actionRange = 2f; // 행동 사거리
    public float actionCooldown = 1f; // 행동 쿨타임
    public int priority = 1; // 우선순위(높을수록 먼저 실행)

    protected float currentCooldown = 0f;

    protected virtual void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    // 쿨타임 및 실행 가능 조건 확인
    public virtual bool CanExecute()
    {
        return currentCooldown <= 0f;
    }

    // 타겟을 찾는 함수
    public abstract Transform FindTarget(Unit_Base self);

    // 실제 행동 실행
    public abstract void Execute(Unit_Base self, Transform target);
}
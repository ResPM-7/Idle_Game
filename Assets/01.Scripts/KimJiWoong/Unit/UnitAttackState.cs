using UnityEngine;
public class UnitAttackState : IUnitState
{
    public void Enter(Unit_Base_Test unit)
    {
        unit.AttackTimer = 0f;
        Unit_Base_Test enemy = unit.CurrentTarget.GetComponent<Unit_Base_Test>();

        if (enemy != null)
        {
            // 기본 데미지 설정
            float finalDamage = unit.CurrentDamage;

            // [추가] 치명타 계산 로직
            // 0 ~ 99 사이의 랜덤 숫자를 뽑아, 내 크리티컬 확률보다 낮으면 발동! (예: 확률이 20이면 0~19가 나올 때 발동)
            if (Random.Range(0, 100) < unit.MyData.criticalRate)
            {
                finalDamage *= unit.MyData.criticalDamage; // 데미지 배율 적용
                //Debug.Log($"{unit.MyData.unitName} 크리티컬 발동! 데미지: {finalDamage}");
            }

            // 적에게 최종 데미지를 입힘 (저쪽에서 방어력 연산 수행)
            enemy.TakeDamage(finalDamage);
        }

        unit.ChangeState(unit.idleState);
    }

    public void Execute(Unit_Base_Test unit) { }
    public void Exit(Unit_Base_Test unit) { }
}
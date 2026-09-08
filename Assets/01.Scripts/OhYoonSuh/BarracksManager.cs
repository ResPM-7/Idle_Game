using System.Collections.Generic;
using UnityEngine;

public class BarracksManager : Singleton<BarracksManager>
{
    public List<Unit_Base_Test> activeUnits = new List<Unit_Base_Test>();

    public float bonusFlatHp = 10f;
    public float bonusFlatAttack = 10f;

    // 업그레이드 발동 시 호출
    public void UpgradeUnits(float hpIncrease, float attackIncrease)
    {
        bonusFlatHp += hpIncrease;
        bonusFlatAttack += attackIncrease;

        // 필드에 배치된 모든 플레이어 유닛 갱신
        foreach (Unit_Base_Test unit in activeUnits)
        {
            ApplyStatsToUnit(unit, hpIncrease);
        }

        ShowNextUpgrade();
    }

    public void ApplyStatsToUnit(Unit_Base_Test unit, float lastHpIncrease = 0f)
    {
        if (unit.myData == null) return;

        float baseMaxHp = unit.myData.maxHp;

        float previousMaxHp = baseMaxHp + (bonusFlatHp - lastHpIncrease);
        float newMaxHp = baseMaxHp + bonusFlatHp;

        if (previousMaxHp > 0)
        {
            float hpRatio = unit.currentHp / previousMaxHp;
            unit.currentHp = newMaxHp * hpRatio;
        }
    }

    // 공격력 계산용 (공격할 때 실시간 호출)
    public float GetUpgradedAttack(float baseAttack)
    {
        // 퍼센트 증가 대신 상수 값 덧셈
        return baseAttack + bonusFlatAttack;
    }

    private void ShowNextUpgrade()
    {
        Debug.Log("다음 업그레이드 선택지");
    }
}
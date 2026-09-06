using System.Collections.Generic;
using UnityEngine;

public class BarracksManager : Singleton<BarracksManager>
{
    public List<Unit_Base_Test> activeUnits = new List<Unit_Base_Test>();

    public float bonusHpPercent = 0f;
    public float bonusAttackPercent = 0f;

    // 업그레이드 발동 시 호출할 메서드
    public void UpgradeUnits(float hpIncreasePercent, float attackIncreasePercent)
    {
        bonusHpPercent += hpIncreasePercent;
        bonusAttackPercent += attackIncreasePercent;

        // 필드에 배치된 모든 플레이어 유닛 갱신
        foreach (Unit_Base_Test unit in activeUnits)
        {
            ApplyStatsToUnit(unit, hpIncreasePercent);
        }

        ShowNextUpgrade();
    }

    public void ApplyStatsToUnit(Unit_Base_Test unit, float lastHpIncrease = 0f)
    {
        if (unit.myData == null) return;

        float baseMaxHp = unit.myData.maxHp;
        float previousMaxHp = baseMaxHp * (1f + (bonusHpPercent - lastHpIncrease) / 100f);
        float newMaxHp = baseMaxHp * (1f + bonusHpPercent / 100f);

        if (previousMaxHp > 0)
        {
            float hpRatio = unit.currentHp / previousMaxHp;
            unit.currentHp = newMaxHp * hpRatio;
        }
    }

    // 공격력 계산용(공격할 때 실시간 호출)
    public float GetUpgradedAttack(float baseAttack)
    {
        return baseAttack * (1f + bonusAttackPercent / 100f);
    }

    private void ShowNextUpgrade()
    {
        Debug.Log("다음 업그레이드 선택지");
    }
}
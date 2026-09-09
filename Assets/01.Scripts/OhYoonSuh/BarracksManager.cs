using System.Collections.Generic;
using UnityEngine;

public class BarracksManager : Singleton<BarracksManager>
{
    public List<Unit_Base_Test> activeUnits = new List<Unit_Base_Test>();

    private Dictionary<StatType, float> globalBuffs = new Dictionary<StatType, float>();

    public float bonusFlatHp = 10f;
    public float bonusFlatAttack = 10f;

    //구독시작
    private void OnEnable()
    {
        Unit_Base_Test.OnUnitSpawned += HandleUnitSpawned;
        Unit_Base_Test.OnUnitDespawned += HandleUnitDespawned;
    }

    //구독해제
    private void OnDisable()
    {
        Unit_Base_Test.OnUnitSpawned -= HandleUnitSpawned;
        Unit_Base_Test.OnUnitDespawned -= HandleUnitDespawned;
    }

    // 업그레이드 발동 시 호출
    public void UpgradeStat(StatType type, float increaseAmount)
    {
        if (!globalBuffs.ContainsKey(type))
            globalBuffs[type] = 0f;

        globalBuffs[type] += increaseAmount;

        foreach (Unit_Base_Test unit in activeUnits)
        {
            ApplyRealTimeStat(unit, type, increaseAmount);
        }
    }

    public float GetBuffValue(StatType type)
    {
        return globalBuffs.ContainsKey(type) ? globalBuffs[type] : 0f;
    }

    private void HandleUnitSpawned(Unit_Base_Test unit)
    {
        if (unit.CompareTag("Player"))
        {
            if (!activeUnits.Contains(unit)) activeUnits.Add(unit);

            // 방금 스폰된 유닛에게는 '모든 스탯'을 한 번에 발라줍니다.
            ApplyAllStatsToNewUnit(unit);
        }
    }

    private void HandleUnitDespawned(Unit_Base_Test unit)
    {
        if (activeUnits.Contains(unit)) activeUnits.Remove(unit);
    }

    // 갓 스폰된 유닛 초기화 전용
    private void ApplyAllStatsToNewUnit(Unit_Base_Test unit)
    {
        if (unit.myData == null) return;

        unit.currentHp = unit.myData.maxHp + GetBuffValue(StatType.Health);
        unit.currentDamage = unit.myData.attackDamage + GetBuffValue(StatType.Strength);

        // 예시: unit.attackSpeed = unit.myData.attackSpeed + GetBuffValue(StatType.AttackSpeed);
        // 예시: unit.magicPower = unit.myData.magicPower + GetBuffValue(StatType.MagicPower);
    }

    // 이미 싸우고 있는 유닛 실시간 갱신용
    private void ApplyRealTimeStat(Unit_Base_Test unit, StatType type, float amount)
    {
        if (unit.myData == null) return;

        switch (type)
        {
            case StatType.Health:
                float baseMaxHp = unit.myData.maxHp;
                float currentMaxHp = baseMaxHp + GetBuffValue(StatType.Health);
                float previousMaxHp = currentMaxHp - amount;

                if (previousMaxHp > 0)
                {
                    float hpRatio = unit.currentHp / previousMaxHp;
                    unit.currentHp = currentMaxHp * hpRatio;
                }
                break;

            case StatType.Strength:
                unit.currentDamage = unit.myData.attackDamage + GetBuffValue(StatType.Strength);
                break;

                // 새로운 스탯이 추가되면 아래에 case만 추가하면 완벽하게 작동합니다!
                /*
                case StatType.AttackSpeed:
                    unit.attackSpeed = unit.myData.attackSpeed + GetBuffValue(StatType.AttackSpeed);
                    break;
                */
        }
    }
}
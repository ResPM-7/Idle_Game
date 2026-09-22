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
        if (unit.MyData == null) return;

        // 병영 강화까지 반영한 실제 최대 체력
        unit.CurrentMaxHp =
            unit.MyData.maxHp + GetBuffValue(StatType.Health);

        // 새로 생성된 유닛은 강화된 최대 체력으로 시작
        unit.CurrentHp = unit.CurrentMaxHp;

        unit.CurrentDamage =
            unit.MyData.attackDamage + GetBuffValue(StatType.Strength);
        ApplyRealTimeStat(unit, StatType.Defense, 0);
        ApplyRealTimeStat(unit, StatType.CriticalRate, 0);
        ApplyRealTimeStat(unit, StatType.CriticalDamage, 0);
    }

    // 이미 싸우고 있는 유닛 실시간 갱신용
    private void ApplyRealTimeStat(Unit_Base_Test unit, StatType type, float amount)
    {
        if (unit.MyData == null) return;

        switch (type)
        {
            case StatType.Health:
                float previousMaxHp = unit.CurrentMaxHp;

                float newMaxHp =
                    unit.MyData.maxHp + GetBuffValue(StatType.Health);

                // 현재 체력 비율을 유지하며 최대 체력만 갱신
                float hpRatio = previousMaxHp > 0f
                    ? unit.CurrentHp / previousMaxHp
                    : 1f;

                unit.CurrentMaxHp = newMaxHp;
                unit.CurrentHp = newMaxHp * hpRatio;
                break;

            case StatType.Strength:
                unit.CurrentDamage = unit.MyData.attackDamage + GetBuffValue(StatType.Strength);
                break;

            case StatType.Defense:
                unit.CurrentDefense =
                    unit.MyData.defense +
                    Mathf.RoundToInt(GetBuffValue(StatType.Defense));
                break;

            case StatType.CriticalRate:
                unit.CurrentCriticalRate = Mathf.Clamp(
                    unit.MyData.criticalRate +
                    Mathf.RoundToInt(GetBuffValue(StatType.CriticalRate)),
                    0,
                    100
                );
                break;

            case StatType.CriticalDamage:
                unit.CurrentCriticalDamage =
                    unit.MyData.criticalDamage +
                    GetBuffValue(StatType.CriticalDamage) / 100f;
                break;
        }
    }
}

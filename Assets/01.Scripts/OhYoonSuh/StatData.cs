using UnityEngine;

public class StatData : IStatData
{
    public StatType Type { get; set; }
    public string StatName { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public float CurrentValue { get; set; }
    public int UpgradePrice { get; set; }

    public float ValueIncreasePerLevel { get; set; }
    public float PriceMultiplier { get; set; } = 1.2f;

    // 업그레이드 실행 메서드
    public void LevelUp()
    {
        CurrentLevel++;
        CurrentValue += ValueIncreasePerLevel;

        // 현재 가격에 배율을 적용하고 소수점은 올림 처리
        UpgradePrice = Mathf.CeilToInt(
            UpgradePrice * PriceMultiplier
        );
    }
}
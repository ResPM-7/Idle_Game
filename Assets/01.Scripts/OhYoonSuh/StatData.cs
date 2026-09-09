public class StatData : IStatData
{
    public StatType Type { get; set; }
    public string StatName { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public float CurrentValue { get; set; }
    public int UpgradePrice { get; set; }

    public float ValueIncreasePerLevel { get; set; }
    public int PriceIncreasePerLevel { get; set; }

    // 업그레이드 실행 메서드
    public void LevelUp()
    {
        CurrentLevel++;
        CurrentValue += ValueIncreasePerLevel;
        UpgradePrice += PriceIncreasePerLevel;
    }
}
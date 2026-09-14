public enum StatType
{
    Strength,    // 힘
    Health,      // 체력
   
    Dummy,

    //스탯 데이터 확장 시 추가
}

public interface IStatData
{
    StatType Type { get; } //스탯 타입
    string StatName { get; } //스탯 이름 
    int CurrentLevel { get; } //현재 레벨
    float CurrentValue { get; } //현재 증가수치
    int UpgradePrice { get; } //업그레이드 비용
}
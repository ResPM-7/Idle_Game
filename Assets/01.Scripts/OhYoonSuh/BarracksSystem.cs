using System.Collections.Generic;
using UnityEngine;

public class BarracksSystem : MonoBehaviour
{
    [SerializeField] private StatUpgradeItem healthUIItem;
    [SerializeField] private StatUpgradeItem strengthUIItem;

    private Dictionary<StatType, StatData> statDatabase = new Dictionary<StatType, StatData>();

    private void Start()
    {
        statDatabase[StatType.Health] = new StatData
        {
            Type = StatType.Health,
            StatName = "체력 증가",
            CurrentValue = 0f,
            ValueIncreasePerLevel = 10f,  // 1업당 체력 10 고정 증가
            UpgradePrice = 100,
            PriceIncreasePerLevel = 50
        };

        statDatabase[StatType.Strength] = new StatData
        {
            Type = StatType.Strength,
            StatName = "공격력 증가",
            CurrentValue = 0f,
            ValueIncreasePerLevel = 10f,   // 1업당 공격력 10 고정 증가
            UpgradePrice = 100,
            PriceIncreasePerLevel = 50
        };

        if (healthUIItem != null)
            healthUIItem.Setup(statDatabase[StatType.Health], OnUpgradeClicked);

        if (strengthUIItem != null)
            strengthUIItem.Setup(statDatabase[StatType.Strength], OnUpgradeClicked);
    }

    private void OnUpgradeClicked(StatType type)
    {
        StatData data = statDatabase[type];

        if (!MoneyManager.instance.SpendGold(data.UpgradePrice))
        {
            return;
        }

        data.LevelUp();

        if (type == StatType.Health)
            healthUIItem.UpdateUI(data);
        else if (type == StatType.Strength)
            strengthUIItem.UpdateUI(data);

        if (type == StatType.Health)
            BarracksManager.instance.UpgradeUnits(data.ValueIncreasePerLevel, 0f);
        else if (type == StatType.Strength)
            BarracksManager.instance.UpgradeUnits(0f, data.ValueIncreasePerLevel);
    }
}

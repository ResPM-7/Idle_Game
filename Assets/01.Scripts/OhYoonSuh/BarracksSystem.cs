using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatConfig
{
    public StatType type;               // 스탯 종류 (Health, Strength, AttackSpeed 등)
    public string statName;             // UI에 표시될 이름
    public float valueIncreasePerLevel; // 1업당 증가량
    public int baseUpgradePrice;        // 기본 강화 비용
    [Min(1f)]
    public float priceMultiplier;// 레벨당 비용 증가량
    //public StatUpgradeItem uiItem;      // 연결할 UI 오브젝트
}

public class BarracksSystem : MonoBehaviour
{
    [SerializeField] private StatConfig[] statConfigs;

    private Dictionary<StatType, StatData> statDatabase = new Dictionary<StatType, StatData>();
    public bool IsInitialized => statDatabase.Count > 0;

    public List<SavedUpgrade> CaptureUpgrades()
    {
        var result = new List<SavedUpgrade>();
        foreach (var item in statDatabase.Values)
            result.Add(new SavedUpgrade { type = item.Type, level = item.CurrentLevel });
        return result;
    }

    public void RestoreUpgrades(List<SavedUpgrade> upgrades)
    {
        // 강화 횟수로 현재 설정의 가격/능력치를 재계산합니다.
        foreach (var config in statConfigs)
        {
            var data = statDatabase[config.type];
            int level = upgrades.Find(x => x.type == config.type)?.level ?? 1;
            data.CurrentLevel = 1;
            data.CurrentValue = 0;
            data.UpgradePrice = config.baseUpgradePrice;
            for (int i = 1; i < level; i++) data.LevelUp();
            float previous = BarracksManager.instance.GetBuffValue(config.type);
            BarracksManager.instance.UpgradeStat(config.type, data.CurrentValue - previous);
            if (UIManager.Instance != null) UIManager.Instance.RefreshUpgradeStatItem(data);
        }
    }
    //private Dictionary<StatType, StatUpgradeItem> uiDatabase = new Dictionary<StatType, StatUpgradeItem>();

    private void Start()
    {
        // UIManager의 동적 UI 생성에 전달할 스탯 리스트
        List<IStatData> statList = new List<IStatData>();

        foreach (var config in statConfigs)
        {
            // 1. 데이터 세팅
            StatData newData = new StatData
            {
                Type = config.type,
                StatName = config.statName,
                CurrentValue = 0f,
                ValueIncreasePerLevel = config.valueIncreasePerLevel,
                UpgradePrice = config.baseUpgradePrice,
                PriceMultiplier = Mathf.Max(1f, config.priceMultiplier)
            };

            statDatabase[config.type] = newData;
            statList.Add(newData);
        }

        //// 2. UI 매핑 및 초기화
        //if (config.uiItem != null)
        //{
        //    uiDatabase[config.type] = config.uiItem;
        //    config.uiItem.Setup(newData, OnUpgradeClicked);
        //}

        // 2. UIManager를 통해 UI 탭 항목들을 자동으로 동적 생성
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitUpgradeTab(statList, OnUpgradeClicked);
        }
    }

    private void OnUpgradeClicked(StatType type)
    {
        // 혹시 모를 에러 방지 (데이터가 없으면 무시)
        if (!statDatabase.ContainsKey(type)) return;

        StatData data = statDatabase[type];

        if (MoneyManager.instance == null || BarracksManager.instance == null ||
            !MoneyManager.instance.SpendGold(data.UpgradePrice))
        {
            Debug.Log($"골드가 부족하여 {data.StatName} 업그레이드 실패!");
            return;
        }

        data.LevelUp();


        // 변경된 스탯 데이터를 UIManager를 통해 UI 단일 항목 새로고침
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshUpgradeStatItem(data);
        }

        // 매니저 호출도 한 줄로 끝!
        BarracksManager.instance.UpgradeStat(type, data.ValueIncreasePerLevel);
    }
}

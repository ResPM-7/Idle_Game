
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatConfig
{
    public StatType type;               // 스탯 종류 (Health, Strength, AttackSpeed 등)
    public string statName;             // UI에 표시될 이름
    public float valueIncreasePerLevel; // 1업당 증가량
    public int baseUpgradePrice;        // 기본 강화 비용
    public int priceIncreasePerLevel;   // 레벨당 비용 증가량
    //public StatUpgradeItem uiItem;      // 연결할 UI 오브젝트
}

public class BarracksSystem : MonoBehaviour
{
    [SerializeField] private StatConfig[] statConfigs;

    private Dictionary<StatType, StatData> statDatabase = new Dictionary<StatType, StatData>();
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
                PriceIncreasePerLevel = config.priceIncreasePerLevel
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

        if (MoneyManager.instance != null && !MoneyManager.instance.SpendGold(data.UpgradePrice))
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
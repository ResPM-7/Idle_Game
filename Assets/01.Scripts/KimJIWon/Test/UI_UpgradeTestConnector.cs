using System.Collections.Generic;
using UnityEngine;

// 팀원 데이터 클래스가 없을 때 혼자서 UI를 테스트하기 위한 임시 데이터
public class DummyStat : IStatData
{
    public StatType Type { get; set; }
    public string StatName { get; set; }
    public int CurrentLevel { get; set; }
    public float CurrentValue { get; set; }
    public int UpgradePrice { get; set; }
}

public class UI_UpgradeTestConnector : MonoBehaviour
{
    private List<IStatData> dummyStats;

    private void Start()
    {
        //팀원 매니저 대신 임시 데이터 세팅
        dummyStats = new List<IStatData>
        {
            new DummyStat { Type = StatType.Strength, StatName = "공격력", CurrentLevel = 1, CurrentValue = 10f, UpgradePrice = 100 },
            new DummyStat { Type = StatType.Health, StatName = "체력", CurrentLevel = 1, CurrentValue = 100f, UpgradePrice = 150 },
            new DummyStat { Type = StatType.AttackSpeed, StatName = "공격속도", CurrentLevel = 1, CurrentValue = 1.0f, UpgradePrice = 200 },

            // 추가 확장 테스트 더미
            new DummyStat { Type = StatType.Dummy, StatName = "더미 스탯", CurrentLevel = 1, CurrentValue = 99f, UpgradePrice = 500 }
        };

        //UIManager에 데이터 전달하여 UI 생성
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitUpgradeTab(dummyStats, OnUpgradeRequested);
        }
    }

    //강화 버튼을 눌렀을 때 실행되는 테스트용 로직
    private void OnUpgradeRequested(StatType type)
    {
        var target = dummyStats.Find(s => s.Type == type) as DummyStat;
        if (target != null)
        {
            target.CurrentLevel++;
            target.CurrentValue += 5f;
            target.UpgradePrice += 50;

            // UI_UpgradeTab 갱신 함수 호출
            var upgradeTab = FindAnyObjectByType<UI_UpgradeTab>();
            if (upgradeTab != null)
            {
                upgradeTab.RefreshStatItem(target);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatUpgradeItem : MonoBehaviour
{
    [Header("UI Text Components")]
    [SerializeField] private TMP_Text statNameText;  // 1. 스탯 이름
    [SerializeField] private TMP_Text levelText;     // 2. 현재 레벨
    [SerializeField] private TMP_Text valueText;     // 3. 현재 증가 수치
    [SerializeField] private TMP_Text priceText;      // 4. 업그레이드 비용

    [Header("UI Button")]
    [SerializeField] private Button upgradeButton;   // 강화 버튼

    public StatType TargetStatType { get; private set; }

    //초기 설정
    public void Setup(IStatData initialData, System.Action<StatType> onUpgradeClick)
    {
        TargetStatType = initialData.Type;

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => onUpgradeClick?.Invoke(TargetStatType));
        }

        UpdateUI(initialData);
    }

    // 데이터가 업데이트되거나 초기화될 때 4개 텍스트 각각 갱신
    public void UpdateUI(IStatData data)
    {
        if (data == null || data.Type != TargetStatType) return;

        //스탯 이름
        if (statNameText != null)
            statNameText.text = data.StatName;

        //현재 레벨
        if (levelText != null)
            levelText.text = $"Lv.{data.CurrentLevel}";

        //현재 증가 수치 (공격속도는 소수점 F2, 일반 스탯은 F0 표기)
        if (valueText != null)
        {
            string format = (data.Type == StatType.AttackSpeed) ? "F2" : "F0";
            valueText.text = $"+{data.CurrentValue.ToString(format)}";
        }

        //업그레이드 비용
        if (priceText != null)
            priceText.text = $"{data.UpgradePrice} G";
    }
}
using System;
using TMPro;
using UnityEngine;

public class UI_CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text amountText;

    private static readonly string[] Units =
    {
        "K", "M", "B", "T", "Qa", "Qi"
    };

    public void SetAmount(long amount)
    {
        //TMP_text 미할당 방어처리
        if (amountText == null)
        {
            Debug.LogWarning($"{name}: Amount Text가 연결되지 않았습니다.");
            return;
        }

        amountText.text = FormatAmount(amount);
    }

    private string FormatAmount(long amount)
    {
        double value = amount;
        int unitIndex = -1;

        // 1,000 단위로 축소
        while (Math.Abs(value) >= 1000d && unitIndex + 1 < Units.Length)
        {
            value /= 1000d;
            unitIndex++;
        }

        // 999.95K 등이 1000.0K로 표시되지 않고 1.0M이 되도록 처리
        while (unitIndex + 1 < Units.Length &&
               Math.Round(Math.Abs(value), 1) >= 1000d)
        {
            value /= 1000d;
            unitIndex++;
        }

        // 1,000 미만: 999 / 500처럼 일반 숫자 표시
        if (unitIndex < 0)
            return amount.ToString("N0");

        // 1,000 이상: 10.5K / 10.5M / 10.5B
        return $"{value.ToString("0.0")}{Units[unitIndex]}";
    }
}
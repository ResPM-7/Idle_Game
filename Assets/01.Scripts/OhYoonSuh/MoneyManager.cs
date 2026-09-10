using System;
using TMPro;
using UnityEngine;

public class MoneyManager : Singleton<MoneyManager>
{
    public int currentGold = 0;
    public int currentCredit = 0;

    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI creditText;

    // 값이 변경될 때마다 UI 등에 알리기 위한 이벤트
    public Action<int> OnGoldChanged;
    public Action<int> OnCreditChanged;

    private void Start()
    {
        OnGoldChanged += UpdateGoldUI;
        OnCreditChanged += UpdateCreditUI;

        // 시작할 때 기본 소지금을 UI에 띄워줍니다.
        UpdateGoldUI(currentGold);
        UpdateCreditUI(currentCredit);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy(); // Singleton.cs에 OnDestroy가 있다면 base 호출

        OnGoldChanged -= UpdateGoldUI;
        OnCreditChanged -= UpdateCreditUI;
    }


    private void UpdateGoldUI(int gold)
    {
        if (coinText != null)
        {
            // "N0" 포맷을 사용하면 1000이 1,000처럼 세 자리마다 쉼표가 예쁘게 찍힙니다!
            coinText.text = gold.ToString("N0");
        }
    }

    private void UpdateCreditUI(int credit)
    {
        if (creditText != null)
        {
            creditText.text = credit.ToString("N0");
        }
    }



    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }

        Debug.Log("골드가 부족합니다.");
        return false;
    }

    public void AddCredit(int amount)
    {
        currentCredit += amount;
        OnCreditChanged?.Invoke(currentCredit);
    }

    public bool SpendCredit(int amount)
    {
        if (currentCredit >= amount)
        {
            currentCredit -= amount;
            OnCreditChanged?.Invoke(currentCredit);
            return true;
        }
        return false;
    }
}
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
    public event Action<int> OnGoldChanged;
    public event Action<int> OnCreditChanged;

    protected override void OnDestroy()
    {
        base.OnDestroy(); // Singleton.cs에 OnDestroy가 있다면 base 호출
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
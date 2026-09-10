using System.Collections.Generic;

using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public static UIManager Instance => instance;

    [Header("Managers")]
    [SerializeField] private UI_PopUpManager popUpManager;
   
    protected override void Awake()
    {
        base.Awake();

        if (instance != this) return;

        //팝업 매니저 등록 (없을 시)
        if (popUpManager == null)
            popUpManager = FindAnyObjectByType<UI_PopUpManager>();
    }

    private void Start()
    {
        moneyManager = MoneyManager.instance;

        // 초기 보유 재화 표시
        goldDisplay.SetAmount(moneyManager.currentGold);
        creditDisplay.SetAmount(moneyManager.currentCredit);

        // 이후 재화 변경 감지
        moneyManager.OnGoldChanged += HandleGoldChanged;
        moneyManager.OnCreditChanged += HandleCreditChanged;
    }

    #region PopUp
    public T ShowPopup<T>(T popupPrefab) where T : MonoBehaviour
    {
        if (popUpManager != null)
            return popUpManager.ShowPopup<T>(popupPrefab);

        Debug.LogWarning("UI_PopUpManager가 UIManager에 연결되지 않았습니다.");
        return null;
    }
    public void ShowPopupGameObject(GameObject popupObject)
    {
        if (popUpManager != null)
            popUpManager.ShowPopupGameObject(popupObject);
    }

    public void CloseTopPopup()
    {
        if (popUpManager != null)
            popUpManager.CloseTopPopup();
    }

    public void CloseAllPopups()
    {
        if (popUpManager != null)
            popUpManager.CloseAllPopups();
    }
    #endregion

    #region InGame UI / Floating Text
    public void ShowDamageText(float damage, Vector3 worldPos)
    {
        const string poolKey = "DamageText";

        if (ObjectPoolManager.instance == null)
            return;

        GameObject textObj = ObjectPoolManager.instance.GetObject(poolKey);
        if (textObj == null)
        {
            Debug.LogWarning($"'{poolKey}' 풀에서 오브젝트를 가져오지 못했습니다.");
            return;
        }

        if (!textObj.TryGetComponent<UI_DamageText>(out var damageText))
        {
            Debug.LogError($"'{poolKey}' 프리팹에 UI_DamageText가 없습니다.");
            ObjectPoolManager.instance.ReturnObject(poolKey, textObj);
            return;
        }

        damageText.Setup(damage, worldPos, poolKey);
    }
    #endregion

    #region Tab UI
    [Header("Tab UI References")]
    [SerializeField] private UI_UpgradeTab upgradeTab;

    // 게임 시작 시 단 한 번 외부(데이터 매니저)에서 스탯 리스트를 전달받아 탭을 세팅함 <- BarrackSystem.cs  
    public void InitUpgradeTab(List<IStatData> statList, System.Action<StatType> onUpgradeRequest)
    {
        if (upgradeTab == null)
            upgradeTab = FindAnyObjectByType<UI_UpgradeTab>();

        if (upgradeTab != null)
        {
            upgradeTab.InitTab(statList, onUpgradeRequest);
        }
    }
    // 업그레이드 수치 변경 시 특정 스탯 UI 항목만 단일 갱신
    public void RefreshUpgradeStatItem(IStatData updatedData)
    {
        if (upgradeTab == null)
            upgradeTab = FindAnyObjectByType<UI_UpgradeTab>();

        if (upgradeTab != null)
        {
            upgradeTab.RefreshStatItem(updatedData);
        }
    }
    #endregion

    #region Currency Display
    [Header("Currency UI")]
    [SerializeField] private UI_CurrencyDisplay goldDisplay;
    [SerializeField] private UI_CurrencyDisplay creditDisplay;

    private MoneyManager moneyManager;

    private void HandleGoldChanged(int gold)
    {
        goldDisplay.SetAmount(gold);
    }

    private void HandleCreditChanged(int credit)
    {
        creditDisplay.SetAmount(credit);
    }

    protected override void OnDestroy()
    {
        if (moneyManager != null)
        {
            moneyManager.OnGoldChanged -= HandleGoldChanged;
            moneyManager.OnCreditChanged -= HandleCreditChanged;
        }

        base.OnDestroy();
    }
    #endregion
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour
{
    [Header("소환 설정")]
    [SerializeField] private Transform gridPanel;

    [Tooltip("길드 등급 1에서 소환되는 기본 유닛입니다.")]
    [SerializeField] private UnitDataSO baseUnitData;

    [Tooltip("길드 등급 2부터 순서대로 소환할 유닛을 넣습니다. 배열의 0번은 길드 등급 2입니다.")]
    [SerializeField] private UnitDataSO[] upgradedSpawnData;

    [Header("소환 비용")]
    [SerializeField, Min(0)] private int spawnCost = 10;

    [Header("길드 승급 설정")]
    [Tooltip("게임 시작 시 길드 등급입니다. 기본값은 1입니다.")]
    [SerializeField, Min(1)] private int guildLevel = 1;

    [Tooltip("길드 등급 1에서 2로 승급할 때 필요한 골드입니다.")]
    [SerializeField, Min(0)] private int basePromotionCost = 100;

    [Tooltip("다음 승급 비용에 적용되는 배율입니다. 예: 1.5이면 100, 150, 225 순서로 증가합니다.")]
    [SerializeField, Min(1f)] private float promotionCostMultiplier = 1.5f;

    [Header("길드 승급 UI (선택)")]
    [Tooltip("길드 승급 페이지 오른쪽 아래에 있는 승급 버튼을 연결합니다.")]
    [SerializeField] private Button guildPromotionButton;

    [Tooltip("현재 길드 등급을 표시할 텍스트입니다. 없어도 기능은 동작합니다.")]
    [SerializeField] private TMP_Text guildLevelText;

    [Tooltip("다음 승급 비용을 표시할 텍스트입니다. 없어도 기능은 동작합니다.")]
    [SerializeField] private TMP_Text promotionCostText;

    [Tooltip("현재 소환되는 유닛을 표시할 텍스트입니다. 없어도 기능은 동작합니다.")]
    [SerializeField] private TMP_Text summonUnitText;

    private readonly List<Transform> gridSlots = new List<Transform>();
    private UnitDataSO currentSpawnData;
    private WaveManager subscribedWaveManager;

    /// <summary>
    /// 현재 길드 등급입니다. 외부 UI에서도 읽을 수 있지만 직접 변경할 수는 없습니다.
    /// </summary>
    public int GuildLevel => guildLevel;

    /// <summary>
    /// 기본 유닛을 포함한 최대 길드 등급입니다.
    /// </summary>
    public int MaxGuildLevel => 1 + (upgradedSpawnData?.Length ?? 0);

    private void Awake()
    {
        CacheGridSlots();

        // 스테이지가 아니라 현재 길드 등급을 기준으로 소환 데이터를 정합니다.
        ClampGuildLevel();
        RefreshSpawnData();
        RefreshGuildPromotionUI();
    }

    private void OnEnable()
    {
        // 버튼을 인스펙터에 연결하면 별도로 OnClick을 설정하지 않아도 됩니다.
        if (guildPromotionButton != null)
        {
            guildPromotionButton.onClick.RemoveListener(PromoteGuild);
            guildPromotionButton.onClick.AddListener(PromoteGuild);
        }

        // 스테이지가 바뀔 때 소환 유닛을 자동 변경하지는 않습니다.
        // 승급 조건을 만족했는지 UI만 다시 확인합니다.
        subscribedWaveManager = WaveManager.instance;
        if (subscribedWaveManager != null)
            subscribedWaveManager.OnStageChanged += HandleStageChanged;

        RefreshGuildPromotionUI();
    }

    private void OnDisable()
    {
        if (guildPromotionButton != null)
            guildPromotionButton.onClick.RemoveListener(PromoteGuild);

        if (subscribedWaveManager != null)
        {
            subscribedWaveManager.OnStageChanged -= HandleStageChanged;
            subscribedWaveManager = null;
        }
    }

    private void CacheGridSlots()
    {
        gridSlots.Clear();

        if (gridPanel == null)
        {
            Debug.LogError("UnitSpawner의 gridPanel이 연결되지 않았습니다.", this);
            return;
        }

        foreach (Transform child in gridPanel)
            gridSlots.Add(child);

        Debug.Log($"초기화 완료: 총 {gridSlots.Count}개의 그리드 슬롯이 저장되었습니다.");
    }

    /// <summary>
    /// 길드 승급 버튼에서 호출하는 함수입니다.
    /// 골드가 충분하면 길드 등급과 소환 유닛 등급을 한 단계 올립니다.
    /// </summary>
    public void PromoteGuild()
    {
        TryPromoteGuild();
    }

    /// <summary>
    /// 코드에서 승급 성공 여부가 필요할 때 사용하는 함수입니다.
    /// </summary>
    public bool TryPromoteGuild()
    {
        if (guildLevel >= MaxGuildLevel)
        {
            Debug.Log("이미 길드 최대 등급입니다.");
            RefreshGuildPromotionUI();
            return false;
        }

        int requiredStage = GetRequiredStageForNextPromotion();
        if (!HasReachedRequiredStage())
        {
            Debug.Log($"길드 {guildLevel + 1}등급 승급에는 Stage {requiredStage}-1 도달이 필요합니다.");
            RefreshGuildPromotionUI();
            return false;
        }

        // 길드 등급 1 -> 2일 때 upgradedSpawnData[0]을 사용합니다.
        int nextUnitIndex = guildLevel - 1;
        if (upgradedSpawnData == null || upgradedSpawnData[nextUnitIndex] == null)
        {
            Debug.LogWarning($"길드 등급 {guildLevel + 1}에 사용할 유닛 데이터가 없습니다.", this);
            return false;
        }

        if (MoneyManager.instance == null)
        {
            Debug.LogWarning("MoneyManager를 찾을 수 없어 길드 승급을 진행할 수 없습니다.", this);
            return false;
        }

        int promotionCost = GetCurrentPromotionCost();
        if (!MoneyManager.instance.SpendGold(promotionCost))
        {
            Debug.Log($"길드 승급에 필요한 골드가 부족합니다. 필요 골드: {promotionCost}");
            return false;
        }

        guildLevel++;
        RefreshSpawnData();
        RefreshGuildPromotionUI();

        Debug.Log($"길드가 {guildLevel}등급으로 승급했습니다. 이제 {currentSpawnData.unitName} 유닛을 소환합니다.");
        return true;
    }

    /// <summary>
    /// 다음 길드 등급에 필요한 스테이지입니다.
    /// 2등급은 Stage 2-1, 3등급은 Stage 3-1이 필요합니다.
    /// </summary>
    public int GetRequiredStageForNextPromotion()
    {
        return guildLevel + 1;
    }

    /// <summary>
    /// 현재 다음 승급의 스테이지 선행조건을 만족했는지 확인합니다.
    /// </summary>
    public bool HasReachedRequiredStage()
    {
        if (guildLevel >= MaxGuildLevel)
            return true;

        WaveManager waveManager = WaveManager.instance;
        return waveManager != null
            && waveManager.CurrentStage >= GetRequiredStageForNextPromotion();
    }

    /// <summary>
    /// 현재 길드 등급에서 다음 등급으로 승급할 때 필요한 골드를 계산합니다.
    /// </summary>
    public int GetCurrentPromotionCost()
    {
        if (guildLevel >= MaxGuildLevel)
            return 0;

        float calculatedCost = basePromotionCost
            * Mathf.Pow(promotionCostMultiplier, guildLevel - 1);

        return Mathf.Max(0, Mathf.RoundToInt(calculatedCost));
    }

    public void SpawnTestUnit()
    {
        if (currentSpawnData == null)
        {
            Debug.LogWarning("현재 소환할 유닛 데이터가 없습니다.", this);
            return;
        }

        Transform targetSlot = FindEmptyGridSlot();
        if (targetSlot == null)
        {
            Debug.Log("그리드가 꽉 찼습니다! 소환할 수 없습니다.");
            return;
        }

        if (MoneyManager.instance == null)
        {
            Debug.LogWarning("MoneyManager를 찾을 수 없어 유닛을 소환할 수 없습니다.", this);
            return;
        }

        if (!MoneyManager.instance.SpendCredit(spawnCost))
        {
            Debug.Log($"유닛 소환에 필요한 크레딧이 부족합니다. 필요 크레딧: {spawnCost}");
            return;
        }

        if (GridUnitFactory.instance == null)
        {
            // 팩토리가 없을 때 이미 차감한 크레딧을 되돌려 줍니다.
            MoneyManager.instance.AddCredit(spawnCost);
            Debug.LogWarning("GridUnitFactory를 찾을 수 없어 유닛을 소환할 수 없습니다.", this);
            return;
        }

        GridUnitFactory.instance.CreateUnit(
            currentSpawnData.uiPoolName,
            currentSpawnData,
            targetSlot);
    }

    private Transform FindEmptyGridSlot()
    {
        foreach (Transform slot in gridSlots)
        {
            if (slot != null && slot.childCount == 0)
                return slot;
        }

        return null;
    }

    private void ClampGuildLevel()
    {
        guildLevel = Mathf.Clamp(guildLevel, 1, Mathf.Max(1, MaxGuildLevel));
    }

    private void RefreshSpawnData()
    {
        currentSpawnData = baseUnitData;

        if (guildLevel <= 1 || upgradedSpawnData == null)
            return;

        int unitIndex = guildLevel - 2;
        if (unitIndex >= 0
            && unitIndex < upgradedSpawnData.Length
            && upgradedSpawnData[unitIndex] != null)
        {
            currentSpawnData = upgradedSpawnData[unitIndex];
        }
    }

    private void RefreshGuildPromotionUI()
    {
        bool isMaxLevel = guildLevel >= MaxGuildLevel;
        bool hasReachedRequiredStage = HasReachedRequiredStage();

        if (guildLevelText != null)
            guildLevelText.text = $"길드 등급 {guildLevel}";

        if (promotionCostText != null)
        {
            promotionCostText.text = isMaxLevel
                ? "최대 등급"
                : $"승급 조건 Stage {GetRequiredStageForNextPromotion()}-1\n"
                    + $"승급 비용 {GetCurrentPromotionCost():N0} G";
        }

        if (summonUnitText != null)
        {
            summonUnitText.text = currentSpawnData != null
                ? $"소환 유닛 Lv.{currentSpawnData.unitLevel} {currentSpawnData.unitName}"
                : "소환 유닛 미설정";
        }

        if (guildPromotionButton != null)
            guildPromotionButton.interactable = !isMaxLevel && hasReachedRequiredStage;
    }

    private void HandleStageChanged(int stage)
    {
        // stage 값으로 유닛을 바꾸지 않고 승급 버튼 상태만 갱신합니다.
        RefreshGuildPromotionUI();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        basePromotionCost = Mathf.Max(0, basePromotionCost);
        promotionCostMultiplier = Mathf.Max(1f, promotionCostMultiplier);
        ClampGuildLevel();

        // 플레이 중 인스펙터 값을 바꾸면 UI에서도 바로 확인할 수 있게 갱신합니다.
        if (Application.isPlaying)
        {
            RefreshSpawnData();
            RefreshGuildPromotionUI();
        }
    }
#endif
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파티에 편성된 유닛이 없을 때 전투 시작 버튼을 비활성화합니다.
/// 전투가 진행 중일 때는 중지/포기 버튼을 계속 사용할 수 있습니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class PartyStartButtonGuard : MonoBehaviour
{
    [Header("선택 UI")]
    [Tooltip("파티가 비어 있을 때 안내 문구를 표시할 TMP 텍스트입니다. 연결하지 않아도 버튼 차단은 동작합니다.")]
    [SerializeField] private TMP_Text emptyPartyNoticeText;

    [SerializeField] private string emptyPartyMessage =
        "파티에 유닛을 1명 이상 편성해야 합니다.";

    private Button battleButton;

    private void Awake()
    {
        battleButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        RefreshButtonState();
    }

    private void Update()
    {
        // 파티 슬롯은 드래그로 언제든 변경될 수 있으므로 버튼 상태를 계속 맞춥니다.
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        if (battleButton == null)
            return;

        WaveManager waveManager = WaveManager.instance;
        PartyBuildManager partyManager = PartyBuildManager.instance;

        // 매니저 초기화 전에는 잘못된 전투 시작을 방지하기 위해 버튼을 잠급니다.
        if (waveManager == null || partyManager == null)
        {
            battleButton.interactable = false;
            SetNoticeVisible(false);
            return;
        }

        bool isWaitingToStart = waveManager.stageGiveUp;
        bool hasPartyUnit = partyManager.GetActiveUnitCount() > 0;
        bool shouldBlockStart = isWaitingToStart && !hasPartyUnit;

        // 시작 대기 상태에서 파티가 비었을 때만 막습니다.
        // 전투 진행 중에는 파티 유닛이 사망했더라도 중지/포기 버튼을 사용할 수 있습니다.
        battleButton.interactable = !shouldBlockStart;
        SetNoticeVisible(shouldBlockStart);
    }

    private void SetNoticeVisible(bool isVisible)
    {
        if (emptyPartyNoticeText == null)
            return;

        emptyPartyNoticeText.text = emptyPartyMessage;
        emptyPartyNoticeText.gameObject.SetActive(isVisible);
    }
}

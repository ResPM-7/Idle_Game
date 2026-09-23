using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DragableUnit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("데이터 및 UI")]
    public UnitDataSO myData;
    public TextMeshProUGUI levelText;

    [HideInInspector] public Transform originalParent;
    private CanvasGroup canvasGroup;

    private bool isValidDrag = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        // 풀에서 다시 꺼낸 유닛이 이전 드래그 상태를 유지하지 않도록 초기화합니다.
        isValidDrag = false;
        originalParent = null;

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        UpdateLevelUI();
    }

    // 팩토리에서 소환될 때 데이터를 직접 꽂아주는 함수
    public void InitializeByData(UnitDataSO newData)
    {
        myData = newData;
        UpdateLevelUI();
    }

    public void LevelUp()
    {
        UnitDataSO nextData = myData.GetNextUpgradeUnit(); // 랜덤 진화 함수 호출

        if (nextData != null)
        {
            myData = nextData;
            UpdateLevelUI();
        }
        else
        {
            Debug.Log("더 이상 진화할 수 없는 최종 형태입니다!");
        }
    }

    public void UpdateLevelUI()
    {
        if (levelText != null && myData != null)
        {
            levelText.text = $"Lv.{myData.unitLevel}\n{myData.unitName}";
        }
    }

    private bool IsLocked()
    {
        bool isInBattleSlot = GetComponentInParent<BattleSlotUI>() != null;

        // 배틀 슬롯에 있고 && 전투가 진행 중(!stageGiveUp)이라면 조작 불가(true)
        if (isInBattleSlot && WaveManager.instance != null && !WaveManager.instance.stageGiveUp)
        {
            return true;
        }
        return false; // 그 외(그리드에 있거나, 전투 정지 중)에는 모두 조작 가능(false)
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        // 전투 중이면 새 드래그 시작 자체를 막음
        if (IsLocked()) return;

        SoundManager.instance?.PlaySfx("ui_pickup");

        isValidDrag = true;
        originalParent = transform.parent;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        transform.SetParent(transform.root);
    }


    public void OnDrag(PointerEventData eventData)
    {
        // 이미 시작된 드래그만 움직임
        if (!isValidDrag) return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그를 시작하지 못했다면 처리할 것 없음
        if (!isValidDrag) return;

        isValidDrag = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // 일시정지 해제 등으로 전투 상태가 바뀌었어도,
        // root에 남은 유닛은 반드시 원래 슬롯으로 복귀시킴
        if (gameObject.activeSelf &&
            transform.parent == transform.root &&
            originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }

        originalParent = null;
    }

    /// <summary>
    /// 쓰레기통 판매처럼 일반 슬롯 드롭이 아닌 곳에서 드래그를 완료할 때 호출합니다.
    /// 판매된 유닛이 OnEndDrag에서 원래 슬롯으로 돌아가는 것을 막습니다.
    /// </summary>
    public void CompleteExternalDrop()
    {
        isValidDrag = false;
        originalParent = null;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}

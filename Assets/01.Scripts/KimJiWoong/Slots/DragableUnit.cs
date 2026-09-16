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

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. 현재 배틀 슬롯(전장)에 배치된 유닛인지 확인합니다.
        bool isInBattleSlot = GetComponentInParent<BattleSlotUI>() != null;

        // 2. 나중에 WaveManager가 완성되면 아래 주석을 해제하세요!
        // 전투 중(isBattleActive == true)이면서 배틀 슬롯에 있다면 드래그를 막습니다.
        /*
        if (isInBattleSlot && WaveManager.instance.isBattleActive)
        {
            return; 
        }
        */

        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (gameObject.activeSelf && transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }
    }
}
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
        // SO 안에 다음 진화 데이터가 연결되어 있다면?
        if (myData != null && myData.nexUpdateUnit != null)
        {
            // 내 데이터를 다음 레벨 데이터로 통째로 덮어씌움!
            myData = myData.nexUpdateUnit;
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
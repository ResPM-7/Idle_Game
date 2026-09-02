using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DragableUnit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int unitLevel = 1;
    public TextMeshProUGUI levelText;
    public Transform originalParent;

    private CanvasGroup canvasGroup; // 잦은 GetComponent 호출을 방지하기 위해 캐싱

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Start 대신 OnEnable을 사용합니다.
    // 풀에서 꺼내져서 SetActive(true)가 될 때마다 실행됩니다.
    private void OnEnable()
    {
        // 1. 풀에서 나왔을 때 무조건 드래그 가능한 상태로 초기화!
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        // 2. 텍스트 표시
        UpdateLevelUI();
    }

    public void LevelUp()
    {
        unitLevel++;
        UpdateLevelUI();
    }

    public void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = "Lv." + unitLevel;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false; // 마우스 포인터 뒤의 슬롯을 인식하기 위해 잠시 끔
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 유닛이 활성화되어 있고(풀로 안 들어갔고), 여전히 허공(root)에 떠 있다면 원위치
        if (gameObject.activeSelf && transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }
    }
}
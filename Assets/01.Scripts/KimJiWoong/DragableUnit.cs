using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DragableUnit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int unitLevel = 1;
    public TextMeshProUGUI levelText;

    public Transform originalParent;

    private void Start()
    {
        UpdateLevelUI(); // 처음 생성될 때 레벨 텍스트 표시
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
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }
    }

}
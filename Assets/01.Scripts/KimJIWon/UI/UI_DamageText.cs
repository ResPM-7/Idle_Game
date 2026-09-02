using DG.Tweening; 
using TMPro;
using UnityEngine;

public class UI_DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float moveHeight = 60f; // 위로 뜰 높이
    [SerializeField] private float duration = 0.5f;  // 연출 시간

    private static Transform cachedCanvasTransform;

    private string myPoolName;

    public void Setup(float damage, Vector3 worldPos, string poolName)
    {
        myPoolName = poolName;
        damageText.text = Mathf.RoundToInt(damage).ToString();

        if (cachedCanvasTransform == null)
        {
            GameObject overlayCanvas = GameObject.Find("Canvas_Overlay");
            if (overlayCanvas != null)
            {
                cachedCanvasTransform = overlayCanvas.transform;
            }
            else
            {
                Canvas anyCanvas = FindAnyObjectByType<Canvas>();
                if (anyCanvas != null) cachedCanvasTransform = anyCanvas.transform;
            }
        }

        if (cachedCanvasTransform != null)
        {
            transform.SetParent(cachedCanvasTransform, false);
        }

        //유닛 위치를 Canvas 위치로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 1.5f);
        screenPos.z = 0f;
        transform.position = screenPos;
        transform.localScale = Vector3.one;

        transform.DOKill();
        damageText.DOKill();
        damageText.alpha = 1f;

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(screenPos.y + moveHeight, duration).SetEase(Ease.OutQuad));
        seq.Join(damageText.DOFade(0f, duration));

        //반납
        seq.OnComplete(() =>
        {
        ObjectPoolManager.instance.ReturnObject(myPoolName, gameObject);
        });
    }

    private void OnDisable()
    {
        transform.DOKill();
        damageText.DOKill();
    }
}
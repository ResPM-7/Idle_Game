using DG.Tweening; 
using TMPro;
using UnityEngine;

public class UI_DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float moveHeight = 60f; // 위로 뜰 높이
    [SerializeField] private float duration = 0.5f;  // 연출 시간

    private string myPoolName;

    public void Setup(float damage, Vector3 worldPos, string poolName)
    {
        myPoolName = poolName;
        damageText.text = Mathf.RoundToInt(damage).ToString();

        Vector3 targetWorldPos = worldPos + new Vector3(0f, 1f, 0f);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorldPos);
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
using System.Text;
using DG.Tweening; 
using TMPro;
using UnityEngine;

public class UI_DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector2 randomScreenOffset = new Vector2(30f, 15f);
    [SerializeField] private float moveHeight = 60f; // 위로 뜰 높이
    [SerializeField] private float duration = 0.5f;  // 연출 시간

    [Header("Blocked Style")]
    [SerializeField] private TMP_SpriteAsset blockedSpriteAsset;

    [Header("Critical Style")]
    [SerializeField] private float criticalScale = 1.25f;
    [SerializeField] private Color criticalTint = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private string criticalIconSpriteName;

    private string myPoolName;
    private TMP_SpriteAsset damageSpriteAsset;
    private readonly StringBuilder spriteTextBuilder = new();

    private void Awake()
    {
        damageSpriteAsset = damageText.spriteAsset;
    }

    public void Setup(float damage, Vector3 worldPos, string poolName, bool isCritical = false)
    {
        myPoolName = poolName;

        bool isBlocked = damage <= 0f;
        bool useCriticalStyle = isCritical && !isBlocked;

        damageText.tintAllSprites = true;
        damageText.color = useCriticalStyle ? criticalTint : Color.white;

        if (isBlocked)
        {
            damageText.spriteAsset = blockedSpriteAsset;
            damageText.text = "<sprite name=\"BLOCKED\">";
        }
        else
        {
            damageText.spriteAsset = damageSpriteAsset;
            damageText.text = BuildDamageSpriteText(damage, useCriticalStyle);
        }

        Vector3 targetWorldPos = worldPos + worldOffset;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorldPos);
        screenPos.x += Random.Range(-Mathf.Abs(randomScreenOffset.x), Mathf.Abs(randomScreenOffset.x));
        screenPos.y += Random.Range(-Mathf.Abs(randomScreenOffset.y), Mathf.Abs(randomScreenOffset.y));
        screenPos.z = 0f; 

        transform.position = screenPos;
        transform.localScale = Vector3.one * (useCriticalStyle ? criticalScale : 1f);

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

    private string BuildDamageSpriteText(float damage, bool isCritical)
    {
        int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
        string digits = roundedDamage.ToString();

        spriteTextBuilder.Clear();

        if (isCritical && HasCriticalIcon())
        {
            spriteTextBuilder.Append("<sprite name=\"");
            spriteTextBuilder.Append(criticalIconSpriteName);
            spriteTextBuilder.Append("\">");
        }

        foreach (char digit in digits)
        {
            spriteTextBuilder.Append("<sprite name=\"damage_");
            spriteTextBuilder.Append(digit);
            spriteTextBuilder.Append("\">");
        }

        return spriteTextBuilder.ToString();
    }

    private bool HasCriticalIcon()
    {
        return !string.IsNullOrWhiteSpace(criticalIconSpriteName)
            && damageText.spriteAsset != null
            && damageText.spriteAsset.GetSpriteIndexFromName(criticalIconSpriteName) >= 0;
    }

    private void OnDisable()
    {
        transform.DOKill();
        damageText.DOKill();
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_DelayedFillBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image trailImage;
    [SerializeField, Min(0f)] private float trailDelay = 0.15f;
    [SerializeField, Min(0f)] private float trailDuration = 0.35f;

    private Tween trailTween;

    public void SetValue(float normalizedValue)
    {
        if (fillImage == null)
            return;

        float targetFill = Mathf.Clamp01(normalizedValue);
        float previousFill = fillImage.fillAmount;

        fillImage.fillAmount = targetFill;

        if (trailImage == null)
            return;

        KillTrailTween();

        if (targetFill >= previousFill)
        {
            trailImage.fillAmount = targetFill;
            return;
        }

        trailImage.fillAmount = Mathf.Max(trailImage.fillAmount, previousFill);

        if (trailDuration <= 0f)
        {
            trailImage.fillAmount = targetFill;
            return;
        }

        trailTween = DOTween.Sequence()
            .AppendInterval(trailDelay)
            .Append(trailImage
                .DOFillAmount(targetFill, trailDuration)
                .SetEase(Ease.OutQuad));
    }

    public void SetValueImmediate(float normalizedValue)
    {
        float targetFill = Mathf.Clamp01(normalizedValue);

        KillTrailTween();

        if (fillImage != null)
            fillImage.fillAmount = targetFill;

        if (trailImage != null)
            trailImage.fillAmount = targetFill;
    }

    public void SetFillSprite(Sprite sprite)
    {
        if (fillImage == null)
            return;

        fillImage.sprite = sprite;
        fillImage.color = Color.white;
    }

    private void OnDisable()
    {
        KillTrailTween();
    }

    private void KillTrailTween()
    {
        if (trailTween == null)
            return;

        trailTween.Kill();
        trailTween = null;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;

    public void SetFill(float normalizedHp)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = Mathf.Clamp01(normalizedHp);
        }
    }

    public void SetScreenPosition(Vector3 screenPosition)
    {
        transform.position = screenPosition;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
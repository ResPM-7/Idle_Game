using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Sprite playerFillSprite;
    [SerializeField] private Sprite enemyFillSprite;

    public void SetFill(float normalizedHp)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = Mathf.Clamp01(normalizedHp);
        }
    }

    public void SetStyle(bool isEnemy)
    {
        if (hpFillImage == null)
            return;

        hpFillImage.sprite = isEnemy
            ? enemyFillSprite
            : playerFillSprite;
        hpFillImage.color = Color.white;
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

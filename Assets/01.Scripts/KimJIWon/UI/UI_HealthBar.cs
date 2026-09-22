using UnityEngine;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private UI_DelayedFillBar delayedFillBar;

    [Header("Faction Sprites")]
    [SerializeField] private Sprite playerFillSprite;
    [SerializeField] private Sprite enemyFillSprite;

    public void SetFill(float normalizedHp)
    {
        if (delayedFillBar != null)
            delayedFillBar.SetValue(normalizedHp);
    }

    public void SetFillImmediate(float normalizedHp)
    {
        if (delayedFillBar != null)
            delayedFillBar.SetValueImmediate(normalizedHp);
    }

    public void SetStyle(bool isEnemy)
    {
        if (delayedFillBar != null)
            delayedFillBar.SetFillSprite(isEnemy ? enemyFillSprite : playerFillSprite);
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

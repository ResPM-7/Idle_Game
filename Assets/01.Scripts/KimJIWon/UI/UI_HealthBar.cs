using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;

    [Header("Test Data")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    [Header("Test Target")]
    [SerializeField] private Transform testTarget;

    private void Update()
    {
        UpdateHealthBar();

#if UNITY_EDITOR
        
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.qKey.wasPressedThisFrame)
        {
            TakeDamage(10f);
        }
        if (keyboard.wKey.wasPressedThisFrame)
        {
            Heal(10f);
        }
#endif
    }

    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);

#if UNITY_EDITOR
        Vector3 spawnPos = (transform.parent != null) ? transform.parent.position : transform.position;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDamageText(damage, spawnPos);
        }
#endif
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    private void UpdateHealthBar()
    {
        if (maxHp > 0f)
        {
            SetFill(currentHp / maxHp);
        }
    }
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
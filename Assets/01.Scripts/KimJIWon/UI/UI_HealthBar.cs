using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;

    [Header("Test Data")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    private void Update()
    {
        UpdateHealthBar();

#if UNITY_EDITOR
        // 2. 테스트용(A: 데미지, S: 회복)
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.wasPressedThisFrame)
        {
            TakeDamage(10f);
        }
        if (keyboard.sKey.wasPressedThisFrame)
        {
            Heal(10f);
        }
#endif
    }

    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);

    #if UNITY_EDITOR
            string poolKey = "DamageText";
            
            GameObject textObj = ObjectPoolManager.instance.GetObject(poolKey);
    
        if (textObj != null)
            {
                textObj.GetComponent<UI_DamageText>().Setup(damage, transform.parent.position, poolKey);
            }
    #endif
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    private void UpdateHealthBar()
    {
        if (hpFillImage != null && maxHp > 0)
        {
            hpFillImage.fillAmount = currentHp / maxHp;
        }
    }
}
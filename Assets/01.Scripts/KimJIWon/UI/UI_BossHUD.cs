using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossHUD : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text timerText;

    [Header("Bars")]
    [SerializeField] private UI_DelayedFillBar healthBar;
    [SerializeField] private Image timerFillImage;

    public void Show(
        string bossName,
        float currentHp,
        float maxHp,
        float remainingTime,
        float totalTime)
    {
        gameObject.SetActive(true);

        SetBossName(bossName);
        SetHealthInternal(currentHp, maxHp, true);
        SetTimer(remainingTime, totalTime);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetBossName(string bossName)
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
    }

    public void SetHealth(float currentHp, float maxHp)
    {
        SetHealthInternal(currentHp, maxHp, false);
    }

    private void SetHealthInternal(float currentHp, float maxHp, bool immediate)
    {
        float healthRatio = maxHp > 0f
            ? Mathf.Clamp01(currentHp / maxHp)
            : 0f;

        if (healthBar != null)
        {
            if (immediate)
                healthBar.SetValueImmediate(healthRatio);
            else
                healthBar.SetValue(healthRatio);
        }

        if (healthText != null)
        {
            healthText.text =
                $"{healthRatio * 100f:F0}%  {currentHp:N0} / {maxHp:N0}";
        }
    }

    public void SetTimer(float remainingTime, float totalTime)
    {
        float safeRemainingTime = Mathf.Max(0f, remainingTime);

        float timerRatio = totalTime > 0f
            ? Mathf.Clamp01(safeRemainingTime / totalTime)
            : 0f;

        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = timerRatio;
        }

        if (timerText != null)
        {
            int totalSeconds = Mathf.CeilToInt(safeRemainingTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}

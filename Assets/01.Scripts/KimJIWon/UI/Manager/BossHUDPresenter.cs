using UnityEngine;

public class BossHUDPresenter : MonoBehaviour
{
    [SerializeField] private UI_BossHUD bossHUD;

    private void Awake()
    {
        //게임 시작 시 비활성화
        if (bossHUD != null)
        {
            bossHUD.Hide();
        }
    }

    public void BeginBossBattle(
        string bossName,
        float currentHp,
        float maxHp,
        float remainingTime,
        float totalTime)
    {
        if (bossHUD == null)
            return;

        bossHUD.Show(
            bossName,
            currentHp,
            maxHp,
            remainingTime,
            totalTime
        );
    }

    public void UpdateBossHealth(float currentHp, float maxHp)
    {
        if (bossHUD == null)
            return;

        bossHUD.SetHealth(currentHp, maxHp);
    }

    public void UpdateBossTimer(float remainingTime, float totalTime)
    {
        if (bossHUD == null)
            return;

        bossHUD.SetTimer(remainingTime, totalTime);
    }

    public void EndBossBattle()
    {
        if (bossHUD == null)
            return;

        bossHUD.Hide();
    }
}
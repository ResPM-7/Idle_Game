using UnityEngine;

public class Boss : MonoBehaviour
{
    private Unit_Base_Test unitBase;

    private void Awake()
    {
        unitBase = GetComponent<Unit_Base_Test>();
    }

    private void OnEnable()
    {
        if (unitBase != null)
        {
            unitBase.OnDeathEvent += HandleDeath;
            unitBase.OnHpChanged += HandleHpChanged;
        }
    }

    private void OnDisable()
    {
        if (unitBase != null)
        {
            unitBase.OnDeathEvent -= HandleDeath;
            unitBase.OnHpChanged -= HandleHpChanged;
        }
    }

    private void HandleHpChanged(Unit_Base_Test unit, float currentHp, float maxHp, float damage, bool isCrit)
    {
        if (BossHUDPresenter.instance != null)
        {
            BossHUDPresenter.instance.UpdateBossHealth(currentHp, maxHp);
        }
    }

    private void HandleDeath()
    {
        // 진짜로 보스의 HP가 0이 되어 죽었을 때만 웨이브가 넘어갑니다!
        WaveManager.instance.BossKilled();
    }
}
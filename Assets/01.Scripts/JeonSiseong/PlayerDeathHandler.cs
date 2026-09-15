using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
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
        }
    }

    private void OnDisable()
    {
        if (unitBase != null)
        {
            unitBase.OnDeathEvent -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Debug.Log("플레이어 사망");

        WaveManager.instance.PlayerDied();
    }
}

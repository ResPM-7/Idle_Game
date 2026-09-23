using UnityEngine;

public class AttackBuffSkill : MonoBehaviour, IPoolable
{
    [Header("Buff Setting")]
    [SerializeField] private float buffMultiplier = 1.5f;  //  50% 증가
    [SerializeField] private float buffDuration = 5f;     // 버프 지속 시간

    public void OnSpawned()
    {
        CancelInvoke();
        if(PartyBuildManager.instance != null)
        {
            PartyBuildManager.instance.ApplyUnitDamageBuff(buffMultiplier, buffDuration);
        }

        Invoke(nameof(ReturnToPool), 0.1f);

    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(gameObject);
    }

    public void OnDespawned() { CancelInvoke(); }
}

using UnityEngine;

public class FireSkill : Skill, IPoolable
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageTarget(collision);
    }

    public void OnSpawned()
    {
        CancelInvoke();
        Invoke(nameof(ReturnToPool), 1f);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(gameObject);
    }
    public void OnDespawned() { CancelInvoke(); }
}

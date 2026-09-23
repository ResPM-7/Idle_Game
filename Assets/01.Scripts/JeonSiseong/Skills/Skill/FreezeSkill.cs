using UnityEngine;

public class FreezeSkill : Skill, IPoolable
{
    [Header("Freeze Setting")]
    [SerializeField] private float freezeDuration = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & enemyLayer.value) == 0)
            return;

        Unit_Base_Test unit = collision.GetComponent<Unit_Base_Test>();
        if(unit != null )
        {
            unit.ApplyFreeze(freezeDuration);
        }
    }
    public void OnSpawned()
    {
        CancelInvoke();
        Invoke(nameof(ReturnToPool), 0.2f);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(gameObject);
    }
    public void OnDespawned() { CancelInvoke(); }
}

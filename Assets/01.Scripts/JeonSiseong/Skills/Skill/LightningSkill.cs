using UnityEngine;

public class LightningSkill : Skill
{
    [SerializeField] private string poolName = "Lightning";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageTarget(collision);
    }
    void Start()
    {
        Invoke(nameof(ReturnToPool), 0.3f);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
    }
}

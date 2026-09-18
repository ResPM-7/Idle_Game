using UnityEngine;

public class FireSkill : Skill
{
    [SerializeField] private string poolName = "Fire";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageTarget(collision);
    }

    void Start()
    {
        Invoke(nameof(ReturnToPool), 1f);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
    }
}

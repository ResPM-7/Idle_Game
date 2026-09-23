using UnityEngine;

public class FreezeSkill : Skill
{
    [Header("Freeze Setting")]
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private string poolName = "Freeze";

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
    void Start()
    {
        Invoke(nameof(ReturnToPool), 0.2f);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
    }
}

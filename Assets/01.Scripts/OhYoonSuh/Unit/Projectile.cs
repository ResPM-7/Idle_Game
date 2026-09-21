using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 18.75f;

    private Transform target;
    private float amount;
    private bool isHeal;
    private string myPoolName;
    private bool isCrit;

    public void Setup(Transform target, float amount, bool isHeal, string poolName, bool isCrit = false)
    {
        this.target = target;
        this.amount = amount;
        this.isHeal = isHeal;
        this.myPoolName = poolName;
        this.isCrit = isCrit;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            ReturnToPool();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        Unit_Base_Test targetUnit = target.GetComponent<Unit_Base_Test>();

        if (targetUnit != null && targetUnit.CurrentHp > 0)
        {
            if (isHeal)
            {
                targetUnit.TakeHeal(amount);
            }
            else
            {
                targetUnit.TakeDamage(amount, isCrit);
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (!string.IsNullOrEmpty(myPoolName) && ObjectPoolManager.instance != null)
        {
            ObjectPoolManager.instance.ReturnObject(myPoolName, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
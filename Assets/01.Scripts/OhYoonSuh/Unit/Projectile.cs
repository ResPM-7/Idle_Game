using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 18.75f;

    private Transform target;
    private float amount;
    private bool isHeal;
    private string myPoolName;
    private bool isCrit;
    private Unit_Base_Test targetUnit;
    private uint targetVersion;
    private bool isReady;

    protected Transform Target => target;

    public void Setup(Transform target, float amount, bool isHeal, string poolName, bool isCrit = false)
    {
        this.target = target;
        this.amount = amount;
        this.isHeal = isHeal;
        this.myPoolName = poolName;
        this.isCrit = isCrit;
        targetUnit = target != null ? target.GetComponent<Unit_Base_Test>() : null;
        targetVersion = targetUnit != null ? targetUnit.SpawnVersion : 0;
        isReady = true;

        OnSetup();
    }

    protected virtual void Update()
    {
        // 기존 풀은 먼저 활성화하므로 Setup이 끝난 뒤에만 비행합니다.
        if (!isReady) return;
        if (target == null || !target.gameObject.activeInHierarchy
            || (targetUnit != null && (!targetUnit.IsCombatReady || targetUnit.CurrentHp <= 0f || targetUnit.SpawnVersion != targetVersion)))
        {
            ReturnToPool();
            return;
        }

        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.000001f)
        {
            UpdateFlight(direction);
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if ((transform.position - target.position).sqrMagnitude < 0.04f)
        {
            HitTarget();
        }
    }

    protected virtual void OnSetup() { }

    protected virtual void UpdateFlight(Vector3 direction) { }

    private void HitTarget()
    {
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
        target = null;

        if (!string.IsNullOrEmpty(myPoolName) && ObjectPoolManager.instance != null)
        {
            ObjectPoolManager.instance.ReturnObject(myPoolName, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDisable()
    {
        // 기존 ReturnObject의 비활성화 과정에서 다음 발사를 위해 상태를 비웁니다.
        isReady = false;
        target = null;
        targetUnit = null;
        targetVersion = 0;
        amount = 0f;
        isHeal = isCrit = false;
        myPoolName = null;
    }
}

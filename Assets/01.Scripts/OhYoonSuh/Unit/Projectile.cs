using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 18.75f;

    private Transform target;
    private float amount;
    private bool isHeal;
    private string myPoolName;
    private bool isCrit;
    private Unit_Base_Test targetUnit;
    // 같은 풀 객체가 새 유닛으로 재사용되면 이전 투사체가 새 유닛을 맞히지 않도록 구분합니다.
    private uint targetSpawnVersion;
    private float lifetime;
    [Tooltip("투사체의 최대 생존 시간(초)입니다. 시간이 지나면 풀로 반환합니다.")]
    [SerializeField, Min(0.1f)] private float maxLifetime = 10f;

    protected Transform Target => target;

    public void Setup(Transform target, float amount, bool isHeal, string poolName, bool isCrit = false)
    {
        this.target = target;
        this.amount = amount;
        this.isHeal = isHeal;
        this.myPoolName = poolName;
        this.isCrit = isCrit;
        targetUnit = target != null ? target.GetComponent<Unit_Base_Test>() : null;
        targetSpawnVersion = targetUnit != null ? targetUnit.SpawnVersion : 0;
        lifetime = 0f;

        OnSetup();
    }

    protected virtual void Update()
    {
        lifetime += Time.deltaTime;
        if (target == null || !target.gameObject.activeInHierarchy || lifetime >= maxLifetime
            || (targetUnit != null && (targetUnit.CurrentHp <= 0f || targetUnit.SpawnVersion != targetSpawnVersion)))
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
            ObjectPoolManager.instance.ReturnObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnSpawned() { lifetime = 0f; }

    // 다음 발사에서 이전 타깃·피해량·회전이 남지 않도록 정리합니다.
    public void OnDespawned()
    {
        target = null;
        targetUnit = null;
        targetSpawnVersion = 0;
        amount = 0f;
        isHeal = false;
        isCrit = false;
        myPoolName = null;
        lifetime = 0f;
        transform.localRotation = Quaternion.identity;
    }
}

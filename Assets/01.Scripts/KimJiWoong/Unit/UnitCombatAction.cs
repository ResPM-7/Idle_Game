using UnityEngine;

/// <summary>근접·원거리·회복의 공통 부모입니다. 공유 행동에는 유닛별 상태를 저장하지 않습니다.</summary>
public abstract class UnitCombatAction
{
    public abstract float GetRange(Unit_Base_Test owner);
    public abstract int GetTargetLayer(Unit_Base_Test owner);
    public virtual bool CanTarget(Unit_Base_Test owner, Unit_Base_Test target)
        => target != null && target.isActiveAndEnabled && target.IsCombatReady && target.CurrentHp > 0f
            && ((1 << target.gameObject.layer) & GetTargetLayer(owner)) != 0;
    public abstract void Execute(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool critical);

    protected void Fire(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool heal, bool critical, string poolName)
    {
        if (string.IsNullOrEmpty(poolName)) return;
        ObjectPoolManager pool = ObjectPoolManager.instance;
        if (pool == null) return;
        // 기존 풀 사용 순서 유지: 활성화된 객체를 꺼낸 다음 같은 호출 안에서 위치와 데이터를 설정합니다.
        GameObject obj = pool.GetObject(poolName);
        if (obj == null) return;
        obj.transform.position = owner.transform.position;
        if (obj.TryGetComponent<Projectile>(out var projectile))
            projectile.Setup(target.transform, amount, heal, poolName, critical);
        else
        {
            Debug.LogError("[유닛 공격] 투사체 프리팹에 Projectile 컴포넌트가 없습니다.", obj);
            pool.ReturnObject(poolName, obj);
        }
    }
}

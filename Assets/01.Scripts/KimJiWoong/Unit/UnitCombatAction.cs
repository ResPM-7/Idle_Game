using System;
using UnityEngine;

/// <summary>근접·원거리·힐 행동의 공통 부모입니다. 행동 객체는 상태 없이 공유합니다.</summary>
public abstract class UnitCombatAction
{
    public abstract float GetRange(Unit_Base_Test owner);
    public abstract int GetTargetLayer(Unit_Base_Test owner);
    public virtual bool CanTarget(Unit_Base_Test owner, Unit_Base_Test target)
    {
        return target != null && target.CanBeTargeted && target.CurrentHp > 0f
            && ((1 << target.gameObject.layer) & GetTargetLayer(owner)) != 0;
    }
    public abstract void Execute(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool critical);

    protected void Fire(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool heal, bool critical, string poolName)
    {
        if (string.IsNullOrEmpty(poolName)) return;
        ObjectPoolManager pool = ObjectPoolManager.instance;
        if (pool == null) return;
        // 값 형식 문맥과 한 번 만든 대리자를 전달하여 매 발사 시 캡처 람다 생성을 피합니다.
        var request = new ProjectileRequest
        {
            Position = owner.transform.position, Target = target.transform,
            Amount = amount, Heal = heal, Critical = critical, PoolName = poolName
        };
        pool.Spawn(poolName, request, PrepareProjectile);
    }

    private struct ProjectileRequest
    {
        public Vector3 Position;
        public Transform Target;
        public float Amount;
        public bool Heal, Critical;
        public string PoolName;
    }
    private static readonly Action<GameObject, ProjectileRequest> PrepareProjectile = Prepare;
    private static void Prepare(GameObject obj, ProjectileRequest request)
    {
        obj.transform.position = request.Position;
        if (!obj.TryGetComponent<Projectile>(out var projectile))
            throw new InvalidOperationException("투사체 프리팹에 Projectile 컴포넌트가 없습니다.");
        projectile.Setup(request.Target, request.Amount, request.Heal, request.PoolName, request.Critical);
    }
}

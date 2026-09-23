/// <summary>공통 행동을 상속한 원거리 공격입니다. 투사체는 풀에서 가져옵니다.</summary>
public sealed class RangedCombatAction : UnitCombatAction
{
    public override float GetRange(Unit_Base_Test owner) => owner.MyData.attackRange;
    public override int GetTargetLayer(Unit_Base_Test owner) => owner.TargetLayer.value;
    public override void Execute(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool critical)
        => Fire(owner, target, amount, false, critical, owner.MyData.projectilePoolName);
}

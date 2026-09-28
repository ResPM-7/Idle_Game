/// <summary>공통 행동을 상속한 근접 공격입니다.</summary>
public sealed class MeleeCombatAction : UnitCombatAction
{
    public override float GetRange(Unit_Base_Test owner) => owner.MyData.attackRange;
    public override int GetTargetLayer(Unit_Base_Test owner) => owner.TargetLayer.value;
    public override void Execute(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool critical)
        => target.TakeDamage(amount, critical);
}

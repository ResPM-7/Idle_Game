/// <summary>공통 행동을 상속한 회복입니다. 체력이 부족한 아군만 대상으로 삼습니다.</summary>
public sealed class HealCombatAction : UnitCombatAction
{
    public override float GetRange(Unit_Base_Test owner) => owner.MyData.healRange;
    public override int GetTargetLayer(Unit_Base_Test owner) => owner.AllyLayer.value;
    public override bool CanTarget(Unit_Base_Test owner, Unit_Base_Test target)
        => base.CanTarget(owner, target) && target.CurrentHp < target.CurrentMaxHp;
    public override void Execute(Unit_Base_Test owner, Unit_Base_Test target, float amount, bool critical)
        => Fire(owner, target, amount, true, critical, owner.MyData.healProjectilePoolName);
}

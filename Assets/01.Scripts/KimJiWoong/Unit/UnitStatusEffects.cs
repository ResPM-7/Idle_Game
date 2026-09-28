using UnityEngine;

/// <summary>일시 효과의 남은 시간을 숫자로 관리합니다. 코루틴과 대기 객체를 만들지 않습니다.</summary>
public sealed class UnitStatusEffects
{
    private float damageTime, freezeTime;
    private UnitVisualEffect visual;
    public float DamageMultiplier { get; private set; } = 1f;
    public bool IsFrozen => freezeTime > 0f;
    public void BindVisual(UnitVisualEffect value) => visual = value;
    public void ApplyDamageBuff(float multiplier, float duration)
    {
        damageTime = Mathf.Max(0f, duration);
        DamageMultiplier = damageTime > 0f ? Mathf.Max(0f, multiplier) : 1f;
        visual?.SetDamageBuff(damageTime > 0f);
    }
    public void ApplyFreeze(float duration)
    {
        freezeTime = Mathf.Max(0f, duration);
        visual?.SetFrozen(IsFrozen);
    }
    public void Tick(float deltaTime)
    {
        if (damageTime > 0f && (damageTime -= deltaTime) <= 0f)
        {
            DamageMultiplier = 1f;
            visual?.SetDamageBuff(false);
        }
        if (freezeTime > 0f && (freezeTime -= deltaTime) <= 0f) visual?.SetFrozen(false);
    }
    public void Reset()
    {
        if (damageTime > 0f || freezeTime > 0f) visual?.ResetEffects();
        damageTime = freezeTime = 0f;
        DamageMultiplier = 1f;
    }
}

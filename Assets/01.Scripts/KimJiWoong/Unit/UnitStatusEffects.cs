using UnityEngine;

/// <summary>버프·빙결 시간을 값으로 관리하여 코루틴과 대기 객체의 반복 생성을 없앱니다.</summary>
public sealed class UnitStatusEffects
{
    private float damageTime;
    private float freezeTime;
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
        if (damageTime > 0f)
        {
            damageTime -= deltaTime;
            if (damageTime <= 0f)
            {
                DamageMultiplier = 1f;
                visual?.SetDamageBuff(false);
            }
        }
        if (freezeTime > 0f)
        {
            freezeTime -= deltaTime;
            if (freezeTime <= 0f) visual?.SetFrozen(false);
        }
    }
    public void Reset()
    {
        if (damageTime > 0f || freezeTime > 0f) visual?.ResetEffects();
        damageTime = 0f;
        freezeTime = 0f;
        DamageMultiplier = 1f;
    }
}

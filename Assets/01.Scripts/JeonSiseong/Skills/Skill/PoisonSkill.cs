using System.Collections.Generic;
using UnityEngine;

public class PoisonSkill : Skill
{
    [Header("독 스킬 설정")]
    [SerializeField, Min(0.01f)] private float duration = 5f;
    [SerializeField, Min(0.01f)] private float damageInterval = 1f;
    [Tooltip("프리팹 이름이 아니라 오브젝트 풀에 등록한 이름입니다.")]
    [SerializeField] private string poolName = "Poison";

    private float elapsedTime;
    private float tickTimer;
    private bool returning;
    private readonly List<Collider2D> targetsInRange = new List<Collider2D>(32);
    private readonly List<Collider2D> tickTargets = new List<Collider2D>(32);

    private void OnEnable()
    {
        // Start는 최초 한 번만 호출되므로 풀 재사용 초기화는 여기서 처리합니다.
        elapsedTime = 0f;
        tickTimer = 0f;
        returning = false;
        targetsInRange.Clear();
        tickTargets.Clear();
    }

    private void OnDisable()
    {
        returning = true;
        targetsInRange.Clear();
        tickTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!returning && ((1 << collision.gameObject.layer) & enemyLayer.value) != 0
            && !targetsInRange.Contains(collision))
            targetsInRange.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        targetsInRange.Remove(collision);
    }

    private void Update()
    {
        if (returning) return;

        // 지속시간을 넘긴 프레임의 초과 시간에는 피해를 주지 않습니다.
        float step = Mathf.Min(Time.deltaTime, Mathf.Max(0f, duration - elapsedTime));
        elapsedTime += step;
        tickTimer += step;
        float interval = Mathf.Max(0.01f, damageInterval);

        while (tickTimer >= interval)
        {
            tickTimer -= interval;
            ApplyTick();
            if (returning) return;
        }

        if (elapsedTime >= duration) ReturnToPool();
    }

    private void ApplyTick()
    {
        // 적 사망으로 원본 목록이 바뀌어도 안전하게 순회합니다.
        // 목록을 재사용하므로 매 틱마다 새 배열을 만들지 않습니다.
        tickTargets.Clear();
        for (int i = targetsInRange.Count - 1; i >= 0; i--)
        {
            Collider2D target = targetsInRange[i];
            if (target == null || !target.isActiveAndEnabled)
                targetsInRange.RemoveAt(i);
            else
                tickTargets.Add(target);
        }

        for (int i = 0; i < tickTargets.Count && !returning; i++)
        {
            Collider2D target = tickTargets[i];
            if (target != null && target.isActiveAndEnabled) DamageTarget(target);
        }
        tickTargets.Clear();
    }

    private void ReturnToPool()
    {
        if (returning) return;
        returning = true;
        if (ObjectPoolManager.instance != null)
            ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
        else
        {
            Debug.LogWarning("독 스킬을 반환할 오브젝트 풀 매니저가 없어 비활성화합니다.", this);
            gameObject.SetActive(false);
        }
    }
}

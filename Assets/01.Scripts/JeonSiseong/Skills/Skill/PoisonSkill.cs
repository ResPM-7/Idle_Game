using System.Collections;
using UnityEngine;

public class PoisonSkill : Skill
{
    [Header("Poison Setting")]
    [SerializeField] private string poolName = "Poison";
    [SerializeField] private float duration = 5f;
    [SerializeField] private float damageInterval = 1f;

    private void OnEnable()
    {
        StartCoroutine(PoisonRoutine());
    }

    private IEnumerator PoisonRoutine()
    {
        // GetObject()가 활성화한 뒤 SkillManager가 위치를 세팅할 시간을 한 프레임 줌
        yield return null;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll(
                transform.position,
                radius,
                enemyLayer
            );

            foreach (Collider2D target in targets)
            {
                DamageTarget(target);
            }

            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }

        if (ObjectPoolManager.instance != null)
        {
            ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
        }
    }
}
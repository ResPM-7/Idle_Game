using UnityEngine;

public class FireSkill : Skill
{
    [SerializeField] private string poolName = "Fire";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageTarget(collision);
    }

    private void OnEnable()
    {
        // 풀에서 꺼낼 때마다 반환을 예약합니다.
        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), 0.3f);
    }

    private void OnDisable()
    {
        // 중간에 반환되면 이전 예약을 취소합니다.
        CancelInvoke(nameof(ReturnToPool));
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
    }
}

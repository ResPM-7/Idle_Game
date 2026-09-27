using UnityEngine;

public class FreezeSkill : Skill
{
    [Header("Freeze Setting")]
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private string poolName = "Freeze";


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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer.value) == 0)
            return;

        Unit_Base_Test unit = collision.GetComponent<Unit_Base_Test>();
        if (unit != null)
        {
            unit.ApplyFreeze(freezeDuration);
        }
    }
}

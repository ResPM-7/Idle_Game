using UnityEngine;

public class AttackBuffSkill : MonoBehaviour
{
    [Header("Buff Setting")]
    [SerializeField] private float buffMultiplier = 1.5f; // 공격력 50% 증가
    [SerializeField] private float buffDuration = 5f;     // 버프 지속 시간
    [SerializeField] private string poolName = "AttackBuff";

    // 풀 생성 과정의 최초 OnEnable과 실제 사용을 구분합니다.
    private bool hasStarted;

    private void Start()
    {
        hasStarted = true;
        ActivateSkill();
    }

    private void OnEnable()
    {
        // ObjectPoolManager가 처음 오브젝트를 생성할 때도 OnEnable이 호출됩니다.
        // 그때는 버프를 적용하지 않고, 풀에서 재사용될 때만 실행합니다.
        if (!hasStarted)
            return;

        ActivateSkill();
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnToPool));
    }

    private void ActivateSkill()
    {
        CancelInvoke(nameof(ReturnToPool));

        if (PartyBuildManager.instance != null)
        {
            PartyBuildManager.instance.ApplyUnitDamageBuff(
                buffMultiplier,
                buffDuration
            );
        }

        Invoke(nameof(ReturnToPool), 0.1f);
    }

    private void ReturnToPool()
    {
        if (ObjectPoolManager.instance != null)
        {
            ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
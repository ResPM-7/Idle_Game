using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Unit_Base_Test unitBase;

    private void Awake()
    {
        unitBase = GetComponent<Unit_Base_Test>();
    }

    private void OnEnable()
    {
        if (unitBase != null && unitBase.myData != null)
        {
            unitBase.Init(unitBase.myData);
        }

        // 유닛이 활성화될 때 사망 이벤트를 귀 기울여 듣기 시작합니다 (구독)
        if (unitBase != null)
        {
            unitBase.OnDeathEvent += HandleDeath;
        }
    }

    private void OnDisable()
    {
        // 비활성화(풀 반환)될 때는 반드시 구독을 취소해야 메모리 누수가 없습니다!
        if (unitBase != null)
        {
            unitBase.OnDeathEvent -= HandleDeath;
        }
    }

    // 체력이 0이 되어 OnDeathEvent가 터지면 이 함수가 실행됩니다.
    private void HandleDeath()
    {
        // 이때 확실하게 웨이브 매니저에게 적이 죽었다고 알립니다.
        WaveManager.instance.EnemyKilled();

        // (참고: 오브젝트를 풀로 되돌리는 로직은 UnitDestroyedState 내부에서 처리하는 것이 깔끔합니다)
    }
}

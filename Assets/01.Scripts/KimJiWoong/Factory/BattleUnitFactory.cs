using UnityEngine;

public class BattleUnitFactory : Singleton<BattleUnitFactory>
{
    private struct SpawnRequest
    {
        public UnitDataSO Data;
        public Vector3 Position;
    }
    // 생성마다 캡처 람다를 만들지 않고 대리자와 값 형식 문맥을 재사용합니다.
    private static readonly System.Action<GameObject, SpawnRequest> PrepareUnit = Prepare;
    private static void Prepare(GameObject obj, SpawnRequest request)
    {
        obj.transform.SetPositionAndRotation(request.Position, Quaternion.identity);
        if (!obj.TryGetComponent<Unit_Base_Test>(out var unit))
            throw new System.InvalidOperationException("전투 프리팹에 Unit_Base_Test 컴포넌트가 없습니다.");
        unit.Init(request.Data);
    }
    /// <summary>
    /// 풀에서 전투 유닛을 가져와 지정된 스폰 위치에 배치하고 초기화합니다.
    /// </summary>
    public GameObject CreateBattleUnit(UnitDataSO data, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("전투 유닛 생성 위치가 비어있습니다.", this);
            return null;
        }

        return CreateBattleUnit(data, spawnPoint.position);
    }

    public GameObject CreateBattleUnit(UnitDataSO data, Vector3 spawnPosition)
    {
        if (data == null || string.IsNullOrEmpty(data.battlePoolName))
        {
            Debug.LogWarning("전투 유닛 데이터 또는 battlePoolName이 비어있습니다!", this);
            return null;
        }

        GameObject battleUnit = ObjectPoolManager.instance.Spawn(data.battlePoolName,
            new SpawnRequest { Data = data, Position = spawnPosition }, PrepareUnit);

        if (battleUnit == null)
        {
            Debug.LogWarning($"전투 유닛 풀에서 가져오기 실패: {data.battlePoolName}", this);
            return null;
        }

        return battleUnit;
    }
}

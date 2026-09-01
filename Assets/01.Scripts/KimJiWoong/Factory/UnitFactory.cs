using UnityEngine;

public class UnitFactory : Singleton<UnitFactory>
{
    /// <summary>
    /// 풀에서 유닛을 가져와 부모 슬롯에 배치하고 초기화합니다.
    /// </summary>
    public GameObject CreateUnit(string poolName, int level, Transform parentSlot)
    {
        // 1. ObjectPoolManager에서 유닛 가져오기
        GameObject unit = ObjectPoolManager.instance.GetObject(poolName);

        if (unit == null)
        {
            Debug.LogWarning($"{poolName} 풀에서 유닛을 찾을 수 없습니다! 인스펙터 설정을 확인하세요.");
            return null;
        }

        // 2. 부모 슬롯 설정 및 Transform 초기화
        unit.transform.SetParent(parentSlot, false); // false로 설정하면 localPosition, localScale 자동 유지
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = Vector3.one;

        // 3. 유닛 데이터 초기화 (레벨 세팅 등)
        DragableUnit dragUnit = unit.GetComponent<DragableUnit>();
        if (dragUnit != null)
        {
            dragUnit.unitLevel = level;
            dragUnit.UpdateLevelUI(); // 팩토리에서 찍어낼 때 UI도 같이 갱신해줌
        }

        return unit;
    }
}
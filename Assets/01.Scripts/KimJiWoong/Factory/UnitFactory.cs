using UnityEngine;

public class UnitFactory : Singleton<UnitFactory>
{
    /// <summary>
    /// 풀에서 유닛을 가져와 부모 슬롯에 배치하고 초기화합니다.
    /// </summary>
    public GameObject CreateUnit(string poolName, UnitDataSO data, Transform parentSlot)
    {
        GameObject unit = ObjectPoolManager.instance.GetObject(poolName);

        if (unit == null) return null;

        unit.transform.SetParent(parentSlot, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = Vector3.one;

        DragableUnit dragUnit = unit.GetComponent<DragableUnit>();
        if (dragUnit != null)
        {
            //위에서 만든 함수로 SO 데이터를 주입
            dragUnit.InitializeByData(data);
        }

        return unit;
    }
}
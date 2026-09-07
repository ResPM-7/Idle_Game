using System.Collections.Generic;
using UnityEngine;

public class UI_UpgradeTab : MonoBehaviour
{
    [Header("Layout Reference")]
    [SerializeField] private Transform contentTransform; // ScrollView/Viewport/Content
    [SerializeField] private StatUpgradeItem itemPrefab;   // StatUpgradeItem 프리팹

    private List<StatUpgradeItem> spawnedItems = new List<StatUpgradeItem>();

    //외부에서 스탯 데이터 리스트, 클릭 콜백 받아 초기화
    public void InitTab(List<IStatData> statList, System.Action<StatType> onUpgradeRequest)
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        spawnedItems.Clear();

        if (statList == null) return;

        //프리팹생성
        foreach (var statData in statList)
        {
            StatUpgradeItem item = Instantiate(itemPrefab, contentTransform);
            item.Setup(statData, onUpgradeRequest);
            spawnedItems.Add(item);
        }
    }

    // 특정한 스탯 수치가 바뀌었을 때 해당 UI만 찾아서 갱신하는 함수
    public void RefreshStatItem(IStatData updatedData)
    {
        if (updatedData == null) return;

        var targetItem = spawnedItems.Find(item => item.TargetStatType == updatedData.Type);
        if (targetItem != null)
        {
            targetItem.UpdateUI(updatedData);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("설정")] 
    public string unitPoolName = "UnitTest_Prefab";
    public Transform gridPanel;

    public void SpawnTestUnit()
    {
        List<Transform> emptySlots = new List<Transform>();

        foreach (Transform slot in gridPanel)
        {
            if (slot.childCount == 0)
            {
                emptySlots.Add(slot);
            }
        }

        if (emptySlots.Count > 0)
        {
            Transform targetSlot = emptySlots[0]; 
            UnitFactory.instance.CreateUnit(unitPoolName, 1, targetSlot);
        }
        else
        {
            Debug.Log("그리드가 꽉 찼습니다! 소환 불가.");
        }
    }
}
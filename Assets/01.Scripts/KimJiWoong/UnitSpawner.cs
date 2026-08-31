using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("설정")]
    public GameObject unitPrefab;
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
            GameObject newUnit = Instantiate(unitPrefab, targetSlot);

            newUnit.transform.localPosition = Vector3.zero;
            newUnit.transform.localScale = Vector3.one; // 크기가 이상해지는 현상 방지!
        }
        else
        {
            Debug.Log("그리드가 꽉 찼습니다! 소환 불가.");
        }
    }
}
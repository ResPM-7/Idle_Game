using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("설정")] 
    public Transform gridPanel;
    public UnitDataSO baseUnitData; // 1스테이지
    [SerializeField] private UnitDataSO[] upgradedSpawnData; // 2스테이지부터 순서대로

    [Header("비용 설정")]
    [SerializeField] private int spawnCost = 10;

    private List<Transform> gridSlots = new List<Transform>();

    private UnitDataSO currentSpawnData;

    private void Awake()
    {
        // 그리드 하위의 모든 슬롯을 찾아 리스트에 넣습니다.
        foreach (Transform child in gridPanel)
        {
            gridSlots.Add(child);
        }

        Debug.Log($"초기화 완료: 총 {gridSlots.Count}개의 그리드 슬롯이 리스트에 저장되었습니다.");
    }


    private void Start()
    {
        UpdateSpawnData(WaveManager.instance.CurrentStage);
    }

    private void OnEnable()
    {
        WaveManager.instance.OnStageChanged += UpdateSpawnData;
    }


    private void OnDisable()
    {
        WaveManager manager = WaveManager.instance;

        if (manager != null)
            manager.OnStageChanged -= UpdateSpawnData;
    }

    private void UpdateSpawnData(int stage)
    {
        currentSpawnData = baseUnitData;

        if (stage < 2 || upgradedSpawnData == null || upgradedSpawnData.Length == 0)
            return;

        int index = Mathf.Min(stage - 2, upgradedSpawnData.Length - 1);
        if (upgradedSpawnData[index] != null)
            currentSpawnData = upgradedSpawnData[index];
    }

    public void SpawnTestUnit()
    {
        Transform targetSlot = null;

        // 미리 저장해둔 리스트만 빠르게 검사합니다.
        foreach (Transform slot in gridSlots)
        {
            if (slot.childCount == 0) // 자식이 없다면 빈 칸
            {
                targetSlot = slot;
                break; // 첫 번째 빈 칸을 찾았으니 즉시 반복문 탈출!
            }
        }

        // 빈 칸을 찾았으면 소환, 못 찾았으면 꽉 찬 상태
        if (targetSlot != null)
        {
            if (MoneyManager.instance != null && MoneyManager.instance.SpendCredit(spawnCost))
            {
                GridUnitFactory.instance.CreateUnit(
                    currentSpawnData.uiPoolName,
                    currentSpawnData,
                    targetSlot
                    );
            }
        }
        else
        {
            Debug.Log("그리드가 꽉 찼습니다! 소환 불가.");
        }
    }
}
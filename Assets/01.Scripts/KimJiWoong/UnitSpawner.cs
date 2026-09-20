using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("설정")]
    public Transform gridPanel;
    [SerializeField] private UnitDataSO playerUnitData;
    [SerializeField] private int baseUnitId = 101;
    [SerializeField] private int[] upgradedSpawnUnitIds = { 102, 103, 104, 105 };

    [Header("비용 설정")]
    [SerializeField] private int spawnCost = 10;

    private List<Transform> gridSlots = new List<Transform>();

    private UnitData currentSpawnData;

    private void Awake()
    {
        // 그리드 하위의 모든 슬롯을 찾아 리스트에 넣습니다.
        foreach (Transform child in gridPanel)
        {
            gridSlots.Add(child);
        }

        Debug.Log($"초기화 완료: 총 {gridSlots.Count}개의 그리드 슬롯이 리스트에 저장되었습니다.");
    }

    private void OnEnable()
    {
        WaveManager.instance.OnStageChanged += UpdateSpawnData;
    }

    private void Start()
    {
        UpdateSpawnData(WaveManager.instance.CurrentStage);
    }

    private void OnDisable()
    {
        if (WaveManager.instance != null)
            WaveManager.instance.OnStageChanged -= UpdateSpawnData;
    }

    private void UpdateSpawnData(int stage)
    {
        if (playerUnitData == null)
        {
            currentSpawnData = null;
            Debug.LogWarning("UnitSpawner에 PlayerUnitData가 연결되지 않았습니다.");
            return;
        }

        int targetId = baseUnitId;

        if (stage >= 2 && upgradedSpawnUnitIds != null && upgradedSpawnUnitIds.Length > 0)
        {
            int index = Mathf.Min(stage - 2, upgradedSpawnUnitIds.Length - 1);
            targetId = upgradedSpawnUnitIds[index];
        }

        currentSpawnData = playerUnitData.GetById(targetId);
        if (currentSpawnData == null)
            Debug.LogWarning($"PlayerUnitData에서 unitId {targetId}를 찾을 수 없습니다.");
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
        if (targetSlot != null && currentSpawnData != null)
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

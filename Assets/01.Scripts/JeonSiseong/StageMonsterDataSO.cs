using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageMonsterDataSO", menuName = "Data/Stage Monster Data")]
public class StageMonsterDataSO : ScriptableObject
{
    [Header("사이클 설정")]
    public int cycleLength = 10;              // 사이클 길이 (3+3+4 = 10스테이지)
    public float multiplierPerCycle = 0.5f;   // 사이클 한 바퀴마다 배율 증가폭

    public List<StageMonsterEntry> entries = new List<StageMonsterEntry>();

    public StageMonsterEntry GetEntryForStage(int stage)        // 사이클 반복
    {
        int relativeStage = ((stage - 1) % cycleLength) + 1;

        foreach (var entry in entries)
        {
            if (relativeStage >= entry.startStage && relativeStage <= entry.endStage)
                return entry;
        }

        Debug.LogWarning($"스테이지 {stage} (사이클 내 {relativeStage})에 해당하는 StageMonsterData가 없습니다. 마지막 구간을 사용합니다.");
        return entries.Count > 0 ? entries[entries.Count - 1] : null;
    }

    public float GetMultiplierForStage(int stage)      // 사이클마다 배율 적용
    {
        int cycleCount = (stage - 1) / cycleLength;
        return Mathf.Pow(1f +  multiplierPerCycle, cycleCount);
    }
}

[System.Serializable]
public class StageMonsterEntry
{
    public int startStage;
    public int endStage;
    public UnitDataSO[] enemyPool;
    public UnitDataSO bossData;
}
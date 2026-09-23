using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageBossDataSO", menuName = "Data/Stage Boss Data")]
public class StageBossDataSO : ScriptableObject
{
    [Header("보스 사이클")]
    public int cycleLength = 6;
    public float multiplierPerCycle = 0.5f;

    public List<StageBossEntry> entries = new List<StageBossEntry>();

    public StageBossEntry GetBossForStage(int stage)
    {
        int relativeStage = ((stage - 1) % cycleLength) + 1;

        foreach (var entry in entries)
        {
            if (entry.stage == relativeStage)
                return entry;
        }

        Debug.LogWarning($"스테이지 {stage}에 해당하는 보스 데이터가 없습니다.");
        return null;
    }

    public float GetMultiplierForStage(int stage)
    {
        int cycleCount = (stage - 1) / cycleLength;

        return Mathf.Pow(1f + multiplierPerCycle, cycleCount);
    }
}

[System.Serializable]
public class StageBossEntry
{
    public int stage;
    public UnitDataSO bossData;
}
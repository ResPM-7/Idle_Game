using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageMonsterDataSO", menuName = "Data/Stage Monster Data")]
public class StageMonsterDataSO : ScriptableObject
{
    public List<StageMonsterEntry> entries = new List<StageMonsterEntry>();

    public StageMonsterEntry GetEntryForStage(int stage)
    {
        foreach (var entry in entries)
        {
            if (stage >= entry.startStage && stage <= entry.endStage)
                return entry;
        }

        Debug.LogWarning($"스테이지 {stage}에 해당하는 StageMonsterData가 없습니다. 마지막 구간을 사용합니다.");
        return entries.Count > 0 ? entries[entries.Count - 1] : null;
    }
}

[System.Serializable]
public class StageMonsterEntry
{
    public int startStage;
    public int endStage;
    public int enemyId;
    public int bossId;
    public UnitDataSO enemyData;
    public UnitDataSO bossData;
}

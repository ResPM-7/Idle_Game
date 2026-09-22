using System;
using System.Collections.Generic;

// SO/씬 오브젝트 참조 대신 ID와 슬롯 번호만 저장합니다.
[Serializable]
public class GameSaveData
{
    public int version = 1;
    public int gold;
    public int credit;
    public int guildLevel = 1;
    public int stage = 1;
    public List<SavedUnitSlot> units = new List<SavedUnitSlot>();
    public List<SavedUpgrade> upgrades = new List<SavedUpgrade>();
}

[Serializable]
public class SavedUnitSlot
{
    public bool party;
    public int slot;
    public int unitId;
}

[Serializable]
public class SavedUpgrade
{
    public StatType type;
    public int level;
}

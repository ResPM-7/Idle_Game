using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitData
{
    // <sheet-fields>
    public int @unitId;
    public string @uiPoolName;
    public string @battlePoolName;
    public int @unitLevel;
    public string @unitName;
    public float @maxHp;
    public float @moveSpeed;
    public float @attackDamage;
    public float @attackSpeed;
    public float @attackRange;
    public int @defense;
    public int @criticalRate;
    public float @criticalDamage;
    public int @coin;
    public int @credit;
    // </sheet-fields>

    public Sprite unitSprite;
    public int[] nextUpgradeUnitIds = Array.Empty<int>();

    [NonSerialized] private UnitDataSO database;

    internal void Bind(UnitDataSO owner)
    {
        database = owner;
    }

    public UnitData GetNextUpgradeUnit()
    {
        if (database == null || nextUpgradeUnitIds == null || nextUpgradeUnitIds.Length == 0)
            return null;

        int index = nextUpgradeUnitIds.Length == 1
            ? 0
            : UnityEngine.Random.Range(0, nextUpgradeUnitIds.Length);
        return database.GetById(nextUpgradeUnitIds[index]);
    }
}

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Data/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [SerializeField] private List<UnitData> units = new List<UnitData>();
    private Dictionary<int, UnitData> lookup;

    public IReadOnlyList<UnitData> Units => units;
    public int Count => units.Count;

    public UnitData GetById(int unitId)
    {
        EnsureLookup();
        return lookup.TryGetValue(unitId, out UnitData unit) ? unit : null;
    }

    public bool TryGetById(int unitId, out UnitData unit)
    {
        EnsureLookup();
        return lookup.TryGetValue(unitId, out unit);
    }

    public void SetUnits(IEnumerable<UnitData> newUnits)
    {
        units.Clear();
        if (newUnits != null) units.AddRange(newUnits);
        RebuildLookup();
    }

    private void OnEnable() => RebuildLookup();
    private void OnValidate() => RebuildLookup();

    private void EnsureLookup()
    {
        if (lookup == null) RebuildLookup();
    }

    private void RebuildLookup()
    {
        lookup = new Dictionary<int, UnitData>();
        foreach (UnitData unit in units)
        {
            if (unit == null) continue;
            unit.Bind(this);
            if (!lookup.ContainsKey(unit.unitId)) lookup.Add(unit.unitId, unit);
        }
    }
}

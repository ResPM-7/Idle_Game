using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    private const string HpBarPoolKey = "HpBar";

    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0f, 0f);

    private readonly Dictionary<Unit_Base_Test, UI_HealthBar> healthBars = new();
    private readonly List<Unit_Base_Test> pendingUnits = new();

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void OnEnable()
    {
        Unit_Base_Test.OnUnitSpawned += HandleUnitSpawned;
        Unit_Base_Test.OnUnitDespawned += HandleUnitDespawned;
    }

    private void OnDisable()
    {
        Unit_Base_Test.OnUnitSpawned -= HandleUnitSpawned;
        Unit_Base_Test.OnUnitDespawned -= HandleUnitDespawned;

        foreach (var pair in healthBars)
        {
            ReturnHealthBar(pair.Value);
        }

        healthBars.Clear();
        pendingUnits.Clear();
    }

    private void LateUpdate()
    {
        TryCreatePendingHealthBars();
        UpdateHealthBarPositions();
    }

    private void HandleUnitSpawned(Unit_Base_Test unit)
    {
        if (unit == null ||
            healthBars.ContainsKey(unit) ||
            pendingUnits.Contains(unit))
        {
            return;
        }

        pendingUnits.Add(unit);
    }

    private void HandleUnitDespawned(Unit_Base_Test unit)
    {
        pendingUnits.Remove(unit);

        if (unit != null && healthBars.Remove(unit, out UI_HealthBar healthBar))
        {
            ReturnHealthBar(healthBar);
        }
    }

    private void TryCreatePendingHealthBars()
    {
        if (ObjectPoolManager.instance == null)
            return;

        for (int i = pendingUnits.Count - 1; i >= 0; i--)
        {
            Unit_Base_Test unit = pendingUnits[i];

            if (unit == null || !unit.gameObject.activeInHierarchy)
            {
                pendingUnits.RemoveAt(i);
                continue;
            }

            GameObject healthBarObject =
                ObjectPoolManager.instance.GetObject(HpBarPoolKey);

            if (healthBarObject == null)
                return;

            if (!healthBarObject.TryGetComponent(out UI_HealthBar healthBar))
            {
                ObjectPoolManager.instance.ReturnObject(
                    HpBarPoolKey,
                    healthBarObject
                );

                pendingUnits.RemoveAt(i);
                Debug.LogWarning("HpBar prefab에 UI_HealthBar가 없습니다.");
                continue;
            }

            float maxHp = unit.myData.maxHp;
            float normalizedHp = maxHp > 0f
                ? unit.currentHp / maxHp
                : 0f;

            healthBar.SetFill(normalizedHp);
            healthBar.SetVisible(true);

            healthBars.Add(unit, healthBar);
            pendingUnits.RemoveAt(i);
        }
    }

    private void UpdateHealthBarPositions()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
            return;

        foreach (var pair in healthBars)
        {
            Unit_Base_Test unit = pair.Key;
            UI_HealthBar healthBar = pair.Value;

            if (unit == null || healthBar == null)
                continue;

            Vector3 screenPosition = worldCamera.WorldToScreenPoint(
                unit.transform.position + worldOffset
            );

            bool isVisible = screenPosition.z > 0f;

            healthBar.SetVisible(isVisible);

            if (isVisible)
                healthBar.SetScreenPosition(screenPosition);
        }
    }

    private void ReturnHealthBar(UI_HealthBar healthBar)
    {
        if (healthBar == null || ObjectPoolManager.instance == null)
            return;

        healthBar.SetVisible(false);

        ObjectPoolManager.instance.ReturnObject(
            HpBarPoolKey,
            healthBar.gameObject
        );
    }
}
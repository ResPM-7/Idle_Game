using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    private const string HpBarPoolKey = "HpBar";

    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0f, 0f);

    private readonly Dictionary<Unit_Base_Test, UI_HealthBar> healthBars = new();
    private readonly List<Unit_Base_Test> pendingUnits = new();
    private readonly List<Unit_Base_Test> pendingBlockedUnits = new();

    private readonly HashSet<Unit_Base_Test> revealedEnemyHealthBars = new();
    [SerializeField] private LayerMask enemyLayer;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

#if UNITY_EDITOR // TakeDamage 테스트용 함수
    private void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.qKey.wasPressedThisFrame)
        {
            TestDamageAllUnits(10f);
        }

        if (keyboard.wKey.wasPressedThisFrame)
        {
            TestHealAllUnits(10f);
        }

        if (keyboard.bKey.wasPressedThisFrame)
        {
            TestDamagePopup(0f, false);
        }

        if (keyboard.cKey.wasPressedThisFrame)
        {
            TestDamagePopup(1234f, true);
        }
    }

    private void TestDamagePopup(float damage, bool isCritical)
    {
        foreach (Unit_Base_Test unit in healthBars.Keys)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy)
                continue;

            UIManager.Instance?.ShowDamageText(damage, unit.transform.position, isCritical);
            return;
        }

        Debug.LogWarning("Damage Popup을 표시할 활성 유닛이 없습니다.");
    }

    private void TestDamageAllUnits(float damage)
    {
        var unitSnapshot = new List<Unit_Base_Test>(healthBars.Keys);

        foreach (Unit_Base_Test unit in unitSnapshot)
        {
            if (unit == null || unit.MyData == null || !unit.gameObject.activeInHierarchy)
            {
                continue;
            }

            unit.TakeDamage(damage);
        }
    }

    private void TestHealAllUnits(float amount)
    {
        var healthBarSnapshot = new List<KeyValuePair<Unit_Base_Test, UI_HealthBar>>(healthBars);

        foreach (var pair in healthBarSnapshot)
        {
            Unit_Base_Test unit = pair.Key;
            UI_HealthBar healthBar = pair.Value;

            if (unit == null || unit.MyData == null || healthBar == null || !unit.gameObject.activeInHierarchy)
            {
                continue;
            }

            unit.CurrentHp = Mathf.Min(unit.CurrentHp + amount,unit.MyData.maxHp);

            float normalizedHp = unit.MyData.maxHp > 0f ? unit.CurrentHp / unit.MyData.maxHp : 0f;

            healthBar.SetFill(normalizedHp);
        }
    }
#endif

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
            Unit_Base_Test unit = pair.Key;
            UI_HealthBar healthBar = pair.Value;

            if (unit != null)
                unit.OnHpChanged -= HandleUnitHpChanged;

            ReturnHealthBar(healthBar);
        }

        healthBars.Clear();
        pendingUnits.Clear();
        pendingBlockedUnits.Clear();
        revealedEnemyHealthBars.Clear();
    }

    private void LateUpdate()
    {
        TryCreatePendingHealthBars();
        ShowPendingBlockedPopups();
        UpdateHealthBarPositions();
    }

    private void HandleUnitSpawned(Unit_Base_Test unit)
    {
        if (unit == null)
        {
            return;
        }

        // Init에서 전달되는 damage 0 이벤트는 실제 BLOCKED가 아니므로 취소한다.
        pendingBlockedUnits.RemoveAll(pendingUnit => pendingUnit == unit);
        revealedEnemyHealthBars.Remove(unit);

        if (healthBars.TryGetValue(unit, out UI_HealthBar existingHealthBar))
        {
            float maxHp = unit.MyData != null ? unit.MyData.maxHp : 0f;
            float normalizedHp = maxHp > 0f ? unit.CurrentHp / maxHp : 0f;

            existingHealthBar.SetFill(normalizedHp);
            existingHealthBar.SetVisible(false);
            return;
        }

        if (pendingUnits.Contains(unit))
            return;

        pendingUnits.Add(unit);
    }

    private void HandleUnitDespawned(Unit_Base_Test unit)
    {
        pendingUnits.Remove(unit);
        pendingBlockedUnits.RemoveAll(pendingUnit => pendingUnit == unit);

        if (unit == null)
            return;

        unit.OnHpChanged -= HandleUnitHpChanged;
        revealedEnemyHealthBars.Remove(unit);

        if (healthBars.Remove(unit, out UI_HealthBar healthBar))
            ReturnHealthBar(healthBar);
    }
    private void HandleUnitHpChanged(Unit_Base_Test unit, float currentHp, float maxHp, float damage,bool isCritical)
    {
        if (unit == null || !healthBars.TryGetValue(unit, out UI_HealthBar healthBar) || healthBar == null)
        {
            return;
        }

        float normalizedHp = maxHp > 0f ? currentHp / maxHp : 0f;

        healthBar.SetFill(normalizedHp);

        if (damage < 0f)
            return;

        if (Mathf.Approximately(damage, 0f))
        {
            pendingBlockedUnits.Add(unit);
            return;
        }

        ShowDamagePopup(unit, damage, isCritical);
    }

    private void ShowPendingBlockedPopups()
    {
        foreach (Unit_Base_Test unit in pendingBlockedUnits)
        {
            if (unit != null && unit.gameObject.activeInHierarchy && healthBars.ContainsKey(unit))
                ShowDamagePopup(unit, 0f, false);
        }

        pendingBlockedUnits.Clear();
    }

    private void ShowDamagePopup(Unit_Base_Test unit, float damage, bool isCritical)
    {
        if (IsEnemy(unit))
            revealedEnemyHealthBars.Add(unit);

        UIManager.Instance?.ShowDamageText(damage, unit.transform.position, isCritical);
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

            GameObject healthBarObject = ObjectPoolManager.instance.GetObject(HpBarPoolKey);

            if (healthBarObject == null)
                return;

            if (!healthBarObject.TryGetComponent(out UI_HealthBar healthBar))
            {
                ObjectPoolManager.instance.ReturnObject(HpBarPoolKey, healthBarObject);

                pendingUnits.RemoveAt(i);
                Debug.LogWarning("HpBar prefab에 UI_HealthBar가 없습니다.");
                continue;
            }

            float maxHp = unit.CurrentMaxHp;
            float normalizedHp = maxHp > 0f ? unit.CurrentHp / maxHp : 0f;

            healthBar.SetStyle(IsEnemy(unit));
            healthBar.SetFill(normalizedHp);
            // HP바가 풀에서 나온 직후 이전 위치에 잠깐 보이는 것을 방지
            healthBar.SetVisible(false);

            // HP바 생성 전에 이미 피해를 받은 경우에도 표시
            if (IsEnemy(unit) && unit.CurrentHp < maxHp)
            {
                revealedEnemyHealthBars.Add(unit);
            }

            healthBars.Add(unit, healthBar);

            unit.OnHpChanged -= HandleUnitHpChanged;
            unit.OnHpChanged += HandleUnitHpChanged;

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

            bool isVisible = screenPosition.z > 0f && ShouldShowHealthBar(unit); 

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

        ObjectPoolManager.instance.ReturnObject(HpBarPoolKey, healthBar.gameObject);
    }

    //적 판별
    private bool IsEnemy(Unit_Base_Test unit)
    {
        if (unit == null)
            return false;

        return (enemyLayer.value & (1 << unit.gameObject.layer)) != 0;
    }
    private bool ShouldShowHealthBar(Unit_Base_Test unit)
    {
        // 아군은 처음부터 표시
        if (!IsEnemy(unit))
            return true;

        // 적은 한 번이라도 피해를 받은 뒤 표시
        return revealedEnemyHealthBars.Contains(unit);
    }
}

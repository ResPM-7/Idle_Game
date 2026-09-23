using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>임시 씬에서 역할 선택·대상 수명·상태 효과와 탐색의 관리 메모리 할당을 검사합니다.</summary>
public static class UnitCombatSmokeTest
{
    [MenuItem("도구/검증/유닛 전투 행동 검사")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[유닛 행동 검사] 재생을 중지하고 실행해 주세요.");
            return;
        }
        Scene scene = EditorSceneManager.NewPreviewScene();
        UnitDataSO player = null, allyData = null, enemyData = null;
        Unit_Base_Test owner = null, ally = null, enemy = null;
        try
        {
            player = Data(Team_Test.Player);
            allyData = Data(Team_Test.Player);
            enemyData = Data(Team_Test.Enemy);
            player.canMelee = false;
            player.canHeal = true;
            owner = Create(scene, player, Vector3.zero);
            ally = Create(scene, allyData, Vector3.right);
            enemy = Create(scene, enemyData, Vector3.right * 3f);
            ally.CurrentHp = 10f;
            Check(owner.Combat.FindTarget() && owner.CurrentTarget == ally.transform, "회복할 아군 우선 선택");
            Check(owner.Combat.SelectedAction is HealCombatAction && owner.Combat.StopDistance == 2f, "회복 자식 행동과 사거리");
            ally.CurrentHp = ally.CurrentMaxHp;
            Check(!owner.Combat.HasValidTarget, "회복 완료 대상 해제");
            Check(owner.Combat.FindTarget() && owner.CurrentTarget == enemy.transform
                && owner.Combat.SelectedAction is RangedCombatAction, "힐러의 적 원거리 공격 전환");

            player.canMelee = true;
            player.canRanged = true;
            owner.Combat.Configure(owner);
            owner.Combat.FindTarget();
            Check(owner.Combat.SelectedAction is MeleeCombatAction, "근접·원거리 동시 설정 시 근접 우선");
            ally.CurrentHp = 50f;
            owner.Combat.FindTarget();
            Check(owner.Combat.SelectedAction is HealCombatAction, "복합 역할도 회복 우선");
            ally.CurrentHp = ally.CurrentMaxHp;
            player.canMelee = false;
            player.canHeal = false;
            owner.Combat.Configure(owner);
            owner.Combat.FindTarget();
            Check(owner.Combat.SelectedAction is RangedCombatAction && owner.Combat.IsInRange(), "원거리 역할과 거리 판정");
            enemy.gameObject.SetActive(false);
            enemy.Init(enemyData);
            enemy.gameObject.SetActive(true);
            Check(!owner.Combat.HasValidTarget, "같은 객체의 재생성 감지");
            player.canRanged = false;
            owner.Combat.Configure(owner);
            Check(!owner.Combat.FindTarget(), "모든 행동이 꺼진 유닛은 공격하지 않음");

            var effects = new UnitStatusEffects();
            effects.ApplyDamageBuff(1.5f, 2f);
            effects.ApplyFreeze(1f);
            effects.Tick(1.1f);
            Check(!effects.IsFrozen && effects.DamageMultiplier == 1.5f, "빙결과 버프의 독립 타이머");
            effects.Tick(1f);
            Check(effects.DamageMultiplier == 1f, "버프 만료");
            owner.CurrentDamage = 20f;
            owner.ApplyDamageBuff(1.5f, 2f);
            Check(owner.CurrentDamage == 30f, "기본 공격력과 일시 배율 합성");
            owner.CurrentDamage = 40f;
            Check(owner.CurrentDamage == 60f, "버프 중 강화 반영");
            owner.ApplyDamageBuff(1f, 0f);
            Check(owner.CurrentDamage == 40f, "버프 종료 시 강화 유지");

            player.canRanged = true;
            owner.Combat.Configure(owner);
            // 최초 호출 준비 비용을 제외한 탐색·유효성·거리·효과 타이머만 측정합니다.
            for (int i = 0; i < 100; i++) RunLoop(owner, effects);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) RunLoop(owner, effects);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Check(allocated == 0, "반복 탐색·판정에서 관리 메모리 할당: " + allocated + "바이트");
            Debug.Log("[유닛 행동 검사 통과] 역할 선택·복합 역할·대상 재생성·버프와 강화·반복 탐색 할당을 확인했습니다. 투사체·애니메이션·외부 이벤트는 실제 전투 프로파일링이 필요합니다.");
        }
        finally
        {
            if (owner != null) UnitTargetRegistry.Unregister(owner);
            if (ally != null) UnitTargetRegistry.Unregister(ally);
            if (enemy != null) UnitTargetRegistry.Unregister(enemy);
            EditorSceneManager.ClosePreviewScene(scene);
            if (player != null) UnityEngine.Object.DestroyImmediate(player);
            if (allyData != null) UnityEngine.Object.DestroyImmediate(allyData);
            if (enemyData != null) UnityEngine.Object.DestroyImmediate(enemyData);
        }
    }
    private static void RunLoop(Unit_Base_Test owner, UnitStatusEffects effects)
    {
        owner.Combat.FindTarget();
        owner.Combat.IsInRange();
        effects.Tick(0.01f);
    }
    private static UnitDataSO Data(Team_Test team)
    {
        var data = ScriptableObject.CreateInstance<UnitDataSO>();
        data.team = team;
        data.maxHp = 100f;
        data.attackDamage = 10f;
        data.attackRange = 8f;
        data.healRange = 2f;
        data.searchRange = 20f;
        return data;
    }
    private static Unit_Base_Test Create(Scene scene, UnitDataSO data, Vector3 position)
    {
        var obj = new GameObject("전투행동검사") { hideFlags = HideFlags.HideAndDontSave };
        obj.SetActive(false);
        SceneManager.MoveGameObjectToScene(obj, scene);
        obj.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        obj.AddComponent<CircleCollider2D>();
        var unit = obj.AddComponent<Unit_Base_Test>();
        obj.transform.position = position;
        unit.Init(data);
        obj.SetActive(true);
        UnitTargetRegistry.Register(unit);
        return unit;
    }
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("[유닛 행동 검사 실패] " + message);
    }
}

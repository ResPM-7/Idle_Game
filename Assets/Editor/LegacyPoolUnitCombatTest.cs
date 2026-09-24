using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>기존 풀용 유닛의 행동 선택과 효과를 임시 씬에서 검사합니다. 실제 물리 탐색과 풀 반복은 플레이 검증이 필요합니다.</summary>
public static class LegacyPoolUnitCombatTest
{
    [MenuItem("도구/검증/기존 풀 유닛 행동 검사")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[유닛 검사] 재생을 중지하고 실행해 주세요.");
            return;
        }
        Scene scene = EditorSceneManager.NewPreviewScene();
        UnitDataSO player = ScriptableObject.CreateInstance<UnitDataSO>();
        UnitDataSO enemyData = ScriptableObject.CreateInstance<UnitDataSO>();
        try
        {
            player.maxHp = enemyData.maxHp = 100f;
            player.attackRange = 5f;
            player.healRange = 2f;
            player.team = Team_Test.Player;
            enemyData.team = Team_Test.Enemy;
            var owner = Create(scene, player, Vector3.zero);
            var enemy = Create(scene, enemyData, Vector3.right * 3f);
            owner.CurrentTarget = enemy.transform;
            Check(owner.Combat.SelectedAction is MeleeCombatAction && owner.Combat.IsInRange(), "근접 행동·사거리");
            player.canMelee = false;
            player.canRanged = true;
            owner.Combat.Configure();
            owner.CurrentTarget = enemy.transform;
            Check(owner.Combat.SelectedAction is RangedCombatAction, "원거리 행동");
            player.canHeal = true;
            owner.Combat.Configure();
            owner.CurrentHp = 10f;
            owner.CurrentTarget = owner.transform;
            Check(owner.Combat.SelectedAction is HealCombatAction, "회복 행동·자기 회복 호환");
            owner.CurrentHp = owner.CurrentMaxHp;
            Check(!owner.Combat.HasValidTarget, "회복 완료 대상 무효화");
            owner.CurrentTarget = enemy.transform;
            Check(owner.Combat.SelectedAction is RangedCombatAction, "힐러의 적 공격 호환");
            enemy.Init(enemyData);
            Check(!owner.Combat.HasValidTarget, "재생성된 타깃 감지");
            owner.CurrentDamage = 20f;
            owner.ApplyDamageBuff(1.5f, 2f);
            Check(owner.CurrentDamage == 30f, "일시 버프 적용");
            owner.CurrentDamage = 40f;
            Check(owner.CurrentDamage == 60f, "버프 중 강화 유지");
            owner.ApplyDamageBuff(1f, 0f);
            Check(owner.CurrentDamage == 40f, "일시 버프 해제");
            var effects = new UnitStatusEffects();
            effects.ApplyFreeze(1f);
            effects.Tick(2f);
            Check(!effects.IsFrozen, "빙결 만료");
            player.canMelee = player.canRanged = player.canHeal = false;
            owner.Combat.Configure();
            owner.CurrentTarget = enemy.transform;
            Check(!owner.Combat.HasValidTarget, "모든 행동 비활성");
            Debug.Log("[유닛 행동 검사 통과] 역할·거리·회복·재생성·버프를 확인했습니다. 풀 반복 및 GC는 실제 플레이에서 확인해 주세요.");
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(scene);
            UnityEngine.Object.DestroyImmediate(player);
            UnityEngine.Object.DestroyImmediate(enemyData);
        }
    }
    private static Unit_Base_Test Create(Scene scene, UnitDataSO data, Vector3 position)
    {
        var obj = new GameObject("유닛행동검사") { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(obj, scene);
        obj.transform.position = position;
        obj.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        var unit = obj.AddComponent<Unit_Base_Test>();
        unit.Init(data);
        return unit;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("[유닛 행동 검사 실패] " + message);
    }
}

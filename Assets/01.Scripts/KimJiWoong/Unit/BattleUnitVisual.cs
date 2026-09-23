using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 전투 판정은 기존 루트가 담당하고 SPUM 자식은 외형/애니메이션만 담당합니다.
[DisallowMultipleComponent]
public class BattleUnitVisual : MonoBehaviour
{
    private static BattleVisualCatalog catalog;
    private readonly Dictionary<GameObject, SPUM_Prefabs> cached = new Dictionary<GameObject, SPUM_Prefabs>();
    private Unit_Base_Test unit;
    private SpriteRenderer placeholder;
    private SPUM_Prefabs current;
    private BattleVisualCatalog.Entry entry;
    private Vector3 previousPosition;
    private Vector3 baseScale;
    private SortingGroup sorting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache() { catalog = null; }

    public void Apply(Unit_Base_Test owner)
    {
        unit = owner;
        if (placeholder == null) placeholder = GetComponent<SpriteRenderer>();
        if (catalog == null) catalog = Resources.Load<BattleVisualCatalog>("GameData/BattleVisualCatalog");
        entry = catalog != null ? catalog.Find(owner.MyData) : null;
        if (current != null) current.gameObject.SetActive(false);
        current = null;
        if (entry == null || entry.prefab == null)
        {
            if (placeholder != null) placeholder.enabled = true;
            Debug.LogWarning($"전투 외형 연결 누락: {owner.MyData.unitName}", this);
            return;
        }

        if (!cached.TryGetValue(entry.prefab, out current) || current == null)
        {
            var visual = Instantiate(entry.prefab, transform, false);
            visual.name = "BattleVisual_" + entry.prefab.name;
            visual.transform.localRotation = Quaternion.identity;
            current = visual.GetComponent<SPUM_Prefabs>();
            if (current == null || current._anim == null)
            {
                Destroy(visual);
                current = null;
                if (placeholder != null) placeholder.enabled = true;
                Debug.LogError("SPUM 외형에 SPUM_Prefabs 또는 Animator가 없습니다.", entry.prefab);
                return;
            }
            // 같은 종류로 다시 소환되면 이미 만든 외형을 재사용합니다.
            // 일부 제공 프리팹은 클립 목록이 비어 있어 패키지 정보에서 채워야 합니다.
            if (current.IDLE_List.Count == 0 || current.MOVE_List.Count == 0 || current.ATTACK_List.Count == 0)
                current.PopulateAnimationLists();
            current.OverrideControllerInit();
            cached[entry.prefab] = current;
        }

        current.gameObject.SetActive(true);
        current.transform.localPosition = entry.offset;
        baseScale = Vector3.one * entry.scale;
        current.transform.localScale = baseScale;
        current._anim.speed = 1f;
        current._anim.Rebind();
        current._anim.Update(0f);
        Play(PlayerState.IDLE, 0);
        Play(PlayerState.MOVE, 0);
        current._anim.SetBool("1_Move", false);
        sorting = current.GetComponent<SortingGroup>();
        if (sorting == null) sorting = current.gameObject.AddComponent<SortingGroup>();
        sorting.sortingLayerID = placeholder != null ? placeholder.sortingLayerID : 0;
        if (placeholder != null) placeholder.enabled = false;
        previousPosition = transform.position;
        Face(owner.MyData.team == Team_Test.Player ? 1f : -1f);
    }

    public void PlayAttack()
    {
        if (current == null || unit.IsFrozen) return;
        Play(PlayerState.ATTACK, entry.attackAnimation);
    }

    private void Play(PlayerState state, int index)
    {
        if (!current.StateAnimationPairs.TryGetValue(state.ToString(), out var clips) || clips.Count == 0)
            return;
        index = Mathf.Clamp(index, 0, clips.Count - 1);
        if (clips[index] == null) return;
        current.OverrideController[state.ToString()] = clips[index];
        if (state == PlayerState.ATTACK)
        {
            current._anim.SetBool("1_Move", false);
            current._anim.SetTrigger("2_Attack");
        }
    }

    private void LateUpdate()
    {
        if (current == null || unit == null) return;
        Vector3 delta = transform.position - previousPosition;
        previousPosition = transform.position;
        current._anim.speed = unit.IsFrozen ? 0f : 1f;
        current._anim.SetBool("1_Move", !unit.IsFrozen && delta.sqrMagnitude > 0.000001f);
        if (unit.CurrentTarget != null)
            Face(unit.CurrentTarget.position.x - transform.position.x);
        else if (Mathf.Abs(delta.x) > 0.001f) Face(delta.x);
        // 캐릭터 파츠 순서를 유지하며 캐릭터 전체의 앞뒤를 정렬합니다.
        sorting.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
    }

    private void Face(float direction)
    {
        if (Mathf.Abs(direction) < 0.001f) return;
        var scale = baseScale;
        scale.x *= (direction > 0f) == entry.facesLeft ? -1f : 1f;
        current.transform.localScale = scale;
    }

    private void OnDisable()
    {
        if (current != null) current.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var visual in cached.Values)
            if (visual != null && visual.OverrideController != null)
                Destroy(visual.OverrideController);
    }
}

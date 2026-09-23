using UnityEngine;
using UnityEngine.Rendering;

// 전투 판정은 기존 루트가 담당하고 SPUM 자식은 외형/애니메이션만 담당합니다.
[DisallowMultipleComponent]
public class BattleUnitVisual : MonoBehaviour
{
    [SerializeField] private bool facesLeft = true;
    [SerializeField, Min(0)] private int attackAnimation;
    private bool initialized;
    private Transform visualRoot;
    private AnimatorOverrideController ownedController;
    private Unit_Base_Test unit;
    private SPUM_Prefabs current;
    private Vector3 previousPosition;
    private Vector3 baseScale;
    private SortingGroup sorting;


    public void Apply(Unit_Base_Test owner)
    {
        unit = owner;
        // 이미 풀링된 프리팹 내부의 외형을 찾습니다. 외형을 추가 생성하지 않습니다.
        if (!initialized)
        {
            current = GetComponentInChildren<SPUM_Prefabs>(true);
            if (current == null)
            {
                Debug.LogError($"{name}: SPUM_Prefabs 컴포넌트가 없습니다.", this);
                return;
            }
            if (current._anim == null || current._anim.runtimeAnimatorController == null)
            {
                Debug.LogError($"{name}: SPUM Animator/Controller 연결을 확인해주세요.", this);
                current = null;
                return;
            }
            if (current.IDLE_List.Count == 0 || current.MOVE_List.Count == 0 || current.ATTACK_List.Count == 0)
                current.PopulateAnimationLists();
            // Animator Controller는 풀 객체당 한 번만 준비합니다.
            current.OverrideControllerInit();
            ownedController = current.OverrideController;
            visualRoot = current._anim.transform;
            baseScale = visualRoot.localScale;
            sorting = GetComponent<SortingGroup>();
            if (sorting == null)
            {
                Debug.LogError(
                    $"{name}: SortingGroup 컴포넌트가 없습니다. 전투 프리팹에 직접 추가해주세요.",
                    this
                );
                return;
            }
            initialized = true;
        }
        current._anim.speed = 1f;
        current._anim.Rebind();
        current._anim.Update(0f);
        Play(PlayerState.IDLE, 0);
        Play(PlayerState.MOVE, 0);
        current._anim.ResetTrigger("2_Attack");
        current._anim.SetBool("1_Move", false);
        sorting.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
        previousPosition = transform.position;
        Face(owner.MyData.team == Team_Test.Player ? 1f : -1f);
    }

    public void PlayAttack()
    {
        if (!initialized || unit == null || unit.IsFrozen) return;
        Play(PlayerState.ATTACK, attackAnimation);
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
        if (!initialized || current == null || unit == null) return;
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
        scale.x = Mathf.Abs(scale.x) * (((direction > 0f) == facesLeft) ? -1f : 1f);
        visualRoot.localScale = scale;
    }

    // SPUM이 풀 객체 루트에 있으므로 별도로 비활성화하지 않습니다.

    private void OnDestroy()
    {
        if (ownedController != null) Destroy(ownedController);
    }
}

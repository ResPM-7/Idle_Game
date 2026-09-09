using UnityEngine;

// TestCanvas 하위의 Panel 오브젝트에 붙어있는 스크립트입니다.
public class BattleSlotPanel : MonoBehaviour
{
    [Header("이 파티의 슬롯들")]
    [SerializeField] private BattleSlotUI[] battleSlots;

    private void Start()
    {
        //자식 오브젝트들을 찾아 배열에 미리 싹 담아둡니다
        if (battleSlots == null)
            battleSlots = GetComponentsInChildren<BattleSlotUI>();
    }

    // 어느 슬롯이든 유닛 배치가 바뀌면 무조건 이 함수를 한 번 호출합니다.
    public void DeployParty()
    {
        for (int i = 0; i < battleSlots.Length; i++)
        {
            BattleSlotUI slot = battleSlots[i];

            if (slot.transform.childCount > 0)
            {
                // UI 슬롯에 유닛이 있다면, 파티 매니저에게 소환(또는 유지) 명령!
                DragableUnit uiUnit = slot.transform.GetChild(0).GetComponent<DragableUnit>();
                PartyBuildManager.instance.DeployUnit(i, uiUnit.myData);
            }
            else
            {
                // UI 슬롯이 비어있다면, 파티 매니저에게 해당 자리를 비우라고 명령!
                PartyBuildManager.instance.RemoveUnit(i);
            }
        }

        Debug.Log($"[{gameObject.name}] 파티 데이터 전송 완료! 전장 세팅 끝!");
    }
}
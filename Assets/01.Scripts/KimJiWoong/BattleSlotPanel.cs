using UnityEngine;

// TestCanvas 하위의 Panel 오브젝트에 붙어있는 스크립트입니다.
public class BattleSlotPanel : Singleton<BattleSlotPanel>
{

    // 5개의 배틀 슬롯을 캐싱해둘 배열
    [SerializeField] private BattleSlotUI[] battleSlots;

    private void Start()
    {
        //자식 오브젝트들을 찾아 배열에 미리 싹 담아둡니다
        if (battleSlots == null)
            battleSlots = GetComponentsInChildren<BattleSlotUI>();
    }

    // 어느 슬롯이든 유닛 배치가 바뀌면 무조건 이 함수를 한 번 호출합니다.
    public void SyncAllBattleSlots()
    {
        for (int i = 0; i < battleSlots.Length; i++)
        {
            BattleSlotUI slot = battleSlots[i];

            if (slot.transform.childCount > 0)
            {
                // UI에 유닛이 있다면 파티 매니저에게 배치 명령!
                // (PartyManager 쪽에서 이미 같은 데이터면 무시하도록 처리해둠)
                DragableUnit unit = slot.transform.GetChild(0).GetComponent<DragableUnit>();
                PartyManager.instance.DeployUnit(slot.slotIndex, unit.myData);
            }
            else
            {
                // UI가 비어있다면 파티 매니저에게 비우기 명령!
                PartyManager.instance.RemoveUnit(slot.slotIndex);
            }
        }
    }
}
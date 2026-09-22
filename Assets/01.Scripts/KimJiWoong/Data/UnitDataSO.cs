using UnityEngine;

public enum Team_Test { Player, Enemy }

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Data/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public int unitId;
    public int unitLevel;
    public string unitName;
    public Team_Test team = Team_Test.Player;

    public string uiPoolName;
    public string battlePoolName;

    [Header("외형 정보")]
    public Sprite unitSprite;

    [Header("전투 정보")]
    public float maxHp;
    public float moveSpeed;
    public float attackDamage;
    public float attackSpeed;
    public float searchRange;
    public float attackRange;

    public bool canMelee = true;
    public bool canRanged = false;
    public string projectilePoolName;

    public bool canHeal = false;
    public float healRange = 5f;
    public string healProjectilePoolName;

    public int defense;
    [Range(0,100)]
    public int criticalRate;
    public float criticalDamage; //배율



    [Header("적 처치시 재화드롭")]
    public int coin;//스텟강화
    public int credit;//소환재화

    [Header("다음 업그레이드 유닛 (여러 개면 /로 구분하여 랜덤 진화 있으면 추가 없으면 빈칸)")]
    public UnitDataSO[] nextUpgradeUnits; //단일 객체에서 배열[]로 변경!

    // 랜덤 진화를 처리하는 핵심 함수
    public UnitDataSO GetNextUpgradeUnit()
    {
        // 진화 트리가 아예 없으면 null 반환
        if (nextUpgradeUnits == null || nextUpgradeUnits.Length == 0) return null;

        // 진화 트리가 1개뿐이면 그것을 그대로 반환 (기존의 확정 진화)
        if (nextUpgradeUnits.Length == 1) return nextUpgradeUnits[0];

        // 2개 이상일 경우 랜덤으로 하나를 뽑아서 반환 (랜덤 분기 진화)
        return nextUpgradeUnits[Random.Range(0, nextUpgradeUnits.Length)];
    }
}
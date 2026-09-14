using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Data/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public int unitId;
    public int unitLevel;
    public string unitName;

    public string uiPoolName;
    public string battlePoolName;

    [Header("외형 정보")]
    public Sprite unitSprite;

    [Header("전투 정보")]
    public float maxHp;
    public float moveSpeed;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;


    [Header("적 처치시 재화드롭")]
    public int coin;//스텟강화
    public int credit;//소환재화

    [Header("다음 업그레이드 유닛(있으면 추가 없으면 빈칸)")]
    public UnitDataSO nextUpgradeUnit;
}
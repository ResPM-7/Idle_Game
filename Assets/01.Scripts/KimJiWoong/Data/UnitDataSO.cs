using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Data/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("풀링 정보")]
    public string uiPoolName;// 머지 보드용 UI 유닛 풀 이름
    public string battlePoolName;// 실제 전장에서 싸울 전투 유닛 풀 이름

    [Header("기본 정보")]
    public int unitId;
    public int unitLevel;
    public string unitName;
    public float maxHp;
    public float moveSpeed;

    [Header("전투 정보")]
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float attackCooldown;

    [Header("다음 업그레이드 유닛(있으면 추가 없으면 빈칸)")]
    public UnitDataSO nexUpdateUnit;
}
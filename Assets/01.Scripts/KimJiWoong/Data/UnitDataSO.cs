using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Data/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public Team team;
    public float maxHp;
    public float moveSpeed;

    [Header("전투 정보")]
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    public float aoeRadius; // 0이면 단일, 0보다 크면 광역
}
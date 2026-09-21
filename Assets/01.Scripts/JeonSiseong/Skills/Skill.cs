using UnityEngine;

public class Skill : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] protected float radius;
    [SerializeField] protected float damage;
    [SerializeField] protected float damageRatio = 0.1f;


    [Header("Target")]
    [SerializeField] protected LayerMask enemyLayer;


    protected void DamageTarget(Collider2D collision)
    {

        if ((((1 << collision.gameObject.layer) & enemyLayer.value) == 0))
        return;


        ISkillDamageable target = collision.GetComponent<ISkillDamageable>();

        if(target != null)
        {
            float partyAttack = 0f;

            if(PartyBuildManager.instance != null)
            {
                partyAttack = PartyBuildManager.instance.GetTotalAttack();
            }

            
            float finalDamage = damage + (partyAttack * damageRatio);

            target.TakeSkillDamage(finalDamage);

            Debug.Log(
                $"기본 데미지: {damage}, 파티 공격력: {partyAttack}, 배율: {damageRatio}, 최종 데미지: {finalDamage}");
        }


    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}

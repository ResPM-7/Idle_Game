using UnityEngine;

public class Skill : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] protected float radius;
    [SerializeField] protected float damage;


    [Header("Target")]
    [SerializeField] protected LayerMask enemyLayer;


    protected void DamageTarget(Collider2D collision)
    {

        if ((((1 << collision.gameObject.layer) & enemyLayer.value) == 0))
        return;


        ISkillDamageable target = collision.GetComponent<ISkillDamageable>();

        if(target != null)
        {
            target.TakeSkillDamage(damage);
        }


    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }




    void Start()
    {

    }


    void Update()
    {

    }
}

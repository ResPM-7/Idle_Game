using UnityEngine;

public class FireSkill : Skill
{

    //[Header("Fire Settings")]
    //[SerializeField] float radius = 5f;         // 스킬 범위 반지름
    //[SerializeField] float damage = 20;          // 스킬 데미지


    //[Header("Target")]
    //[SerializeField] LayerMask enemyLayer;


    //void DamageEnemy()
    //{
    //    Collider2D[] enemies =Physics2D.OverlapCircleAll(transform.position,radius,enemyLayer);

    //    foreach(Collider2D enemy in enemies)
    //    {
    //        ISkillDamageable target = enemy.GetComponent<ISkillDamageable>();

    //        if(target != null)
    //        {
    //            target.TakeSkillDamage(damage);
    //        }
    //    }
    //}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (((1 << collision.gameObject.layer) & enemyLayer.value) != 0)
        //{ 

        //    ISkillDamageable target = collision.GetComponent<ISkillDamageable>();
        //    if (target != null)
        //    {
        //        Debug.Log("파이어 데미지");
        //        target.TakeSkillDamage(damage);
        //    }
        //}

        DamageTarget(collision);



    }


    void Start()
    {
        //DamageEnemy();

        Destroy(gameObject,1f);
    }


    //private void OnDrawGizmosSelected()
    //{

    //    Gizmos.DrawWireSphere(transform.position, radius);
    //}

   
}

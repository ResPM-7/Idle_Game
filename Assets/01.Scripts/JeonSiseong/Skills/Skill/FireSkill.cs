using UnityEngine;

public class FireSkill : MonoBehaviour
{

    [Header("Fire Settings")]
    [SerializeField] float radius = 5f;         // 스킬 범위 반지름
    [SerializeField] int damage = 20;          // 스킬 데미지


    [Header("Target")]
    [SerializeField] LayerMask enemyLayer;


    void DamageEnemy()
    {
        Collider2D[] enemies =Physics2D.OverlapCircleAll(transform.position,radius,enemyLayer);

        foreach(Collider2D enemy in enemies)
        {
            ISkillDamageable target = enemy.GetComponent<ISkillDamageable>();

            if(target != null)
            {
                target.TakeSkillDamage(damage);
            }
        }
    }



    void Start()
    {
        DamageEnemy();

        Destroy(gameObject,1f);
    }


    private void OnDrawGizmosSelected()
    {

        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void Update()
    {
        
    }
}

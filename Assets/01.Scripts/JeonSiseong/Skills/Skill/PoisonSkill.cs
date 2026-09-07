using System.Collections;
using UnityEngine;

public class PoisonSkill : MonoBehaviour
{

    [Header("Poison Setting")]
    [SerializeField] float radius = 2.5f;       //  스킬 범위 반지름
    [SerializeField] int damage = 10;           // 스킬 데미지
    [SerializeField] float duration = 5f;       // 지속 시간
    [SerializeField] float damageInterval = 1f;    // 데미지 들어가는 시간간격


    [Header("Target")]
    [SerializeField] LayerMask enemyLayer;

    void DamageEnemy()
    {
        Collider2D[] enemies
            = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);


        foreach(Collider2D enemy in enemies)
        {

            ISkillDamageable target = enemy.GetComponent<ISkillDamageable>();

            if(target != null)
            {
                target.TakeSkillDamage(damage);
            }
        }








    }


    IEnumerator PoisonRoutine()
    {

        float timer = 0f;

        while (timer < duration)
        {
            DamageEnemy();

            yield return new WaitForSeconds(damageInterval);

            timer += damageInterval;
        }

        Destroy(gameObject);
    }








    void Start()
    {
        StartCoroutine(PoisonRoutine());
    }


    private void OnDrawGizmosSelected()
    {
        
        Gizmos.DrawWireSphere(transform.position, radius);
    }





    void Update()
    {
        
    }
}

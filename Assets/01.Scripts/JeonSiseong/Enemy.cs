using UnityEngine;

public class Enemy : MonoBehaviour,ISkillDamageable
{

    [SerializeField] int maxHp = 1;
    [SerializeField] int currentHp = 1;



    public void TakeDamage()
    {
        currentHp--;

        if(currentHp <= 0 )
        {
            Die();
        }
    }

    public void Die()
    {
        WaveManager.instance.EnemyKilled();
        Destroy(gameObject);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            TakeDamage();
            
        }
    }

    public void TakeSkillDamage(int damage)
    {
       currentHp -= damage;

        if(currentHp <= 0 )
        {
            Die();
        }
    }


    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}

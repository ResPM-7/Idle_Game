using UnityEngine;

public class Boss : MonoBehaviour, ISkillDamageable
{
    [SerializeField] int maxHp = 1;
    [SerializeField] int currentHp = 1;



    public void TakeDamge()
    {
        currentHp--;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        WaveManager.instance.BossKilled();
        Destroy(gameObject);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TakeDamge();

        }
    }


    public void TakeSkillDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }



}

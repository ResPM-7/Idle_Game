using UnityEngine;

public class Enemy : MonoBehaviour
{

    //[SerializeField] int maxHp = 1;
    [SerializeField] private float currentHp = 1f;

    private bool isDead = false;
    private Unit_Base_Test unitBase;

    private void Awake()
    {
        unitBase = GetComponent<Unit_Base_Test>();
    }




    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if(unitBase !=  null)
        {
            unitBase.currentHp = currentHp;
        }


        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        
        if (isDead) return;

        isDead = true;

        if(unitBase !=null)
        {
            unitBase.currentHp = 0f;
        }

        if(WaveManager.instance != null)
        {
            WaveManager.instance.EnemyKilled();
        }
        
        //Destroy(gameObject);

        ObjectPoolManager.instance.ReturnObject("Enemy", gameObject);
    }

    //public void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        TakeDamage();

    //    }
    //}

    public void TakeSkillDamage(int damage)
    {

        //if (isDead)
        //    return;

        //currentHp -= damage;

        //if (currentHp <= 0)
        //{
        //    Die();
        //}

        TakeDamage(damage);
    }

    private void OnEnable()
    {
        //  변수 초기화
        currentHp = 1f;
        isDead=false;

        
        if(unitBase != null && unitBase.myData != null)
        {
           unitBase.Init(unitBase.myData);
        }
    
        if(unitBase != null && unitBase.myData != null)
        {
            unitBase.Init(unitBase.myData);
        }

    }

}

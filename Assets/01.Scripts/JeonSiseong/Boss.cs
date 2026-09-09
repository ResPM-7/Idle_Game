using UnityEngine;

public class Boss : MonoBehaviour
{
    //[SerializeField] int maxHp = 1;
    //[SerializeField] int currentHp = 1;



    //public void TakeDamge()
    //{
    //    Debug.Log("보스 일반 데미지 받음");

    //    currentHp--;

    //    if (currentHp <= 0)
    //    {
    //        Die();
    //    }
    //}

    //public void Die()
    //{
    //    Debug.Log("보스 다이 호출");


    //    WaveManager.instance.BossKilled();
    //    Destroy(gameObject);
    //}

    //public void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        TakeDamge();

    //    }
    //}


    //public void TakeSkillDamage(int damage)
    //{
    //    Debug.Log("보스 스킬 데미지 받음");


    //    currentHp -= damage;

    //    if (currentHp <= 0)
    //    {
    //        Die();
    //    }
    //}

    private void OnDisable()
    {
       WaveManager.instance.BossKilled();
    }

}

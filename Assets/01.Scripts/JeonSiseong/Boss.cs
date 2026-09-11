using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private int maxHp = 1;

    private int currentHp = 1;
    private bool isDead;


    public void TakeDamage(int damage)
    {
        //Debug.Log("보스 일반 데미지 받음");
        if (isDead)
            return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        //Debug.Log("보스 다이 호출");
        if (isDead)
            return;

        isDead = true;

        if (WaveManager.instance != null)
        {
            WaveManager.instance.BossKilled();
        }
        //Destroy(gameObject);

        if (ObjectPoolManager.instance != null)
        {
            ObjectPoolManager.instance.ReturnObject("Boss", gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }


    }

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
        //WaveManager.instance.BossKilled();
        currentHp = maxHp;
        isDead = false;
    }


    //private void OnDestroy()
    //{
    //    // 오브젝트가 파괴되는 순간, 자신을 파괴한 실행 호출 스택(StackTrace)을 에러 창에 출력합니다.
    //    Debug.LogError($"[보스 파괴 범인 찾기] {gameObject.name}이(가) 파괴되었습니다!\n" + System.Environment.StackTrace);
    //}
}

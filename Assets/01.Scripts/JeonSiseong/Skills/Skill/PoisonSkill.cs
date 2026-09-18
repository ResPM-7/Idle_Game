using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSkill : Skill
{

    [Header("Poison Setting")]
    [SerializeField] float duration = 5f;       // 지속 시간
    [SerializeField] float damageInterval = 1f;    // 데미지 들어가는 시간간격

    private float tickTimer = 0f;
    private List<Collider2D> targetsInRange = new List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!targetsInRange.Contains(collision))
            targetsInRange.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        targetsInRange.Remove(collision);
    }


    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    tickTimer += Time.fixedDeltaTime;

    //    if (tickTimer >= damageInterval)
    //    {
    //        tickTimer = 0f;
    //        DamageTarget(collision);
    //    }

    //}


    //IEnumerator PoisonRoutine()
    //{

    //    float timer = 0f;

    //    while (timer < duration)
    //    {
    //        yield return new WaitForSeconds(damageInterval);

    //        timer += damageInterval;
    //    }

    //    Destroy(gameObject);
    //}

    void Start()
    {
        //StartCoroutine(PoisonRoutine());
        Destroy(gameObject, duration);

    }

    void Update()
    {
        tickTimer += Time.deltaTime;

        if(tickTimer >= damageInterval)
        {
            tickTimer = 0f;

            for(int i = targetsInRange.Count - 1; i >= 0;i--)
            {
                if (targetsInRange[i] == null)
                {
                    targetsInRange.RemoveAt(i);
                    continue;
                }

                DamageTarget(targetsInRange[i]);
            }
        }
    }
}

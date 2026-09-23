using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSkill : Skill, IPoolable
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
 
    void Update()
    {
        tickTimer += Time.deltaTime;

        if(tickTimer >= damageInterval)
        {
            tickTimer = 0f;

            for(int i = targetsInRange.Count - 1; i >= 0;i--)
            {
                if (targetsInRange[i] == null || !targetsInRange[i].gameObject.activeInHierarchy)
                {
                    targetsInRange.RemoveAt(i);
                    continue;
                }

                DamageTarget(targetsInRange[i]);
            }
        }
    }

    public void OnSpawned()
    {
        CancelInvoke();
        tickTimer = 0f;
        targetsInRange.Clear();
        Invoke(nameof(ReturnToPool), duration);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(gameObject);
    }
    public void OnDespawned()
    {
        CancelInvoke();
        tickTimer = 0f;
        targetsInRange.Clear();
    }
}

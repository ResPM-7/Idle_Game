using System.Collections;
using UnityEngine;

public class FireSkill : Skill
{
    [SerializeField] private string poolName = "Fire";
    [SerializeField] private float duration = 1f;

    private void OnEnable()
    {
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(duration);

        if (ObjectPoolManager.instance != null)
        {
            ObjectPoolManager.instance.ReturnObject(poolName, gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageTarget(collision);
    }
}
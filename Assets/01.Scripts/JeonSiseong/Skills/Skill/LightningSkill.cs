using System.Collections;
using UnityEngine;

public class LightningSkill : Skill
{
    [SerializeField] private string poolName = "Lightning";
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
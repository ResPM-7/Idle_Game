using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private string poisonPoolName = "Poison"; // 이미지에 적힌 오타 그대로 맞춤
    [SerializeField] private string lightningPoolName = "Lightning";
    [SerializeField] private string firePoolName = "Fire";
    [SerializeField] private string attackBuffPoolName = "AttackBuff";
    [SerializeField] private string freezePoolName = "Freeze";


    [Header("Skill Cooltime")]                          // 각 스킬 쿨타임
    [SerializeField] float poisonCooltime = 8f;
    [SerializeField] float lightningCooltime = 5f;
    [SerializeField] float fireCooltime = 10f;
    [SerializeField] float attackBuffCooltime = 15f;
    [SerializeField] float freezeCooltime = 12f;

    [Header("스킬 위치")]
    [SerializeField] private Transform skillSpawnPoint;

    [Header("스킬 쿨타임 이미지 확인")]
    [SerializeField] private Image poisonCooldownImage;
    [SerializeField] private Image lightningCooldownImage;
    [SerializeField] private Image fireCooldownImage;
    [SerializeField] private Image attackBuffCooldownImage;
    [SerializeField] private Image freezeCooldownImage;


    float poisonTimer;
    float lightningTimer;
    float fireTimer;
    float attackBuffTimer;
    float freezeTimer;

    

    public void OnClickPoisonSkill()
    {
        if (skillSpawnPoint != null) UsePoison(skillSpawnPoint.position);
    }

    public void OnClickLightningSkill()
    {
        if (skillSpawnPoint != null) UseLightning(skillSpawnPoint.position);
    }

    public void OnClickFireSkill()
    {
        if (skillSpawnPoint != null) UseFire(skillSpawnPoint.position);
    }

    public void OnClickAttackBuffSkill()
    {
        if(skillSpawnPoint != null) UseAttackBuff(skillSpawnPoint.position);
    }

    public void OnClickFreezeSkill()
    {
        if(skillSpawnPoint != null) UseFreeze(skillSpawnPoint.position);
    }

    public void UseFreeze(Vector3 position)
    {
        if(freezeTimer > 0) return;

        GameObject obj = ObjectPoolManager.instance.Spawn(freezePoolName, position, Quaternion.identity);
        if(obj != null)
        {
            freezeTimer = freezeCooltime;
        }
    }
    public void UseAttackBuff(Vector3 position)
    {
        if(attackBuffTimer > 0)
        {
            return;
        }

        GameObject obj = ObjectPoolManager.instance.Spawn(attackBuffPoolName, position, Quaternion.identity);
        if(obj != null)
        {
            attackBuffTimer = attackBuffCooltime;
        }
    }

    public void UsePoison(Vector3 position)       // 플레이어가 호출 할 독 스킬
    {
        if (poisonTimer > 0)
        {
            return;
        }

        GameObject obj = ObjectPoolManager.instance.Spawn(poisonPoolName, position, Quaternion.identity);
        if (obj != null)
        {
            poisonTimer = poisonCooltime;
        }
    }

    public void UseLightning(Vector3 position)       // 플레이어가 호출 할 번개 스킬
    {
        if(lightningTimer > 0)
        {
            return;
        }

        GameObject obj = ObjectPoolManager.instance.Spawn(lightningPoolName, position, Quaternion.identity);
        if (obj != null)
        {
            lightningTimer = lightningCooltime;
        }
    }




    public void UseFire(Vector3 position)       // 플레이어가 호출 할 화염 스킬
    {
        if(fireTimer > 0)
        {
            return;
        }

        GameObject obj = ObjectPoolManager.instance.Spawn(firePoolName, position, Quaternion.identity);
        if (obj != null)
        {
            fireTimer = fireCooltime;
        }
    }

    private void Update()
    {
        if (poisonTimer > 0)
        {
            poisonTimer -= Time.deltaTime;
            if (poisonCooldownImage != null)
                poisonCooldownImage.fillAmount = poisonTimer / poisonCooltime; // 남은 비율 계산
        }

        if (lightningTimer > 0)
        {
            lightningTimer -= Time.deltaTime;
            if (lightningCooldownImage != null)
                lightningCooldownImage.fillAmount = lightningTimer / lightningCooltime;
        }

        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
            if (fireCooldownImage != null)
                fireCooldownImage.fillAmount = fireTimer / fireCooltime;
        }

        if(attackBuffTimer > 0)
        {
            attackBuffTimer -= Time.deltaTime;
            if(attackBuffCooldownImage != null)
                attackBuffCooldownImage.fillAmount = attackBuffTimer / attackBuffCooltime;
        }

        if(freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            if(freezeCooldownImage != null)
                freezeCooldownImage.fillAmount = freezeTimer / freezeCooltime;
        }
    }
}

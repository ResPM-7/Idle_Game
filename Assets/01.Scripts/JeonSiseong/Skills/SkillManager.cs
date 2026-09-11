using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{

    [Header("Skill Prefabs")]                           // 스킬 프리팹
    [SerializeField] GameObject poisonSkillPrefab;
    [SerializeField] GameObject lightningSkillPrefab;
    [SerializeField] GameObject fireSkillPrefab;

    [SerializeField] private string poisonPoolName = "Posion"; // 이미지에 적힌 오타 그대로 맞춤
    [SerializeField] private string lightningPoolName = "Lightning";
    [SerializeField] private string firePoolName = "Fire";


    [Header("Skill Cooltime")]                          // 각 스킬 쿨타임
    [SerializeField] float poisonCooltime = 8f;
    [SerializeField] float lightningCooltime = 5f;
    [SerializeField] float fireCooltime = 10f;

    [Header("스킬 위치")]
    [SerializeField] private Transform skillSpawnPoint;

    [Header("스킬 쿨타임 이미지 확인")]
    [SerializeField] private Image poisonCooldownImage;
    [SerializeField] private Image lightningCooldownImage;
    [SerializeField] private Image fireCooldownImage;


    float poisonTimer;
    float lightningTimer;
    float fireTimer;

    

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

    public void UsePoison(Vector3 position)       // 플레이어가 호출 할 독 스킬
    {
        if (poisonTimer > 0)
        {
            return;
        }

        poisonTimer = poisonCooltime;

        GameObject obj = ObjectPoolManager.instance.GetObject(poisonPoolName);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.identity;
        }
    }

    public void UseLightning(Vector3 position)       // 플레이어가 호출 할 번개 스킬
    {
        if(lightningTimer > 0)
        {
            return;
        }

        lightningTimer = lightningCooltime;

        GameObject obj = ObjectPoolManager.instance.GetObject(lightningPoolName);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.identity;
        }
    }




    public void UseFire(Vector3 position)       // 플레이어가 호출 할 화염 스킬
    {
        if(fireTimer > 0)
        {
            return;
        }

        fireTimer = fireCooltime;

        GameObject obj = ObjectPoolManager.instance.GetObject(firePoolName);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.identity;
        }
    }






    void Start()
    {
        
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
    }
}

using UnityEngine;

public class SkillManager : MonoBehaviour
{

    [Header("Skill Prefabs")]                           // 스킬 프리팹
    [SerializeField] GameObject poisonSkillPrefab;
    [SerializeField] GameObject lightningSkillPrefab;
    [SerializeField] GameObject fireSkillPrefab;


    [Header("Skill Cooltime")]                          // 각 스킬 쿨타임
    [SerializeField] float poisonCooltime = 8f;
    [SerializeField] float lightningCooltime = 5f;
    [SerializeField] float fireCooltime = 10f;


    float poisonTimer;
    float lightningTimer;
    float fireTimer;

    public void UsePoison(Vector3 position)       // 플레이어가 호출 할 독 스킬
    {
        if(poisonTimer > 0)
        {
            return;
        }

        poisonTimer = poisonCooltime;


        GameObject obj=
        Instantiate(poisonSkillPrefab, position, Quaternion.identity);
    }



    public void UseLightning(Vector3 position)       // 플레이어가 호출 할 번개 스킬
    {
        if(lightningTimer > 0)
        {
            return;
        }

        lightningTimer = lightningCooltime;


        GameObject obj =
        Instantiate(lightningSkillPrefab, position, Quaternion.identity);
    }




    public void UseFire(Vector3 position)       // 플레이어가 호출 할 화염 스킬
    {
        if(fireTimer > 0)
        {
            return;
        }

        fireTimer = fireCooltime;


        GameObject obj =
        Instantiate(fireSkillPrefab, position, Quaternion.identity);
    }






    void Start()
    {
        
    }

    
    void Update()
    {
        poisonTimer -= Time.deltaTime;
        lightningTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;
    }
}

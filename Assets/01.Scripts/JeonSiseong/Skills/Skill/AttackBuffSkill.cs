using UnityEngine;

public class AttackBuffSkill : MonoBehaviour
{
    [Header("Buff Setting")]
    [SerializeField] private float buffMultiplier = 1.5f;  //  50% 증가
    [SerializeField] private float buffDuration = 5f;     // 버프 지속 시간

    void Start()
    {
        if(PartyBuildManager.instance != null)
        {
            PartyBuildManager.instance.ApplyUnitDamageBuff(buffMultiplier, buffDuration);
        }

    }

    
    void Update()
    {
        
    }
}

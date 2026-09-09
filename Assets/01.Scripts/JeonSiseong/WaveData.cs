using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Data/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("기본 웨이브 당 처치 목표 (1~5 웨이브 순서)")]
    public int[] baseKillCountPerWave = { 3, 3, 4, 4, 5 };


    [Header("스테이지 성장")]
    public int stageGrowthInterval = 5;     // 몇 스테이지마다 증가 시킬지
    public int killCountGrowthPerInterval = 1; // 그때마다 몇 마리씩 늘릴지

    [Header("보스")]
    public float baseBossTimeLimit = 30f;


    [Header("스폰")]
    public float spawnInterval = 1.2f;
    public int maxAliveCount = 6;







}

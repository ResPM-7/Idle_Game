using UnityEngine;
using TMPro;

public class UI_WaveHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text waveCountText;
    [SerializeField] private TMP_Text bossTimerText;
    private void Awake()
    {
        //자식 오브젝트에서 TMP_Text 컴포넌트 탐색
        if (waveCountText == null)
        {
            Transform waveObj = transform.Find("WaveCountText");
            if (waveObj != null) waveCountText = waveObj.GetComponent<TMP_Text>();
        }

        if (bossTimerText == null)
        {
            Transform timerObj = transform.Find("BossTimerText");
            if (timerObj != null) bossTimerText = timerObj.GetComponent<TMP_Text>();
        }
    }
    private void Start()
    {
        // WaveManager 바인딩
        BindToWaveManager();
    }
    private void BindToWaveManager()
    {
        if (WaveManager.instance == null) return;

        //WaveManager의 [SerializeField] private 필드에 자동 할당
        var type = typeof(WaveManager);

        var waveField = type.GetField("waveCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var timerField = type.GetField("bossTimerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (waveField != null && waveCountText != null)
        {
            waveField.SetValue(WaveManager.instance, waveCountText);
        }

        if (timerField != null && bossTimerText != null)
        {
            timerField.SetValue(WaveManager.instance, bossTimerText);
            bossTimerText.gameObject.SetActive(false); // 초기 상태 비활성화
        }
    }
}
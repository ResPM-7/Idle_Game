using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private string soundId;

    [Tooltip("효과음 중복재생 방지 딜레이(sec)")]
    [SerializeField, Min(0f)] private float minimumInterval;

    private float lastPlayedTime = float.NegativeInfinity;

    public void Play()
    {
        if (Time.unscaledTime - lastPlayedTime < minimumInterval)
            return;

        if (SoundManager.instance == null)
        {
            Debug.LogWarning(
                "[SoundEmitter] SoundManager를 찾지 못했습니다.",
                this
            );
            return;
        }

        lastPlayedTime = Time.unscaledTime;
        SoundManager.instance.PlaySfx(soundId);
    }
}
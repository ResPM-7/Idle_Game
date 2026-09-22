using UnityEngine;

public class SceneBgmPlayer : MonoBehaviour
{
    [SerializeField] private string bgmId;

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(bgmId))
            return;

        if (SoundManager.instance == null)
        {
            Debug.LogWarning(
                "[SceneBgmPlayer] SoundManager를 찾지 못했습니다.",
                this
            );
            return;
        }

        SoundManager.instance.PlayBgm(bgmId);
    }
}
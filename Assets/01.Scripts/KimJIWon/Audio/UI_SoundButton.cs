using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UI_SoundButton : MonoBehaviour
{
    [SerializeField] private string soundId = "ui_click";

    private Button targetButton;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
        targetButton.onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (SoundManager.instance == null)
        {
            Debug.LogWarning(
                "[UI_SoundButton] SoundManager를 찾지 못했습니다.",
                this
            );
            return;
        }

        SoundManager.instance.PlaySfx(soundId);
    }

    private void OnDestroy()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(PlayClickSound);
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_AudioSettings : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("Mute")]
    [SerializeField] private Toggle muteToggle;

    private void Awake()
    {
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        muteToggle.onValueChanged.AddListener(OnMuteChanged);
    }

    private void OnEnable()
    {
        RefreshFromSoundManager();
    }

    private void RefreshFromSoundManager()
    {
        SoundManager manager = SoundManager.instance;

        if (manager == null)
        {
            Debug.LogWarning("SoundManager를 찾지못함",this);
            return;
        }

        float masterValue = manager.MasterVolume * 100f;
        float bgmValue = manager.BgmVolume * 100f;
        float sfxValue = manager.SfxVolume * 100f;

        masterSlider.SetValueWithoutNotify(masterValue);
        bgmSlider.SetValueWithoutNotify(bgmValue);
        sfxSlider.SetValueWithoutNotify(sfxValue);
        muteToggle.SetIsOnWithoutNotify(manager.IsMuted);

        UpdateValueText(masterValueText, masterValue);
        UpdateValueText(bgmValueText, bgmValue);
        UpdateValueText(sfxValueText, sfxValue);
    }

    private void OnMasterVolumeChanged(float value)
    {
        UpdateValueText(masterValueText, value);

        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetMasterVolume(value / 100f);
        }
    }

    private void OnBgmVolumeChanged(float value)
    {
        UpdateValueText(bgmValueText, value);

        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetBgmVolume(value / 100f);
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        UpdateValueText(sfxValueText, value);

        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetSfxVolume(value / 100f);
        }
    }

    private void OnMuteChanged(bool isMuted)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetMuted(isMuted);
        }
    }

    private void UpdateValueText(TMP_Text targetText, float value)
    {
        if (targetText == null)
            return;

        targetText.text = Mathf.RoundToInt(value).ToString();
    }

    private void OnDisable()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SaveSettings();
        }
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        muteToggle.onValueChanged.RemoveListener(OnMuteChanged);
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    private const string MasterKey = "Audio.Master";
    private const string BgmKey = "Audio.BGM";
    private const string SfxKey = "Audio.SFX";
    private const string MutedKey = "Audio.Muted";

    private const string MasterParameter = "MasterVolume";
    private const string BgmParameter = "BGMVolume";
    private const string SfxParameter = "SFXVolume";

    private const float MinimumDecibel = -80f;

    [Header("Audio Data")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup gameplayGroup;

    [Header("SFX Pool")]
    [SerializeField, Min(1)] private int sfxPoolSize = 12;

    private AudioSource bgmSource;
    private readonly List<AudioSource> sfxSources = new();
    private int nextSfxIndex;

    public float MasterVolume { get; private set; } = 0.5f;
    public float BgmVolume { get; private set; } = 0.5f;
    public float SfxVolume { get; private set; } = 0.5f;
    public bool IsMuted { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // 중복 SoundManager라면 초기화하지 않는다.
        if (instance != this)
            return;

        DontDestroyOnLoad(gameObject);

        LoadSettings();
        CreateAudioSources();
    }

    private void Start()
    {
        ApplyAllVolumes();
    }

    private void CreateAudioSources()
    {
        CreateBgmSource();
        CreateSfxPool();
    }

    private void CreateBgmSource()
    {
        GameObject sourceObject = new GameObject("BGM_Source");
        sourceObject.transform.SetParent(transform);

        bgmSource = sourceObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.outputAudioMixerGroup = bgmGroup;
    }

    private void CreateSfxPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sourceObject = new GameObject($"SFX_Source_{i:00}");
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;

            sfxSources.Add(source);
        }
    }

    public void PlayBgm(string id)
    {
        if (!TryGetSound(id, out SoundEntry sound))
            return;

        if (sound.Category != SoundCategory.BGM)
        {
            Debug.LogWarning(
                $"[SoundManager] BGM이 아닌 사운드입니다: {id}",
                this
            );
            return;
        }

        if (bgmSource.clip == sound.Clip && bgmSource.isPlaying)
            return;

        bgmSource.Stop();
        bgmSource.clip = sound.Clip;
        bgmSource.loop = sound.Loop;
        bgmSource.volume = sound.Volume;
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource == null)
            return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySfx(string id)
    {
        if (!TryGetSound(id, out SoundEntry sound))
            return;

        if (sound.Category == SoundCategory.BGM)
        {
            Debug.LogWarning(
                $"[SoundManager] SFX로 재생할 수 없는 BGM입니다: {id}",
                this
            );
            return;
        }

        AudioSource source = GetAvailableSfxSource();

        source.Stop();
        source.clip = sound.Clip;
        source.loop = false;
        source.volume = sound.Volume;
        source.pitch = 1f;

        source.outputAudioMixerGroup =
            sound.Category == SoundCategory.UI
                ? uiGroup
                : gameplayGroup;

        source.Play();
    }

    private AudioSource GetAvailableSfxSource()
    {
        foreach (AudioSource source in sfxSources)
        {
            if (!source.isPlaying)
                return source;
        }

        // 전부 재생 중이면 가장 오래된 순서대로 재사용한다.
        AudioSource reusedSource = sfxSources[nextSfxIndex];

        nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Count;

        return reusedSource;
    }

    private bool TryGetSound(string id, out SoundEntry sound)
    {
        sound = null;

        if (soundLibrary == null)
        {
            Debug.LogError(
                "[SoundManager] SoundLibrary가 할당되지 않았습니다.",
                this
            );
            return false;
        }

        if (!soundLibrary.TryGet(id, out sound))
        {
            Debug.LogWarning(
                $"[SoundManager] 등록되지 않은 Sound ID입니다: {id}",
                this
            );
            return false;
        }

        if (sound.Clip == null)
        {
            Debug.LogWarning(
                $"[SoundManager] AudioClip이 비어 있습니다: {id}",
                this
            );
            return false;
        }

        return true;
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        ApplyMasterVolume();
    }

    public void SetBgmVolume(float value)
    {
        BgmVolume = Mathf.Clamp01(value);
        SetMixerVolume(BgmParameter, BgmVolume);
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        SetMixerVolume(SfxParameter, SfxVolume);
    }

    public void SetMuted(bool muted)
    {
        IsMuted = muted;
        ApplyMasterVolume();
    }

    private void ApplyAllVolumes()
    {
        ApplyMasterVolume();
        SetMixerVolume(BgmParameter, BgmVolume);
        SetMixerVolume(SfxParameter, SfxVolume);
    }

    private void ApplyMasterVolume()
    {
        if (IsMuted)
        {
            audioMixer.SetFloat(MasterParameter, MinimumDecibel);
            return;
        }

        SetMixerVolume(MasterParameter, MasterVolume);
    }

    private void SetMixerVolume(string parameterName, float normalizedValue)
    {
        if (audioMixer == null)
        {
            Debug.LogError(
                "[SoundManager] AudioMixer가 할당되지 않았습니다.",
                this
            );
            return;
        }

        float decibel = normalizedValue <= 0.0001f
            ? MinimumDecibel
            : Mathf.Log10(normalizedValue) * 20f;

        if (!audioMixer.SetFloat(parameterName, decibel))
        {
            Debug.LogWarning(
                $"[SoundManager] 노출 파라미터를 찾지 못했습니다: {parameterName}",
                this
            );
        }
    }

    private void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterKey, 0.5f);
        BgmVolume = PlayerPrefs.GetFloat(BgmKey, 0.5f);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, 0.5f);
        IsMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        PlayerPrefs.SetFloat(BgmKey, BgmVolume);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        PlayerPrefs.SetInt(MutedKey, IsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveSettings();
    }

    protected override void OnApplicationQuit()
    {
        SaveSettings();
        base.OnApplicationQuit();
    }
}
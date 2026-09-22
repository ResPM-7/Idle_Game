using System;
using System.Collections.Generic;
using UnityEngine;

public enum SoundCategory
{
    BGM,
    UI,
    Gameplay
}

[Serializable]
public class SoundEntry
{
    [SerializeField] private string id;
    [SerializeField] private AudioClip clip;
    [SerializeField] private SoundCategory category;

    [SerializeField] private bool loop;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    public string Id => id;
    public AudioClip Clip => clip;
    public SoundCategory Category => category;
    public bool Loop => loop;
    public float Volume => volume;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]

public class SoundLibrary : ScriptableObject
{
    [SerializeField] private List<SoundEntry> sounds = new();

    private Dictionary<string, SoundEntry> lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }
#endif

    private void BuildLookup()
    {
        lookup = new Dictionary<string, SoundEntry>();

        foreach (SoundEntry sound in sounds)
        {
            if (sound == null)
                continue;

            string id = sound.Id?.Trim();

            if (string.IsNullOrEmpty(id))
                continue;

            if (lookup.ContainsKey(id))
            {
                Debug.LogWarning(
                    $"[SoundLibrary] 중복된 Sound ID입니다: {id}",
                    this
                );
                continue;
            }

            lookup.Add(id, sound);
        }
    }

    public bool TryGet(string id, out SoundEntry sound)
    {
        if (lookup == null)
            BuildLookup();

        if (string.IsNullOrWhiteSpace(id))
        {
            sound = null;
            return false;
        }

        return lookup.TryGetValue(id.Trim(), out sound);
    }
}
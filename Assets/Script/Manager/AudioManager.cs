using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("SFX")]
    public GameObject sfxPrefab;
    public List<Sound> sounds;
    [Header("Volume")]
    public float sfxVolume = 1f; // Mặc định max
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        PlayMusic(backgroundMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource && clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource) musicSource.Stop();
    }

    public void PlaySFX(string name, Vector3 position = default)
    {
        Sound sound = sounds.Find(s => s.name == name);
        if (sound != null && sfxPrefab != null)
        {
            GameObject sfxObj = Instantiate(sfxPrefab, position, Quaternion.identity);
            AudioSource audio = sfxObj.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.clip = sound.clip;
                audio.Play();
                Destroy(sfxObj, sound.clip.length);
            }
        }
        else
        {
            Debug.LogWarning("SFX or sound not found: " + name);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource) musicSource.volume = Mathf.Clamp01(volume);
    }
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}

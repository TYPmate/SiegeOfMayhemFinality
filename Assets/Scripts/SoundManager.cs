using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource _musicSource, _effectSource;
    public List<AudioClip> effects = new List<AudioClip>();
    public List<AudioClip> songs = new List<AudioClip>();
    public float currentMusicLength;
    public bool isInBattle = false;
    public bool isPlayingMusic = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (isInBattle)
        {
            AutomaticMusicPlayer();
        }
    }

    public void AutomaticMusicPlayer()
    {
        if (!isPlayingMusic)
        {
            int i;
            i = Random.Range(0, 3);
            PlayMusic(songs[i]);
            StartCoroutine(MusicDelay());
        }
    }

    IEnumerator MusicDelay()
    {
        isPlayingMusic = true;
        yield return new WaitForSeconds(currentMusicLength);
        isPlayingMusic = false;
    }



    public void DifferentEffectPitch(float lowValue, float highValue)
    {
        _effectSource.pitch = Random.Range(lowValue, highValue);
    }

    public void ResetEffectPitch()
    {
        _effectSource.pitch = 1.0f;
    }

    public void PlaySound(AudioClip clip)
    {
        _effectSource.PlayOneShot(clip);
    }

    public void StopSound(AudioClip clip)
    {
        _effectSource.Stop();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        isPlayingMusic = false;
        currentMusicLength = 0;
    }

    public void PlayMusic(AudioClip song)
    {
        if (!isPlayingMusic)
        {
            _musicSource.PlayOneShot(song);
            currentMusicLength = song.length;
        }
    }

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void ChangeMusicVolume(float value)
    {
        _musicSource.volume = value;
    }

    public void ChangeEffectVolume(float value)
    {
        _effectSource.volume = value;
    }
}
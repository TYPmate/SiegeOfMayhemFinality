using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all audio playback including sound effects and music
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource _musicSource, _effectSource;
    public List<AudioClip> effects = new List<AudioClip>();
    public List<AudioClip> songs = new List<AudioClip>();
    public float currentMusicLength;
    public bool isInBattle = false;
    public bool isPlayingMusic = false;

    /// <summary>
    /// Initializes the singleton instance and sets up persistence
    /// </summary>
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

    /// <summary>
    /// Update is called once per frame to handle automatic music playback
    /// </summary>
    void Update()
    {
        if (isInBattle)
        {
            AutomaticMusicPlayer();
        }
    }

    /// <summary>
    /// Automatically plays random battle music when in combat
    /// </summary>
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

    /// <summary>
    /// Coroutine to handle music playback delay between tracks
    /// </summary>
    IEnumerator MusicDelay()
    {
        isPlayingMusic = true;
        yield return new WaitForSeconds(currentMusicLength);
        isPlayingMusic = false;
    }

    /// <summary>
    /// Sets random pitch for sound effects within specified range
    /// </summary>
    /// <param name="lowValue">Minimum pitch value</param>
    /// <param name="highValue">Maximum pitch value</param>
    public void DifferentEffectPitch(float lowValue, float highValue)
    {
        _effectSource.pitch = Random.Range(lowValue, highValue);
    }

    /// <summary>
    /// Resets sound effect pitch to default value
    /// </summary>
    public void ResetEffectPitch()
    {
        _effectSource.pitch = 1.0f;
    }

    /// <summary>
    /// Plays a single sound effect
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    public void PlaySound(AudioClip clip)
    {
        _effectSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Stops playing the current sound effect
    /// </summary>
    /// <param name="clip">The audio clip to stop</param>
    public void StopSound(AudioClip clip)
    {
        _effectSource.Stop();
    }

    /// <summary>
    /// Stops the currently playing music
    /// </summary>
    public void StopMusic()
    {
        _musicSource.Stop();
        isPlayingMusic = false;
        currentMusicLength = 0;
    }

    /// <summary>
    /// Plays a music track
    /// </summary>
    /// <param name="song">The audio clip to play as music</param>
    public void PlayMusic(AudioClip song)
    {
        if (!isPlayingMusic)
        {
            _musicSource.PlayOneShot(song);
            currentMusicLength = song.length;
        }
    }

    /// <summary>
    /// Changes the master volume for all audio
    /// </summary>
    /// <param name="value">The volume value between 0 and 1</param>
    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    /// <summary>
    /// Changes the volume for music audio
    /// </summary>
    /// <param name="value">The volume value between 0 and 1</param>
    public void ChangeMusicVolume(float value)
    {
        _musicSource.volume = value;
    }

    /// <summary>
    /// Changes the volume for sound effects
    /// </summary>
    /// <param name="value">The volume value between 0 and 1</param>
    public void ChangeEffectVolume(float value)
    {
        _effectSource.volume = value;
    }
}
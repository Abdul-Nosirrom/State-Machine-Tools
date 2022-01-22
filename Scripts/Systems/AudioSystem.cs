using System;
using UnityEngine;

/// <summary>
/// Super basic audio system which supports 3D sound.
/// Ensure you change the 'Sounds' audio source to use 3D spatial blend if you intend on using 3D sounds
/// Author : Tarodev
/// </summary>
public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _soundsSource;

    public void PlayMusic(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void PlaySound(AudioClip clip, Vector3 pos, float vol = 1)
    {
        _soundsSource.transform.position = pos;
        PlaySound(clip, vol);
    }

    public void PlaySound(AudioClip clip, float vol = 1)
    {
        _soundsSource.PlayOneShot(clip, vol);
    }

    /*
     * For more information on the change volume methods and how to further implement them
     * watch the Tarodev video on "Manage your sounds in Unity"
     */
    public void ChangeMasterVolume(float vol)
    {
        AudioListener.volume = vol;
    }

    public void ChangeMusicVolume(float vol)
    {
        _musicSource.volume = vol;
    }

    public void ChangeEffectsVolume(float vol)
    {
        _soundsSource.volume = vol;
    }
}
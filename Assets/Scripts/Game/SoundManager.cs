using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SoundManager
{
    public static void PlaySound(Sounds sound)
    {
        var gameObject = new GameObject("Sound", typeof(AudioSource));
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.PlayOneShot(GetAudioClip(sound));
    }

    private static AudioClip GetAudioClip(Sounds sound)
        => GameAssets.GetInstance().SoundAudioClips.FirstOrDefault(audio => audio.Sound == sound)?.AudioClip;

    public static void AddButtonSound(this Button_UI button)
    {
        button.MouseOutOnceFunc += () => PlaySound(Sounds.ButtonOver);
        button.ClickFunc += () => PlaySound(Sounds.ButtonClick);
    }
}

[Serializable]
public class SoundAudioClip
{
    public Sounds Sound;
    public AudioClip AudioClip;
}

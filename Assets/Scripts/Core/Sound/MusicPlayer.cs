using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {
    public AudioClip Menu;
    public List<AudioClip> Levels;
    public AudioClip BossTheme;
    public AudioClip PlayerDeath;
    public AudioClip Credits;

    AudioSource _audiosource1;
    AudioSource _audiosource2;

    void Awake () {
        //SetUpSingleton ();
    }
    void Start () {
        var audioSources = GetComponents<AudioSource> ();
        _audiosource1 = audioSources[0];
        _audiosource2 = audioSources[1];

        //PlayMenuTheme ();
    }
    void SetUpSingleton () {
        if (FindObjectsOfType (GetType ()).Length > 1) {
            Destroy (gameObject);
        } else {
            // DontDestroyOnLoad (gameObject);
        }
    }

    public void PlayMenuTheme () {
        StartCoroutine (ChangeSong (Menu, 0f));
    }
    public void ChangeLevelSong (int levelSongIndex, bool instantPlay = false) {
        StartCoroutine (ChangeSong (Levels[levelSongIndex], 5f, false, instantPlay));
    }
    public void PlayPlayerDeath () {
        StartCoroutine (ChangeSong (PlayerDeath, 0.5f));
    }
    public void PlayBossTheme () {
        StartCoroutine (ChangeSong (BossTheme, 0.75f));
    }
    public void PlayCredits () {
        StartCoroutine (ChangeSong (Credits, 3f));
    }

    public void FadeIn (float duration) {
        var audiosource = _audiosource1.isPlaying ? _audiosource1 : _audiosource2;
        StartCoroutine (FadeSource (audiosource, audiosource.volume, 0f, duration));
    }
    public void FadeOut (float duration) {
        var audiosource = _audiosource1.isPlaying ? _audiosource1 : _audiosource2;
        StartCoroutine (FadeSource (audiosource, audiosource.volume, 1f, duration));
    }

    IEnumerator ChangeSong (AudioClip song, float duration, bool crossFading = false, bool instantPlay = false) {
        while (_audiosource1 == null || _audiosource2 == null) yield return 0;

        AudioSource curAudioSource;
        AudioSource newAudioSource;

        if (_audiosource1.isPlaying) {
            curAudioSource = _audiosource1;
            newAudioSource = _audiosource2;
        } else {
            curAudioSource = _audiosource2;
            newAudioSource = _audiosource1;
        }

        newAudioSource.clip = song;
        newAudioSource.Play ();

        if (!instantPlay)
            newAudioSource.volume = 0f;

        if (crossFading) {
            StartCoroutine (FadeSource (curAudioSource, curAudioSource.volume, 0, duration));
            StartCoroutine (FadeSource (newAudioSource, newAudioSource.volume, 1, duration));

            // for (float i = 1; i >= 0; i -= Time.deltaTime / duration) {
            //     curAudioSource.volume = Mathf.Lerp (0, 1, i * -1);
            //     newAudioSource.volume = Mathf.Lerp (0, 1, i);
            //     yield return null;
            // }
        } else {
            yield return StartCoroutine (FadeSource (curAudioSource, curAudioSource.volume, 0, duration));
            yield return StartCoroutine (FadeSource (newAudioSource, newAudioSource.volume, 1, duration));
            // for (float i = 1; i >= 0; i -= Time.deltaTime / duration) {
            //     curAudioSource.volume = Mathf.Lerp (0, 1, i);
            //     yield return null;
            // }
            // for (float i = 0; i <= 1; i += Time.deltaTime / duration) {
            //     newAudioSource.volume = Mathf.Lerp (0, 1, i);
            //     yield return null;
            // }
        }

        curAudioSource.Stop ();
    }

    IEnumerator FadeSource (AudioSource sourceToFade, float startVolume, float endVolume, float duration) {
        float startTime = Time.time;

        while (true) {
            float elapsed = Time.time - startTime;

            sourceToFade.volume = Mathf.Clamp01 (Mathf.Lerp (startVolume, endVolume, elapsed / duration));

            if (sourceToFade.volume == endVolume) {
                break;
            }

            yield return null;
        }
    }
}
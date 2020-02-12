using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {
    public AudioClip Menu_Sound;
    GameObject _blackScreen;
    MusicPlayer _musicPlayer;

    void Start () {
        _blackScreen = GameObject.Find ("BlackScreen");
        _musicPlayer = GameObject.Find("MusicPlayer").GetComponent<MusicPlayer>();
    }

    public void LoadStartMenu () {
        SceneManager.LoadScene (0);
    }
    public void LoadGameScene () {
        AudioSource.PlayClipAtPoint (Menu_Sound, Camera.main.transform.position, 1f);
        StartCoroutine (ChangeToScene ("Game"));
    }

    public void LoadGameOver () {
        StartCoroutine (ChangeToScene ("GameOver"));
    }

    IEnumerator ChangeToScene (string scene) {
        _blackScreen.SetActive (true);

        yield return _blackScreen.GetComponent<FadeController> ().FadeOut (1);

        SceneManager.LoadScene (scene);
    }
    public void QuitGame () {
        Application.Quit ();
    }
}
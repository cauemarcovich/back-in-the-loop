using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageController : MonoBehaviour {
    UIController _uiController;
    MusicPlayer _musicPlayer;
    BackgroundTransition _backgroundTransition;
    FadeController _blackScreenFadeController;
    GameProgressionHUD _gameProgressionHUD;
    GameObject _pause;

    // [Header ("BackgroundTransition")]
    // public GameObject[] Maps;
    // public float FadeSpeed = 0.1f;
    int _currentLevel = 1;
    float _oldTimeScale = 1f;

    void Start () {
        _musicPlayer = GameObject.Find ("MusicPlayer").GetComponent<MusicPlayer> ();
        _uiController = GameObject.Find ("UI").GetComponent<UIController> ();
        _gameProgressionHUD = GameObject.Find ("UI").GetComponent<GameProgressionHUD> ();
        _backgroundTransition = GetComponent<BackgroundTransition> ();

        //workaround
        var blackScreen = GameObject.Find ("BlackScreen").GetComponent<UnityEngine.UI.Image> ();
        blackScreen.color = new Color (0, 0, 0, 1);

        _blackScreenFadeController = GameObject.Find ("BlackScreen").GetComponent<FadeController> ();

        _pause = GameObject.Find ("Interface").transform.GetChild(0).gameObject;

        StartCoroutine (GameIntro ());
    }

    void Update () {
        if (Input.GetButtonDown ("Pause")) {
            Pause ();
        }
    }

    IEnumerator GameIntro () {
        var _playerInitMoveScript = GameObject.Find ("Player").GetComponent<PlayerInitialMovement> ();

        _musicPlayer.ChangeLevelSong (0, true);
        yield return StartCoroutine (_playerInitMoveScript.MovePlayerToInitPosition ());
        yield return StartCoroutine (_uiController.ShowStageTitle (_currentLevel));

        _gameProgressionHUD.SetNewProgression (_currentLevel);
        GameObject.Find ("Spawners").transform.Find ("EnemySpawner").gameObject.SetActive (true);
    }

    void Pause () {
        if (!_pause.activeSelf) {
            _oldTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _pause.SetActive (true);
        } else {
            _oldTimeScale = 1f;
            Time.timeScale = _oldTimeScale;
            _pause.SetActive (false);
        }

    }

    public void BossEncounter () {
        _musicPlayer.PlayBossTheme ();
    }

    public void MoveToNextLevel () {
        if (_currentLevel < 4)
            StartCoroutine (MoveToNextLevel_ ());
        else
            Fanfare ();
    }
    IEnumerator MoveToNextLevel_ () {
        GameObject.Find ("Spawners").transform.Find ("EnemySpawner").gameObject.SetActive (false);

        _currentLevel++;

        _musicPlayer.ChangeLevelSong (_currentLevel - 1);
        yield return new WaitForSeconds (5f);
        yield return StartCoroutine (_uiController.ShowStageTitle (_currentLevel));

        _backgroundTransition.TransitionMaps (_currentLevel - 1);

        Debug.Log (_gameProgressionHUD);
        _gameProgressionHUD.SetNewProgression (_currentLevel);
        GameObject.Find ("Spawners").transform.Find ("EnemySpawner").gameObject.SetActive (true);
    }

    public void GameOverEvent () {
        StartCoroutine (GameOverEvent_ ());
    }
    IEnumerator GameOverEvent_ () {
        _musicPlayer.PlayPlayerDeath ();
        yield return StartCoroutine (_uiController.ShowGameOver ());
        yield return new WaitForSeconds (3f);

        yield return StartCoroutine (_blackScreenFadeController.FadeIn (1));
        yield return new WaitForSeconds (1f);

        GameObject.Find ("LevelManager").GetComponent<Level> ().LoadStartMenu ();
    }

    public void Fanfare () {
        StartCoroutine (Fanfare_ ());
    }
    IEnumerator Fanfare_ () {
        _musicPlayer.FadeIn (3f);
        yield return new WaitForSeconds (4);

        _musicPlayer.PlayCredits ();

        var playerHUD = GameObject.Find ("UI/Player");
        var progressionHUD = GameObject.Find ("UI/ProgressionBar");

        yield return StartCoroutine (_uiController.ShowFanfare ());

        IEnumerable<FadeController> faders = new List<FadeController> ();
        faders = faders.Concat (playerHUD.GetComponents<FadeController> ());
        faders = faders.Concat (progressionHUD.GetComponents<FadeController> ());
        faders = faders.Concat (playerHUD.GetComponentsInChildren<FadeController> ());
        faders = faders.Concat (progressionHUD.GetComponentsInChildren<FadeController> ());
        faders.ToList ().ForEach (_ => StartCoroutine (_.FadeIn (1f, true)));

        yield return StartCoroutine (_blackScreenFadeController.FadeOut (1f, 0.5f, true));

        yield return new WaitForSeconds (1f);
        yield return StartCoroutine (_uiController.ShowCredits ());
        yield return new WaitForSeconds (2f);

        yield return StartCoroutine (_blackScreenFadeController.FadeOut (2f));

        GameObject.Find ("LevelManager").GetComponent<Level> ().LoadStartMenu ();
    }
}
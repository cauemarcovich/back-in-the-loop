using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public RectTransform StageTitle;
    public RectTransform GameOver;
    public RectTransform Fanfare;
    public RectTransform BlackScreen;
    public RectTransform[] Credits;
    public Image Logo;

    MessageController _messageController;
    Credits _credits;

    void Start () {
        _messageController = GetComponent<MessageController> ();
        _credits = GetComponent<Credits>();
    }

    public IEnumerator ShowStageTitle (int stageLevel) {
        StageTitle.gameObject.SetActive (true);

        var levelMessage = "Stage " + stageLevel;
        var opositePosition = new Vector2 (-StageTitle.anchoredPosition.x, 0f);

        yield return StartCoroutine (_messageController.ShowMessage_Slide (levelMessage, StageTitle, StageTitle.anchoredPosition, Vector2.zero, 20f));
        yield return StartCoroutine (_messageController.ShowMessage_Slide (levelMessage, StageTitle, StageTitle.anchoredPosition, opositePosition, 20f));

        StageTitle.gameObject.SetActive (false);
    }

    public IEnumerator ShowGameOver () {
        GameOver.gameObject.SetActive (true);
        yield return StartCoroutine (_messageController.ShowMessage_FadeOut ("Game Over", GameOver, GameOver.anchoredPosition, 1f));
        yield return new WaitForSeconds (3f);
        yield return StartCoroutine (_messageController.ShowMessage_FadeIn ("Game Over", GameOver, GameOver.anchoredPosition, 1f));
        GameOver.gameObject.SetActive (false);
    }

    public IEnumerator ShowFanfare () {
        Fanfare.gameObject.SetActive (true);
        yield return StartCoroutine (_messageController.ShowMessage_FadeOut ("YOU WIN!", Fanfare, Fanfare.anchoredPosition, 1f));
        yield return new WaitForSeconds (5f);
        yield return StartCoroutine (_messageController.ShowMessage_FadeIn ("YOU WIN!", Fanfare, Fanfare.anchoredPosition, 1f));
        Fanfare.gameObject.SetActive (false);
    }

    public IEnumerator ShowCredits () {
        yield return 0;
        Credits[0].gameObject.SetActive (true);
        Credits[1].gameObject.SetActive (true);
        Logo.gameObject.SetActive (true);
        yield return StartCoroutine (_credits.ShowCredits ());
    }

    public void UpdateScore () {
        Debug.Log ("score");
        //score = Player.score;
    }

    enum MessageType { Slide, Fade }
}
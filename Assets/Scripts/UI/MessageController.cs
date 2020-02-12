using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageController : MonoBehaviour {
	public IEnumerator ShowMessage_Slide (string message, RectTransform messageObject, Vector2 initPosition, Vector2 endPosition, float speed) {
		var textMesh = messageObject.GetComponent<TMPro.TextMeshProUGUI> ();
		textMesh.text = message;

		yield return new WaitForSeconds (1f);

		messageObject.anchoredPosition = initPosition;

		yield return new WaitUntil (() => {
			messageObject.anchoredPosition = Vector3.MoveTowards (messageObject.anchoredPosition, endPosition, speed);
			return Vector3.SqrMagnitude (endPosition - messageObject.anchoredPosition) < .1f;
		});
	}
	public IEnumerator ShowMessage_FadeIn (string message, RectTransform messageObject, Vector2 position, float fadeDuration) {
		yield return StartCoroutine (ShowMessage_Fade (message, messageObject, position, fadeDuration, true));
	}
	public IEnumerator ShowMessage_FadeOut (string message, RectTransform messageObject, Vector2 position, float fadeDuration) {
		yield return StartCoroutine (ShowMessage_Fade (message, messageObject, position, fadeDuration, false));
	}
	IEnumerator ShowMessage_Fade (string message, RectTransform messageObject, Vector2 position, float fadeDuration, bool fadeIn) {
		yield return new WaitForSeconds(.01f);
		var fadeController = messageObject.GetComponent<FadeController> ();
		var textMesh = messageObject.GetComponent<TMPro.TextMeshProUGUI> ();
		textMesh.text = message;

		yield return new WaitForSeconds (1f);

		messageObject.anchoredPosition = position;
		if (fadeIn)
			yield return StartCoroutine (fadeController.FadeIn (fadeDuration, true));
		else
			yield return StartCoroutine (fadeController.FadeOut (fadeDuration, true));
	}
}
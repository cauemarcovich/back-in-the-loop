using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (FadeController))]
public class BlackScreen : MonoBehaviour {
	public float FadeDuration;

	void Start () {
		StartCoroutine (Fade());
	}

	IEnumerator Fade () {
		yield return new WaitForSeconds(0.01f);
		yield return GetComponent<FadeController> ().FadeIn (FadeDuration);
		gameObject.SetActive(false);
	}
}
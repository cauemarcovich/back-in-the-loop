using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour {
	List<Transform> CreditTextObjects;
	Transform Logo;

	void Awake () {
		CreditTextObjects = new List<Transform>();
		CreditTextObjects.Add(transform.Find ("Credits/Credits"));
		CreditTextObjects.Add(transform.Find ("Credits/Credits_2"));
		Logo = transform.Find ("Credits/TallonicLogo");
	}

	public IEnumerator ShowCredits () {
		yield return new WaitForSeconds(0.5f);
		var text = GetCreditsText ();
		
		var textMesh = CreditTextObjects.Select (_ => {
			return _.gameObject.GetComponent<TMPro.TextMeshProUGUI> ();
		}).ToArray ();

		foreach (var line in text) {
			var phrases = line.Split (new string[] { "&&" }, System.StringSplitOptions.None);
			var fontSize = 50f;

			if (phrases.Length > 1) {
				if (phrases[0][0] != '?') {
					fontSize = 30;
				} else {
					phrases[0] = phrases[0].Replace ("?", "");
				}
			}

			textMesh.ToList ().ForEach (_ => _.fontSize = fontSize);

			for (int i = 0; i < phrases.Length; i++) {
				if (phrases[i].Contains ("img")) {

					yield return StartCoroutine (Logo.gameObject.GetComponent<FadeController> ().FadeOut (0.5f));
					// yield return new WaitUntil (() => {
					// 	var oldColor = Logo.color;
					// 	var newAlpha = Mathf.Clamp01 (oldColor.a + 1); //1 = fadespeed

					// 	Logo.color = new Color (oldColor.r, oldColor.g, oldColor.b, newAlpha);

					// 	return newAlpha == 1;
					// });

					yield return new WaitForSeconds (3f);

					yield return StartCoroutine (Logo.gameObject.GetComponent<FadeController> ().FadeIn (0.5f));
					// yield return new WaitUntil (() => {
					// 	var oldColor = Logo.color;
					// 	var newAlpha = Mathf.Clamp01 (oldColor.a - 1); //1 = fadespeed

					// 	Logo.color = new Color (oldColor.r, oldColor.g, oldColor.b, newAlpha);

					// 	return newAlpha == 0;
					// });

					continue;
				}

				textMesh[i].text = phrases[i];

				yield return StartCoroutine (textMesh[i].GetComponent<FadeController> ().FadeOut (1f));

				// yield return new WaitUntil (() => {
				// 	var oldColor = textMesh[i].color;
				// 	var newAlpha = Mathf.Clamp01 (oldColor.a + 1); //1 = fadespeed

				// 	textMesh[i].color = new Color (oldColor.r, oldColor.g, oldColor.b, newAlpha);

				// 	return newAlpha == 1;
				// });

				yield return new WaitForSeconds (2f);
			}
			for (int i = 0; i < textMesh.Length; i++) {
				yield return StartCoroutine (textMesh[i].GetComponent<FadeController> ().FadeIn (1f));
				// yield return new WaitUntil (() => {
				// 	var oldColor = textMesh[i].color;
				// 	var newAlpha = Mathf.Clamp01 (oldColor.a - 1); //1 = fadespeed

				// 	textMesh[i].color = new Color (oldColor.r, oldColor.g, oldColor.b, newAlpha);

				// 	return newAlpha == 0;
				// });
			}

			yield return new WaitForSeconds (.5f);
		}
	}

	string[] GetCreditsText () {
		var list = new List<string> ();

		list.Add ("CREDITS");

		list.Add ("DIRECTOR\nC.MARCOVICH");
		list.Add ("PROGRAMMING\nC.MARCOVICH");
		list.Add ("POOR DESIGN\nC.MARCOVICH");

		list.Add ("GRAPHICS / ARTWORK\nE.NASCIMENTO&&\nAND\nC.MARCOVICH... AGAIN");

		list.Add ("3D MODELING\nMr.Nobody&&\nafter all, this game has no 3D");
		list.Add ("just kidding...");
		list.Add ("I mean, i'm not kidding now");

		list.Add ("MUSIC\nKevin MacLeod&&\nIncompetech");

		list.Add ("?A GAME BY&&\nTALLONIC&&img:TallonicLogo");

		list.Add ("");

		list.Add ("?&&THANK YOU\nFOR\nPLAYING!");

		return list.ToArray ();
	}
}
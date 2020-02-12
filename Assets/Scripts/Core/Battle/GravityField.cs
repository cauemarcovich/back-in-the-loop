using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour {
	public float Durability;
	public float VanishSpeed;

	void Start () {
		StartCoroutine (Deathtime ());
	}

	IEnumerator Deathtime () {
		var cameraShake = GameObject.Find ("Main Camera").GetComponent<CameraShake> ();
		cameraShake.ProgressiveShake(Durability * 2, 0f, 0.025f);

		yield return new WaitForSeconds (Durability);

		var spriteRenderer = GetComponent<SpriteRenderer> ();

		yield return new WaitUntil (() => {
			var alpha = Mathf.Clamp01 (spriteRenderer.color.a - VanishSpeed);
			spriteRenderer.color = new Color (spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

			return spriteRenderer.color.a == 0;
		});

		Destroy (gameObject);
	}
}
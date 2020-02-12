using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
	public void Shake (float duration, float magnitude) {
		StartCoroutine (Shake_ (duration, magnitude));
	}
	public void ProgressiveShake (float duration, float minMagnitude, float maxMagnitude) {
		StartCoroutine (ProgressiveShake_ (duration, minMagnitude, maxMagnitude));
	}

	IEnumerator Shake_ (float duration, float magnitude) {
		var origPosition = transform.localPosition;
		float elapsed = 0.0f;

		while (elapsed < duration) {
			var x = Random.Range (-1, 1f) * magnitude;
			var y = Random.Range (-1, 1f) * magnitude;

			transform.localPosition = new Vector3 (x, y, origPosition.z);

			elapsed += Time.deltaTime;

			yield return null;
		}

		transform.localPosition = origPosition;
	}

	IEnumerator ProgressiveShake_ (float duration, float minMagnitude, float maxMagnitude) {
		var timeBetweenShakes = 0.1f;
		var magnitude = 0f;

		var timeElapsed = 0f;
		var lastTimeElapsed = Time.time;

		while (timeElapsed < duration * 0.5f) {
			if (magnitude < maxMagnitude)
				magnitude = Mathf.Clamp (magnitude + Time.deltaTime / (duration * 0.5f), minMagnitude, maxMagnitude);

			yield return StartCoroutine (Shake_ (timeBetweenShakes, magnitude));

			timeElapsed += Time.time - lastTimeElapsed;
			lastTimeElapsed = Time.time;
		}

		timeElapsed = 0f;
		lastTimeElapsed = Time.time;

		while (timeElapsed < duration * 0.5f) {
			if (magnitude > minMagnitude)
				magnitude = Mathf.Clamp (magnitude - Time.deltaTime / (duration * 0.5f), minMagnitude, maxMagnitude);

			yield return StartCoroutine (Shake_ (timeBetweenShakes, magnitude));

			timeElapsed += Time.time - lastTimeElapsed;
			lastTimeElapsed = Time.time;
		}
	}
}
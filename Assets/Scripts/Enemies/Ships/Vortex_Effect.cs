using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vortex_Effect : MonoBehaviour {
	public float SlowTimeSpeed = 0.2f;

	Vector3 _oldMaskSize;

	void OnEnable () {
		_oldMaskSize = new Vector3 (transform.localScale.x, transform.localScale.y);

		StartCoroutine (Effect ());
	}

	IEnumerator Effect () {
		var cameraShake = GameObject.Find ("Main Camera").GetComponent<CameraShake> ();

		Time.timeScale = SlowTimeSpeed;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;

		cameraShake.ProgressiveShake (1f, 0f, 0.1f);

		var vel = 1.0f;
		var vel2 = 1.0f;

		while (Vector3.SqrMagnitude (transform.localScale - Vector3.zero) > 0.00001f) {
			var newX = Mathf.SmoothDamp (transform.localScale.x, 0f, ref vel, 0.04f);
			var newY = Mathf.SmoothDamp (transform.localScale.y, 0f, ref vel2, 0.04f);

			transform.localScale = new Vector3 (newX, newY);
			yield return new WaitForFixedUpdate ();
		}

		yield return new WaitForSeconds (.1f);
		transform.GetComponent<CircleCollider2D> ().enabled = true;

		var maxSize = new Vector3 (8f, 8f, 0);
		while (transform.localScale.x < maxSize.x && transform.localScale.y < maxSize.y) {
			var newX = Mathf.SmoothDamp (transform.localScale.x, 32f, ref vel, 0.5f);
			var newY = Mathf.SmoothDamp (transform.localScale.y, 32f, ref vel, 0.5f);

			transform.localScale = new Vector3 (newX, newY);

			yield return new WaitForFixedUpdate ();
		}

		Destroy (transform.parent.gameObject);

		Time.timeScale = 1f;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitialMovement : MonoBehaviour {
	public IEnumerator MovePlayerToInitPosition () {
		yield return new WaitForSeconds (1f);

		var newPos = new Vector3 (0f, -5f);
		yield return new WaitUntil (() => {
			transform.position = Vector3.MoveTowards (transform.position, newPos, 3f * Time.deltaTime);
			return transform.position == newPos;
		});

		GetComponent<PlayerController> ()._lockMove = false;
		Destroy (this);
	}
}
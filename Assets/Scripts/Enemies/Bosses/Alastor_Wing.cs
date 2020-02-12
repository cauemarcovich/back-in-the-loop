using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alastor_Wing : MonoBehaviour {
	public AudioClip Shield_Sound;

	void OnTriggerEnter2D (Collider2D other) {
		var damageDealer = other.GetComponent<DamageDealer> ();

		if (damageDealer != null) {
			AudioSource.PlayClipAtPoint (Shield_Sound, Camera.main.transform.position, 1f);
			damageDealer.Hit ();
		}
	}
}
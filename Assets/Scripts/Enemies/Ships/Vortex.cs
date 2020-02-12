using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vortex : Enemy {
	[Header ("Vortex Config")]
	public float MinSize = .2f;
	float MaxSize;

	public float RecoverSpeed = 1f;

	float _maxHealth;

	new void Start () {
		base.Start ();

		MaxSize = transform.localScale.x;
		_maxHealth = Health;
		Health = 10;

		transform.localScale = new Vector3 (0f, 0f, 0f);
	}

	void FixedUpdate () {
		var newHealth = Mathf.Lerp (Health, _maxHealth, Time.deltaTime * RecoverSpeed);
		Health = Mathf.Clamp (newHealth, 0f, _maxHealth);
		UpdateSize ();
	}

	new void OnTriggerEnter2D (Collider2D other) {
		base.OnTriggerEnter2D (other);
		UpdateSize ();
	}
	void UpdateSize () {
		var scaleValue = (((MaxSize - MinSize) * Health) / _maxHealth) + MinSize;
		//Debug.Log (scaleValue);
		transform.localScale = new Vector3 (scaleValue, scaleValue, transform.localScale.z);
	}

	void OnDestroy () {
		var effectObject = transform.GetChild (0);
		effectObject.parent = null;
		effectObject.localScale = new Vector3 (1, 1, 1);
		effectObject.rotation = Quaternion.identity;

		effectObject.gameObject.SetActive (true);

		var effect = effectObject.GetChild (1);
		effect.GetComponent<Vortex_Effect> ().enabled = true;

	}
}
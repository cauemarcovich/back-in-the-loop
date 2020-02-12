using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampBall : MonoBehaviour {
	public float MaxShakeMagnitude;

	void Start () {
		var cameraShake = GameObject.Find ("Main Camera").GetComponent<CameraShake> ();
		cameraShake.ProgressiveShake (8f, 0f, MaxShakeMagnitude);
	}
}
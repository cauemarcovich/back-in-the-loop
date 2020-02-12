using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour {
    public float healRate;
    public float fallSpeed;

    [Header ("Sound")]
    public AudioClip pickSound;
    public float pickSoundVolume = 0.75f;

    void FixedUpdate () {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D (Collider2D collider) {
        if (collider.tag == "Shredder")
            return;

        var playerController = collider.GetComponent<PlayerController> ();

        var amount = playerController.max_health * (healRate / 100);
        playerController.ChangeHealth (amount);

        Destroy (gameObject);

        AudioSource.PlayClipAtPoint (pickSound, Camera.main.transform.position, pickSoundVolume);
    }
}
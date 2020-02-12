using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    public ShipType powerUpType;
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

        var shipRank = collider.gameObject.GetComponent<ShipRank> ();
        if (shipRank == null) Debug.LogWarning ("AQUI");

        shipRank.Leveling (powerUpType);

        AudioSource.PlayClipAtPoint (pickSound, Camera.main.transform.position, pickSoundVolume);
        Destroy (gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using UnityEngine;

public class Vfx : MonoBehaviour {
    public GameObject[] shipEffectPrefabs;
    SpriteRenderer[] currentShipFires;

    void Start () {
        currentShipFires = shipEffectPrefabs[0].GetComponentsInChildren<SpriteRenderer> ();

        StartCoroutine (FireVfx ());
    }

    IEnumerator FireVfx () {
        while (true) {
            yield return Blink ();

            var vertical = Input.GetAxisRaw ("Vertical");
            var alpha = (vertical + 1) / 2;
            foreach (var shipFire in currentShipFires) {
                var color = shipFire.color;
                color.a = alpha;
                shipFire.color = color;
            }

            yield return new WaitForSeconds (Random.Range (0.1f, 0.2f));
        }
    }
    WaitForSeconds Blink () {
        foreach (var shipFire in currentShipFires) {
            var color = shipFire.color;
            color.a = 0;
            shipFire.color = color;
        }

        return new WaitForSeconds (Random.Range (0, 0.075f));
    }

    public void ChangeShipFire (Rank shipRank) {
        GameObject firePrefab = null;
        if (shipRank.ShipType != ShipType.X) {
            if (shipRank.Level.IsBetween (1, 2))
                firePrefab = shipEffectPrefabs[0];
            else if (shipRank.Level == 3)
                firePrefab = shipEffectPrefabs[1];
            else if (shipRank.Level == 4)
                firePrefab = shipEffectPrefabs[2];
        } else {

            if (shipRank.Level.IsBetween (1, 4))
                firePrefab = shipEffectPrefabs[0];
            else if (shipRank.Level.IsBetween (5, 9))
                firePrefab = shipEffectPrefabs[1];
            else if (shipRank.Level == 10)
                firePrefab = shipEffectPrefabs[2];
        }
        if (firePrefab == null) {
            Debug.LogError ("VFX not found.");
            return;
        }

        var color = new Color ();
        switch (shipRank.ShipType) {
            case ShipType.Normal:
                color = new Color32 (55, 187, 244, 126);
                break; //blue
            case ShipType.Splitter:
                color = new Color32 (112, 202, 55, 126);
                break; //green
            case ShipType.Blaster:
                color = new Color32 (173, 57, 57, 126);
                break; //red
            case ShipType.X:
                color = new Color32 (214, 215, 214, 126);
                break; //orange
            default:
                break;
        }

        ChangeFireVfx (firePrefab, color);
    }
    void ChangeFireVfx (GameObject firePrefab, Color color) {
        if (transform.childCount > 0) {
            var fireObject = transform.GetChild (0);
            Destroy (fireObject.gameObject);
        }
        var newFireObject = Instantiate (firePrefab, transform);
        foreach (Transform fire in newFireObject.transform) {
            fire.GetComponent<SpriteRenderer> ().color = color;
        }

        currentShipFires = newFireObject.GetComponentsInChildren<SpriteRenderer> ();
    }
}
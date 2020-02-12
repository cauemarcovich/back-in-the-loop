using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySpawner : MonoBehaviour {
    public float droprate;
    public EnergyDropRate[] drops;

    public void Spawn (Vector3 position) {
        var acc = 0f;
        var random = Random.Range (0, 100);

        GameObject objectToSpawn = null;

        for (int i = 0; i < drops.Length; i++) {
            acc += drops[i].droprate;

            if (random <= acc) {
                objectToSpawn = drops[i].energy;
                break;
            }
        }

        Instantiate (objectToSpawn, position, Quaternion.identity);
    }

    [System.Serializable]
    public class EnergyDropRate {
        public GameObject energy;
        public float droprate;
    }
}
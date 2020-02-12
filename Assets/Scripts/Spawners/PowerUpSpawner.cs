using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour {
    public float dropRate;

    public List<GameObject> drops;

    public void Spawn (Vector3 position) {
        var spawnList = GetSpawnList ();
        if (spawnList == null) return;
        var random = Random.Range (0, 100);
        var spawnType = GetSpawnType (random, spawnList);
        CreatePowerUp (spawnType, position);
    }

    void CreatePowerUp (ShipType type, Vector3 position) {
        if (type == ShipType.Normal)
            Instantiate (drops[0], position, Quaternion.identity);
        if (type == ShipType.Splitter)
            Instantiate (drops[1], position, Quaternion.identity);
        if (type == ShipType.Blaster)
            Instantiate (drops[2], position, Quaternion.identity);
        if (type == ShipType.X)
            Instantiate (drops[3], position, Quaternion.identity);
    }

    IDictionary<ShipType, float> GetSpawnList () {
        var p = GameObject.FindGameObjectWithTag ("Player");

        if (p == null) return null;

        var playerRank = p.GetComponent<ShipRank> ();
        if (playerRank == null) {
            Debug.LogWarning ("Player not found");
            return null;
        }

        var powerUpList = new Dictionary<ShipType, float> ();
        var curST = playerRank.currentRank.ShipType;

        if (curST != ShipType.X) {
            powerUpList.Add (ShipType.Normal, 25f);
            powerUpList.Add (ShipType.Splitter, 25f);
            powerUpList.Add (ShipType.Blaster, 25f);

            powerUpList[curST] += 25f;
        } else {
            powerUpList.Add (ShipType.X, 100f);
        }

        return powerUpList;
    }

    ShipType GetSpawnType (float random, IDictionary<ShipType, float> list) {
        if (list.Count == 1) return list.FirstOrDefault ().Key;

        var acc = 0f;

        for (int i = 0; i < list.Count; i++) {
            acc += list[(ShipType) i];

            if (random <= acc) {
                return (ShipType) i;
            }
        }

        Debug.LogError ("ShipType not found.");
        return (ShipType) 4;
    }
}
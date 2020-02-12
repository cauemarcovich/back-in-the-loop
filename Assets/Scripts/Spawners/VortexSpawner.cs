using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VortexSpawner : MonoBehaviour {
	public float SpawnRate;
	public List<GameObject> AllVortexConfiguration;

	void Start () {
		StartCoroutine (VortexSpawn ());
	}

	IEnumerator VortexSpawn () {
		while (true) {
			var vortexAmount = GameObject.FindObjectsOfType<GameObject> ().Where (_ => _.name.Contains ("Vortex ")).Count ();
			if (vortexAmount == 0) {
				if (Random.Range (0, 100) <= SpawnRate) {
					CreateVortex ();
				}
			}
			yield return new WaitForSeconds (5f);
		}
	}

	void CreateVortex () {
		var player = GameObject.Find ("Player");
		if (player == null) return;

		var pRank = player.GetComponent<ShipRank> ();
		var playerIsX = pRank.currentRank.ShipType == ShipType.X;
		var enemyLevel = System.Convert.ToInt32 (pRank.currentRank.Level / (playerIsX ? 4 : 2));

		var vortexPrefab = AllVortexConfiguration[enemyLevel];
		var enemyPathing = vortexPrefab.GetComponent<EnemyPathing> ();

		var path = enemyPathing.WaveConfig.PathPrefab.transform;
		var newPath = new Vector2 (Random.Range (-4, 4), Random.Range (0, 7));
		foreach (Transform p in path) { p.position = newPath; }

		var vortex = Instantiate (
			vortexPrefab,
			enemyPathing.WaveConfig.Waypoints () [0].position,
			Quaternion.identity);
		vortex.GetComponent<Enemy> ().Health = GetVortexRecoverSpeed (pRank);
	}

	float GetVortexRecoverSpeed (ShipRank pRank) {
		if (pRank.currentRank.ShipType == ShipType.Normal) {
			if (pRank.currentRank.Level == 1) return 35;
			if (pRank.currentRank.Level == 2) return 50;
			if (pRank.currentRank.Level == 3) return 80;
			if (pRank.currentRank.Level == 4) return 100;
		} else if (pRank.currentRank.ShipType == ShipType.Splitter) {
			if (pRank.currentRank.Level == 1) return 18;
			if (pRank.currentRank.Level == 2) return 30;
			if (pRank.currentRank.Level == 3) return 40;
			if (pRank.currentRank.Level == 4) return 50;
		} else if (pRank.currentRank.ShipType == ShipType.Blaster) {
			if (pRank.currentRank.Level == 1) return 35;
			if (pRank.currentRank.Level == 2) return 50;
			if (pRank.currentRank.Level == 3) return 80;
			if (pRank.currentRank.Level == 4) return 100;
		} else if (pRank.currentRank.ShipType == ShipType.X) {
			if (pRank.currentRank.Level == 1) return 50;
			if (pRank.currentRank.Level == 2) return 100;
			if (pRank.currentRank.Level == 3) return 150;
			if (pRank.currentRank.Level == 4) return 200;
		}
		return 1;
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Enemy Wave Config")]
public class WaveConfig : ScriptableObject { //
    public GameObject EnemyPrefab;
    public GameObject PathPrefab;
    public float TimeBetweenWaves = 0.5f;
    public float SpawnRandomFactor = 0.3f;
    public int NumberOfEnemies = 5;
    public float MoveSpeed = 2f;
    public bool RepeatPathing = false;
    public bool IsBoss = false;

    public List<Transform> Waypoints () {
        var waypoints = new List<Transform> ();
        foreach (Transform path in PathPrefab.transform) {
            waypoints.Add (path);
        }
        return waypoints;
    }
}

[System.Serializable]
public class Range {
    public float Min = 0f;
    public float Max = 1f;
}
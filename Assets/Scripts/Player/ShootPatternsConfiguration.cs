using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Shoot Pattern Configuration")]
public class ShootPatternsConfiguration : ScriptableObject {
    public ShootPattern[] patterns;
}

[System.Serializable]
public class ShootPattern {
    public int level;
    public Transform[] positions;
}
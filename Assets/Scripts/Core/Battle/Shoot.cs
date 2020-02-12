using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player {
    [System.Serializable]
    public class Shoot {
        public GameObject projectile;
        public float projectileSpeed = 5f;

        [Header ("Configuration")]
        public ShootConfiguration[] config;

        //[Header("Patterns")]
        //public ShootPatternsConfiguration patternsConfig;
        //public ShootPattern[] patterns;

        [Header ("Audio")]
        public AudioClip projectileSound;
        [Range (0, 1)] public float projectileSoundVolume = 0.75f;

        public float GetFireRate (int level) {
            return config.FirstOrDefault (_ => _.level == level).firerate;
        }
        public Transform[] GetPatternPositions (int level, string name = null) {
            var pattern = config.FirstOrDefault (_ => _.level == level).pattern.transform;
            var positions = new List<Transform> ();

            foreach (Transform child in pattern) {
                if (name != null && !child.name.Contains (name)) continue;

                positions.Add (child);
            }

            return positions.ToArray ();
        }

        #region configuration
        [System.Serializable]
        public class ShootConfiguration {
            public int level;
            public float firerate;
            public GameObject pattern;
        }
        #endregion
    }
    public class Shoot_Bomb : Shoot {
        [Header ("Bomb Config")]
        public GameObject bomb;
        public float pulseForce;
        public AudioClip bombSound;
        [Range (0, 1)] public float bombSoundVolume = 0.75f;
    }

    [Serializable]
    public class NormalShoot : Shoot { }

    [Serializable]
    public class SplitterShoot : Shoot { }

    [Serializable]
    public class BlasterShoot : Shoot_Bomb { }

    [Serializable]
    public class XShoot : Shoot_Bomb {
        [Header ("X Prefabs")]
        public GameObject[] NormalShootsPrefabs;
        public GameObject[] SplitterShootsPrefabs;
        public GameObject[] BlasterShootsPrefabs;
        public GameObject[] BombPrefabs;
    }
}
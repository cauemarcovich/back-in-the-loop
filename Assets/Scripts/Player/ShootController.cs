using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Player;
using UnityEngine;

public class ShootController : MonoBehaviour {
    [Header ("Projectiles")]
    public NormalShoot NormalShoot;
    public SplitterShoot SplitterShoot;
    public BlasterShoot BlasterShoot;
    public XShoot XShoot;

    ShipRank shipRank;
    Shoot currentShoot {
        get {
            switch (shipRank.currentRank.ShipType) {
                case ShipType.Normal:
                    return NormalShoot;
                case ShipType.Splitter:
                    return SplitterShoot;
                case ShipType.Blaster:
                    return BlasterShoot;
                case ShipType.X:
                    return XShoot;
                default:
                    return null;
            }
        }
    }

    [HideInInspector] public bool _isFiring;
    Shoot_Bomb currentBomb {
        get {
            switch (shipRank.currentRank.ShipType) {
                case ShipType.Blaster:
                    return BlasterShoot;
                case ShipType.X:
                    return XShoot;
                default:
                    return null;
            }
        }
    }

    void Start () {
        shipRank = GetComponent<ShipRank> ();
    }

    public IEnumerator FireRepeater () {
        _isFiring = true;
        while (true) {
            var shipType = shipRank.currentRank.ShipType;
            if (shipType == ShipType.Normal) {
                Fire_Normal ();
            } else if (shipType == ShipType.Splitter) {
                Fire_Splitter ();
            } else if (shipType == ShipType.Blaster) {
                Fire_Blaster ();
            } else if (shipType == ShipType.X) {
                Fire_X ();
            }
            yield return new WaitForSeconds (GetCurrentFireRate ());
        }
    }

    #region Normal Shoot
    void Fire_Normal () {
        var level = shipRank.currentRank.Level;
        var positions = currentShoot.GetPatternPositions (level, "Laser1");

        foreach (Transform item in positions) {
            var shoot = Instantiate (currentShoot.projectile, transform.position + item.position, Quaternion.identity);
            shoot.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0f, currentShoot.projectileSpeed);
        }
        AudioSource.PlayClipAtPoint (currentShoot.projectileSound, Camera.main.transform.position, currentShoot.projectileSoundVolume);
    }
    #endregion

    #region Splitter Shoot
    void Fire_Splitter () {
        var level = shipRank.currentRank.Level;
        var positions = currentShoot.GetPatternPositions (level, "Laser2");

        var direction = 1.75f;

        for (int i = 0; i < positions.Length; i++) {
            if (i != 0 && i % 2 == 0) {
                direction = i / 2;
            }

            direction *= -1;

            StartCoroutine (Fire_Splitter_shoot (positions[i].position, direction, currentShoot));
        }
        AudioSource.PlayClipAtPoint (SplitterShoot.projectileSound, Camera.main.transform.position, NormalShoot.projectileSoundVolume);
    }
    IEnumerator Fire_Splitter_shoot (Vector3 position, float direction, Shoot curShoot) {
        //yield return new WaitForSeconds(startTime);

        var shoot = Instantiate (curShoot.projectile, transform.position + position, Quaternion.identity);
        shoot.GetComponent<Rigidbody2D> ().velocity = new Vector3 (direction * curShoot.projectileSpeed / 4, curShoot.projectileSpeed);

        yield return 0;
    }
    #endregion

    #region Blaster Shoot
    void Fire_Blaster () {
        var level = shipRank.currentRank.Level;

        var shoots = currentShoot.GetPatternPositions (level, "Laser3");
        var bombs = currentShoot.GetPatternPositions (level, "Bomb");
        //var shoots = allPpositions.Where(_ => _.name.Contains("Laser")).ToArray();
        //var bombs = allPpositions.Where(_ => _.name.Contains("Bomb")).ToArray();

        Fire_Blaster_shoot (shoots);
        Fire_Blaster_bombs (bombs);

        AudioSource.PlayClipAtPoint (currentShoot.projectileSound, Camera.main.transform.position, currentShoot.projectileSoundVolume);
    }
    void Fire_Blaster_shoot (Transform[] shoots) {
        foreach (Transform item in shoots) {
            var shoot = Instantiate (currentShoot.projectile, transform.position + item.position, Quaternion.identity);
            shoot.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0f, currentShoot.projectileSpeed);
        }
    }
    void Fire_Blaster_bombs (Transform[] bombs) {
        if (GameObject.FindGameObjectsWithTag ("Bomb").Length > 0)
            return;

        for (int i = 0; i < bombs.Length; i++) {
            var bomb = Instantiate (currentBomb.bomb, transform.position + bombs[i].position, Quaternion.identity);

            var pulseDirection = GetPulseDirection (bombs, i);
            StartCoroutine (PulseBomb (bomb, bombs[i].gameObject, pulseDirection));
        }
        if (bombs.Length > 0)
            AudioSource.PlayClipAtPoint (currentBomb.bombSound, Camera.main.transform.position, currentBomb.bombSoundVolume);
    }
    Vector2 GetPulseDirection (Transform[] bombs, int index) {
        var pulseForceDirections = new Vector2[bombs.Length];
        if (bombs.Length == 1) {
            pulseForceDirections[0] = new Vector2 (0f, -1 * currentBomb.pulseForce);
        } else if (bombs.Length == 2) {
            pulseForceDirections[0] = new Vector2 (-1 * currentBomb.pulseForce / 4, -1 * currentBomb.pulseForce);
            pulseForceDirections[1] = new Vector2 (1 * currentBomb.pulseForce / 4, -1 * currentBomb.pulseForce);
        } else if (bombs.Length == 3) {
            pulseForceDirections[0] = new Vector2 (-1 * currentBomb.pulseForce / 4, -1 * currentBomb.pulseForce);
            pulseForceDirections[1] = new Vector2 (1 * currentBomb.pulseForce / 4, -1 * currentBomb.pulseForce);
            pulseForceDirections[2] = new Vector2 (0 * currentBomb.pulseForce, -1 * currentBomb.pulseForce);
        }

        return pulseForceDirections[index];
    }
    IEnumerator PulseBomb (GameObject bomb, GameObject bombConfig, Vector3 direction) {
        var bomb_rigidbody = bomb.GetComponent<Rigidbody2D> ();
        var bomb_collider = bomb.GetComponent<Collider2D> ();
        var bomb_script = bomb.GetComponent<Bomb> ();
        var bomb_config = bombConfig.GetComponent<Bomb> ();

        bomb_collider.enabled = false;
        bomb_rigidbody.isKinematic = false;

        bomb_rigidbody.AddForce (direction);

        yield return new WaitForSeconds (bomb_config.speed / 10f);

        bomb_script.speed = bomb_config.speed;
        bomb_collider.enabled = true;
        bomb_rigidbody.isKinematic = true;
    }
    #endregion

    void Fire_X () {
        //var level = shipRank.currentRank.Level;
        //var pattern = XShoot.GetPatternPositions(level);

        //var normalShoots = pattern.Where(_ => _.name.Contains("Laser1")).ToArray();
        //var splitterShoots = pattern.Where(_ => _.name.Contains("Laser2")).ToArray();
        //var blasterShoots = pattern.Where(_ => _.name.Contains("Laser3")).ToArray();
        //var bombs = pattern.Where(_ => _.name.Contains("Bomb")).ToArray();

        ////FireNormal(normalShoots);
        var indexes = GetFireXPrefabsIndexes ();

        //Normal
        var normalIndex = indexes["Normal"];
        currentShoot.projectile = XShoot.NormalShootsPrefabs[normalIndex];
        Fire_Normal ();

        //Splitter
        var splitterIndex = indexes["Splitter"];
        currentShoot.projectile = XShoot.SplitterShootsPrefabs[splitterIndex];
        Fire_Splitter ();

        //Blaster and Bombs
        var blasterIndex = indexes["Blaster"];
        currentShoot.projectile = XShoot.BlasterShootsPrefabs[blasterIndex];
        var bombIndex = indexes["Bomb"];
        currentBomb.bomb = XShoot.BombPrefabs[bombIndex];

        Fire_Blaster ();
    }
    IDictionary<string, int> GetFireXPrefabsIndexes () {
        var lv = shipRank.currentRank.Level;
        int normalIndex = -1, splitterIndex = -1, blasterIndex = -1, bombIndex = -1;

        #region Indexes
        //NormalIndex
        if (lv == 1)
            normalIndex = 0;
        else if (lv >= 2 && lv <= 5)
            normalIndex = 1;
        else if (lv >= 6 && lv <= 9)
            normalIndex = 2;
        else if (lv == 10)
            normalIndex = 3;

        //SplitterIndex
        if (lv >= 1 && lv <= 2)
            splitterIndex = 0;
        else if (lv >= 3 && lv <= 6)
            splitterIndex = 1;
        else if (lv >= 7 && lv <= 10)
            splitterIndex = 2;

        //BlasterIndex
        if (lv >= 1 && lv <= 3)
            blasterIndex = 0;
        else if (lv >= 4 && lv <= 7)
            blasterIndex = 1;
        else if (lv >= 8 && lv <= 10)
            blasterIndex = 2;

        //MissileIndex
        if (lv >= 1 && lv <= 4)
            bombIndex = 0;
        else if (lv >= 5 && lv <= 8)
            bombIndex = 1;
        else if (lv >= 9 && lv <= 10)
            bombIndex = 2;

        if (normalIndex == -1 || splitterIndex == -1 || blasterIndex == -1 || bombIndex == -1)
            Debug.LogError ("Não foi possível encontrar o index.");
        #endregion

        var result = new Dictionary<string, int> ();
        result.Add ("Normal", normalIndex);
        result.Add ("Splitter", splitterIndex);
        result.Add ("Blaster", blasterIndex);
        result.Add ("Bomb", bombIndex);
        return result;
    }

    public float GetCurrentFireRate () {
        var shipType = shipRank.currentRank.ShipType;
        var level = shipRank.currentRank.Level;

        if (shipType == ShipType.Normal) {
            return NormalShoot.GetFireRate (level);
        } else if (shipType == ShipType.Splitter) {
            return SplitterShoot.GetFireRate (level);
        } else if (shipType == ShipType.Blaster) {
            return BlasterShoot.GetFireRate (level);
        } else if (shipType == ShipType.X) {
            return XShoot.GetFireRate (level);
        }
        return 999999f;
    }
}
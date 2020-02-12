using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunner : Enemy {
    new void Start () {
        base.Start ();
        _sm.SetState (Move);
    }

    void Move () {
        _moveCounter -= Time.deltaTime;

        if (_moveCounter <= 0) {
            _sm.SetState (Shoot);
            SetRandomShootCounter ();
        }
    }

    void Shoot () {
        var laser = Instantiate (Laser, transform.position, Quaternion.identity);
        var direction = Vector2.down;

        if (Tier != EnemyTier.Weak && _player != null) {
            direction = (_player.transform.position - transform.position).normalized;
            float rotation = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
            laser.transform.rotation = Quaternion.Euler (0f, 0f, rotation - 90);
        }

        laser.GetComponent<Rigidbody2D> ().velocity = direction * ProjectileSpeed; // new Vector3(0, -ProjectileSpeed);
        AudioSource.PlayClipAtPoint (projectileSound, Camera.main.transform.position, projectileSoundVolume);

        _sm.SetState (Move);
    }

    // void Update()
    // {
    //     CountdownAndShoot();
    // }

    // void CountdownAndShoot()
    // {
    //     _shootCounter -= Time.deltaTime;

    //     if (_shootCounter <= 0)
    //     {
    //         Fire();
    //         _shootCounter = Random.Range(MinTimeBetweenShoots, MaxTimeBetweenShoots);
    //     }
    // }
    // void Fire()
    // {
    //     var laser = Instantiate(Laser, transform.position, Quaternion.identity);
    //     var direction = Vector2.down;

    //     if (Tier != EnemyTier.Weak && _player != null)
    //     {
    //         direction = (_player.transform.position - transform.position).normalized;
    //         float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //         laser.transform.rotation = Quaternion.Euler(0f, 0f, rotation - 90);
    //     }

    //     laser.GetComponent<Rigidbody2D>().velocity = direction * ProjectileSpeed;// new Vector3(0, -ProjectileSpeed);
    //     AudioSource.PlayClipAtPoint(projectileSound, Camera.main.transform.position, projectileSoundVolume);
    // }
}
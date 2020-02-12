using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : Enemy {
    [Header ("Warrior Config")]
    public float burstInitialDelay;
    public int shootsPerBurst;
    public float timeBetweenShoots;

    new void Start () {
        base.Start ();
        _sm.SetState (Move);
    }

    void Move () {
        _moveCounter -= Time.deltaTime;

        if (_moveCounter <= 0) {
            _sm.SetState (Fire);
            SetRandomShootCounter ();
        }
    }

    void Fire () {
        if (!_pathing.IsStopped ())
            StartCoroutine (Firing ());
    }

    IEnumerator Firing () {
        _pathing.Stop (true);

        yield return new WaitForSeconds (burstInitialDelay);

        for (int i = 0; i < shootsPerBurst; i++) {
            var l1_direction = (Vector2.left + Vector2.down).normalized;
            var laser1 = Instantiate (Laser, transform.position, Quaternion.Euler (0f, 0f, -45));
            laser1.GetComponent<Rigidbody2D> ().velocity = l1_direction * ProjectileSpeed;

            var l2_direction = (Vector2.down).normalized;
            var laser2 = Instantiate (Laser, transform.position, Quaternion.Euler (0f, 0f, 0f));
            laser2.GetComponent<Rigidbody2D> ().velocity = l2_direction * ProjectileSpeed;

            var l3_direction = (Vector2.right + Vector2.down).normalized;
            var laser3 = Instantiate (Laser, transform.position, Quaternion.Euler (0f, 0f, 45));
            laser3.GetComponent<Rigidbody2D> ().velocity = l3_direction * ProjectileSpeed;

            AudioSource.PlayClipAtPoint (projectileSound, Camera.main.transform.position, projectileSoundVolume);
            yield return new WaitForSeconds (timeBetweenShoots);
        }

        _sm.SetState (Move);
        _pathing.Stop (false);
    }

    /*void Update()
    {
        shootCounter -= Time.deltaTime;

        if (shootCounter <= 0)
        {
            Fire();
            shootCounter = Random.Range(MinTimeBetweenShoots, MaxTimeBetweenShoots);
        }
    }
    void CountdownAndShoot()
    {
        shootCounter -= Time.deltaTime;

        if (shootCounter <= 0)
        {
            Fire();
            shootCounter = Random.Range(MinTimeBetweenShoots, MaxTimeBetweenShoots);
        }
    }
    // void Fire()
    // {
    //     StartCoroutine(Fire_Burst());
    // }

    IEnumerator Fire_Burst()
    {
        
    }*/
}
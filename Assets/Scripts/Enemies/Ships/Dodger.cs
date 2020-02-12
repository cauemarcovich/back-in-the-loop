using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodger : Enemy {
    [Header ("Dodger Config")]
    public float EvadeSpeed;
    Coroutine evading;

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

    public void SetEvadeState () { _sm.SetState (Evade); }
    void Evade () {
        if (!_pathing.IsStopped ())
            StartCoroutine (Evading ());
    }

    IEnumerator Evading () {
        _pathing.Stop (true);

        var dodgeCorretion = transform.position.x >= _player.transform.position.x ? 0.75f : -0.75f;
        var escapePosition = new Vector3 (
            transform.position.x + dodgeCorretion,
            transform.position.y,
            0f);

        while (transform.position != escapePosition) {
            transform.position = Vector3.MoveTowards (transform.position, escapePosition, EvadeSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame ();
        }

        _sm.SetState (Move);
        _pathing.Stop (false);

        yield return new WaitForSeconds (0.5f);
    }

    /*void Update()
    {
        CountdownAndShoot();
    }
    void CountdownAndShoot()
    {
        if (state == EnemyState.Attacking)
            return;

        shootCounter -= Time.deltaTime;

        if (shootCounter <= 0)
        {

            Fire();
            shootCounter = Random.Range(MinTimeBetweenShoots, MaxTimeBetweenShoots);
        }
    }

    void Fire()
    {
        var laser = Instantiate(Laser, transform.position, Quaternion.identity);
        var direction = Vector2.down;

        if (Tier != EnemyTier.Weak && player != null)
        {
            direction = (player.transform.position - transform.position).normalized;
            float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            laser.transform.rotation = Quaternion.Euler(0f, 0f, rotation - 90);
        }

        laser.GetComponent<Rigidbody2D>().velocity = direction * ProjectileSpeed;// new Vector3(0, -ProjectileSpeed);
        AudioSource.PlayClipAtPoint(projectileSound, Camera.main.transform.position, projectileSoundVolume);
    }

    public void Evade()
    {
        if (evading == null)
            evading = StartCoroutine(Evade_coroutine());
    }

    IEnumerator Evade_coroutine()
    {
        state = EnemyState.Attacking;

        var pathing = GetComponent<EnemyPathing>();
        pathing.Stop(true);

        var dodgeCorretion = transform.position.x >= player.transform.position.x ? 0.75f : -0.75f;
        var escapePosition = new Vector3(
            transform.position.x + dodgeCorretion,
            transform.position.y,
            0f);

        while (transform.position != escapePosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, escapePosition, EvadeSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        state = EnemyState.Idle;
        pathing.Stop(false);

        yield return new WaitForSeconds(0.5f);

        evading = null;
    }
    */
}
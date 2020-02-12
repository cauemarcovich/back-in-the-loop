using System.Collections;
using System.Linq;
using UnityEngine;

public class Rex : Boss {
    [Header ("TeD")]
    public SuperShootAbility TED;

    [Header ("Bullet Wall")]
    public SuperShootAbility BULLET_WALL;

    new void Start () {
        base.Start ();

        ConfigureAbilities ();
        Abilities.ForEach (_ => _.Cooldown.ResetCooldown ());
    }

    /* --- */
    /* TeD */
    /* --- */
    public void TeD () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (TeD_Fire ());
        }
    }
    IEnumerator TeD_Fire () {
        //Move To Center
        yield return new WaitUntil (_pathing.MoveToCenter);

        //Delay
        yield return new WaitForSeconds (TED.GeneralDelay);

        //Fire
        for (int i = 0; i < TED.ShootsPerBurst; i++) {
            LookAtPlayer ();
            var direction = _gun.up.normalized;
            Fire (_gun, direction);
            yield return new WaitForSeconds (TED.TimeBetweenShoots);
        }

        //Delay
        yield return new WaitForSeconds (TED.GeneralDelay);

        //Finish        
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
    }

    /* ---------- */
    /* BulletWall */
    /* ---------- */
    void BulletWall () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (BulletWall_Fire ());
        }
    }
    IEnumerator BulletWall_Fire () {
        //Move To Center
        yield return new WaitUntil (() => _pathing.MoveToCenter ());

        //Delay
        yield return new WaitForSeconds (BULLET_WALL.GeneralDelay);

        //Fire
        var turn = false;
        var angle = 0;

        for (int i = 0; i < BULLET_WALL.ShootsPerBurst; i++) {
            angle = !turn ? Random.Range (-55, -45) : Random.Range (-45, -35);

            for (int r = angle; r <= -angle; r += 20) {
                LookAtBottom ();
                _gun.rotation = _gun.rotation * Quaternion.Euler (0f, 0f, r);
                var direction = _gun.up.normalized;

                Fire (_gun, direction, false);
            }

            AudioSource.PlayClipAtPoint (projectileSound, Camera.main.transform.position, projectileSoundVolume);

            yield return new WaitForSeconds (BULLET_WALL.TimeBetweenShoots);

            turn = !turn;
        }

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
    }

    /* Configure Abilities */
    void ConfigureAbilities () {
        TED.Action = TeD;
        BULLET_WALL.Action = BulletWall;

        Abilities.Add (TED);
        Abilities.Add (BULLET_WALL);
    }
}
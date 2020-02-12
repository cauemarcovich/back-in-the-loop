using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;

public class Alastor : Boss {
    // public bool setInvisible = true;

    [Header ("Spin Chase")]
    public SpinChaseAbility SPIN_CHASE;
    public MorphAbility SPIN_CHASE_MORPH;
    float initialSpeed;

    [Header ("Invisibility")]
    public Invisibility INVISIBILITY;
    float initialHealth;

    [Header ("BulletHell")]
    public SuperShootAbility BULLET_HELL;

    new void Start () {
        base.Start ();

        ConfigureAbilities ();
        Abilities.ForEach (_ => _.Cooldown.ResetCooldown ());

        initialSpeed = _pathing.WaveConfig.MoveSpeed;
        initialHealth = Health;
    }

    /* --------- */
    /* SpinChase */
    /* --------- */
    void SpinChase () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            if (initialHealth * .5 >= Health) {
                SPIN_CHASE.InvisibleChase = true;
            }
            StartCoroutine (SpinChase_Exec ());
        }
    }
    IEnumerator SpinChase_Exec () {
        //Move To Center
        yield return new WaitUntil (_pathing.MoveToCenter);
        yield return new WaitForSeconds (SPIN_CHASE.GeneralDelay);

        //Activate Shield
        yield return SpinChase_ShieldActivate ();
        yield return new WaitForSeconds (SPIN_CHASE.GeneralDelay);

        //Set Invisibility

        if (SPIN_CHASE.InvisibleChase)
            yield return SetInvisibility ();

        //Rotate
        yield return new WaitUntil (() => SPIN_CHASE.MaximizeSpinSpeed ());
        yield return new WaitForSeconds (SPIN_CHASE.GeneralDelay);

        //Chase
        SPIN_CHASE.ChaseDurationCounter = SPIN_CHASE.ChaseDuration;
        var oldSpeed = _pathing.WaveConfig.MoveSpeed;
        while (SPIN_CHASE.ChaseDurationCounter >= 0) {
            if (_player == null) break;

            var playerPos = _player.transform.position;
            yield return new WaitUntil (() => _pathing.MoveToPosition (playerPos));

            yield return new WaitForSeconds (SPIN_CHASE.GeneralDelay);

            SPIN_CHASE.ChaseDurationCounter -= Time.deltaTime + SPIN_CHASE.GeneralDelay;

            GetComponent<EnemyPathing> ().WaveConfig.MoveSpeed *= 1.5f;
        }

        _pathing.WaveConfig.MoveSpeed = oldSpeed;

        //Move To Center
        yield return new WaitUntil (_pathing.MoveToCenter);

        //Set Invisibility Off
        if (SPIN_CHASE.InvisibleChase)
            yield return SetInvisibility (false);

        //Rotate Off
        yield return new WaitUntil (() => SPIN_CHASE.MinimizeSpinSpeed ());

        //Correction
        var rotCorretion = 360 * (transform.rotation.z.IsBetween (0f, 180f) ? 1f : -1);
        yield return new WaitUntil (() => {
            transform.Rotate (Vector3.forward * Time.deltaTime * rotCorretion);
            return transform.rotation.z.IsBetween (-0.1f, 0.1f);
        });
        var rot = transform.rotation;
        rot.z = 0f;
        transform.rotation = rot;

        //Deactivate Shield
        yield return SpinChase_ShieldDeactivate ();
        yield return new WaitForSeconds (SPIN_CHASE.GeneralDelay);

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
        _pathing.WaveConfig.MoveSpeed = initialSpeed;
    }
    IEnumerator SpinChase_ShieldActivate () {
        //FirstMove
        var leftWingDest = _leftWing.localPosition + new Vector3 (-0.075f, -0.1f);
        var rightWingDest = _rightWing.localPosition + new Vector3 (0.075f, -0.1f);

        yield return Morph_Exec_Movement (_leftWing, _rightWing, leftWingDest, rightWingDest);

        //SecondMove
        leftWingDest = _leftWing.localPosition + new Vector3 (0f, -0.07f);
        rightWingDest = _rightWing.localPosition + new Vector3 (0f, -0.07f);

        yield return Morph_Exec_Movement (_leftWing, _rightWing, leftWingDest, rightWingDest);
    }
    IEnumerator SpinChase_ShieldDeactivate () {
        //FirstMove
        var leftWingDest = _leftWing.localPosition + new Vector3 (0f, 0.07f);
        var rightWingDest = _rightWing.localPosition + new Vector3 (0f, 0.07f);

        yield return Morph_Exec_Movement (_leftWing, _rightWing, leftWingDest, rightWingDest);
        //SecondMove
        leftWingDest = _leftWing.localPosition + new Vector3 (0.075f, 0.1f);
        rightWingDest = _rightWing.localPosition + new Vector3 (-0.075f, 0.1f);

        yield return Morph_Exec_Movement (_leftWing, _rightWing, leftWingDest, rightWingDest);
    }

    IEnumerator Morph_Exec_Movement (Transform _leftWing, Transform _rightWing, Vector3 leftWingDest, Vector3 rightWingDest) {
        yield return new WaitUntil (() => {
            var result = false;
            result = _pathing.MoveChildToPosition (_leftWing, leftWingDest, SPIN_CHASE_MORPH.MorphSpeedRatio);
            result = _pathing.MoveChildToPosition (_rightWing, rightWingDest, SPIN_CHASE_MORPH.MorphSpeedRatio);
            return result;
        });
    }

    /* ------------ */
    /* Invisibility */
    /* ------------ */
    IEnumerator SetInvisibility (bool setInvisible = true) {
        if (INVISIBILITY.VanishTime == 0) yield return 0;

        var sprites = new List<SpriteRenderer> ();
        foreach (Transform child in transform)
            sprites.Add (child.GetComponent<SpriteRenderer> ());

        if (setInvisible) {
            yield return new WaitUntil (() => Hidden (sprites, INVISIBILITY.VanishTime * Time.deltaTime, INVISIBILITY.MinimumAlphaVisibility));
        } else {
            yield return new WaitUntil (() => Show (sprites, INVISIBILITY.VanishTime * Time.deltaTime));
        }
    }

    /* ----------- */
    /* Bullet Hell */
    /* ----------- */
    void BulletHell () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (BulletHell_Exec ());
        }
    }
    IEnumerator BulletHell_Exec () {
        var oldGunPosition = _gun;

        //Move To Center
        yield return new WaitUntil (_pathing.MoveToCenter);
        yield return new WaitForSeconds (BULLET_HELL.GeneralDelay);

        //Activate Shield
        yield return SpinChase_ShieldActivate ();
        yield return new WaitForSeconds (BULLET_HELL.GeneralDelay);

        //Rotate
        yield return new WaitUntil (() => SPIN_CHASE.MaximizeSpinSpeed ());
        yield return new WaitForSeconds (BULLET_HELL.GeneralDelay);

        //Fire
        var angle = 0f;
        _gun.position = new Vector3 (gameObject.transform.position.x, gameObject.transform.position.y, oldGunPosition.position.z);

        for (int i = 0; i < BULLET_HELL.ShootsPerBurst; i++) {
            angle = Random.Range (0.0f, 45.0f);

            for (int r = 0; r < 18; r++) {
                LookAtBottom ();
                _gun.rotation = _gun.rotation * Quaternion.Euler (0f, 0f, r * 20 + angle);
                var direction = _gun.up;

                Fire (_gun, direction, false);
            }

            AudioSource.PlayClipAtPoint (projectileSound, Camera.main.transform.position, projectileSoundVolume);

            yield return new WaitForSeconds (BULLET_HELL.TimeBetweenShoots);
        }

        //Rotate Off
        yield return new WaitUntil (() => SPIN_CHASE.MinimizeSpinSpeed ());

        //Correction
        var rotCorretion = 360 * (transform.rotation.z.IsBetween (0f, 180f) ? 1f : -1);
        yield return new WaitUntil (() => {
            transform.Rotate (Vector3.forward * Time.deltaTime * rotCorretion);
            return transform.rotation.z.IsBetween (-0.1f, 0.1f);
        });
        var rot = transform.rotation;
        rot.z = 0f;
        transform.rotation = rot;

        //Deactivate Shield
        yield return SpinChase_ShieldDeactivate ();
        yield return new WaitForSeconds (BULLET_HELL.GeneralDelay);

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
    }

    public void ConfigureAbilities () {
        SPIN_CHASE.SetSpinner (GetComponent<Spinner> ());
        SPIN_CHASE.Action = SpinChase;
        Abilities.Add (SPIN_CHASE);

        BULLET_HELL.Action = BulletHell;
        Abilities.Add (BULLET_HELL);
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Diablo : Boss {
    Transform _leftGun;
    Transform _rightGun;
    Transform _baseGun;

    [Header ("Bullet Wall")]
    public SuperShootAbility BULLET_WALL;

    [Header ("Morph")]
    public MorphAbility MORPH;
    public float SpeedMod;
    int morphLevel = 0;
    float initialHealth;

    [Header ("Invisibility")]
    public Invisibility INVISIBILITY;
    public float InvisibleDuration;
    bool invisibilityEnabled;

    [Header ("Vamp Ball")]
    public BigShootAbility VAMP_BALL;

    [Header ("Gravity")]
    public BigShootAbility GRAVITY;
    // public float gr_MinCooldown;
    // public float gr_MaxCooldown;
    // public float gr_Delay;
    // public GameObject gr_object;
    // public float gr_MaxSize;
    // public float gr_Speed;
    // float _gr_Cooldown;

    new void Start () {
        base.Start ();

        ConfigureAbilities ();
        Abilities.ForEach (_ => _.Cooldown.ResetCooldown ());
        _gun = transform.Find ("BigCannon/_gun");
        _baseGun = transform.Find ("BigCannon");
        _leftGun = transform.Find ("Weapon_Left/_gun");
        _rightGun = transform.Find ("Weapon_Right/_gun");

        initialHealth = Health;
    }

    void FixedUpdate () {
        if (Health <= initialHealth * .5 && morphLevel == 0)
            _sm.SetState (Morph);
        if (Health <= initialHealth * .2 && morphLevel == 1)
            _sm.SetState (Morph);

        if (Health <= initialHealth * .5 && !invisibilityEnabled)
            Invisibility ();
    }

    /* -------------------- */
    /* Basic Gun - Override */
    /* -------------------- */
    void NewGun () {
        StartCoroutine (this.NewGun_Fire ());
        _sm.SetState (Idle);
    }
    IEnumerator NewGun_Fire () {
        for (int i = 0; i < BASIC_GUN.ShootsPerBurst; i++) {
            LookAtPlayer ();

            var direction = _gun.up.normalized;
            Fire (_leftGun, direction);
            Fire (_rightGun, direction);

            yield return new WaitForSeconds (BASIC_GUN.TimeBetweenShoots);
        }
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
                _leftGun.rotation = _leftGun.rotation * Quaternion.Euler (0f, 0f, r);
                var leftGunDirection = _leftGun.up.normalized;

                _rightGun.rotation = _rightGun.rotation * Quaternion.Euler (0f, 0f, r);
                var rightGunDirection = _rightGun.up.normalized;

                Fire (_leftGun, leftGunDirection, false);
                Fire (_rightGun, rightGunDirection, false);
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

    /* ----- */
    /* Morph */
    /* ----- */
    void Morph () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (Morph_Exec ());
        }
    }
    IEnumerator Morph_Exec () {
        morphLevel++;

        var middleCollider = GetComponent<PolygonCollider2D> ();
        var leftWingCollider = transform.Find("Wing_Left").GetComponent<PolygonCollider2D> ();
        var rightWingCollider = transform.Find("Wing_Right").GetComponent<PolygonCollider2D> ();

        middleCollider.enabled = false;
        leftWingCollider.enabled = false;
        rightWingCollider.enabled = false;

        //Preparation
        var leftWingDest = Vector3.zero;
        var rightWingDest = Vector3.zero;

        var childMaterials = new List<Material> () {
            transform.Find ("Base").GetComponent<SpriteRenderer> ().material,
            transform.Find ("Wing_Left").GetComponent<SpriteRenderer> ().material,
            transform.Find ("Wing_Right").GetComponent<SpriteRenderer> ().material,
        };

        //Move To Center
        yield return new WaitUntil (_pathing.MoveToCenter);

        yield return new WaitForSeconds (MORPH.GeneralDelay);

        //ShineOn
        yield return new WaitUntil (() => ChangeAllColor (childMaterials, MORPH.MorphAlphaVariation, MORPH.MaxAlphaValue));

        //FirstMovement
        leftWingDest = _leftWing.localPosition + new Vector3 (-0.15f, 0f);
        rightWingDest = _rightWing.localPosition + new Vector3 (0.15f, 0f);

        yield return new WaitUntil (() => {
            var result = false;
            result = _pathing.MoveChildToPosition (_leftWing, leftWingDest, MORPH.MorphSpeedRatio);
            result = _pathing.MoveChildToPosition (_rightWing, rightWingDest, MORPH.MorphSpeedRatio) && result;
            return result;
        });

        //ShineOff
        yield return new WaitUntil (() => ChangeAllColor (childMaterials, -MORPH.MorphAlphaVariation, MORPH.MinAlphaValue));

        //Boost
        _pathing.MoveSpeedModifier = SpeedMod;
        yield return new WaitForSeconds (MORPH.GeneralDelay);

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);

        middleCollider.enabled = true;
        leftWingCollider.enabled = true;
        rightWingCollider.enabled = true;
    }

    /* ------------ */
    /* Invisibility */
    /* ------------ */
    public void Invisibility () {
        invisibilityEnabled = true;
        StartCoroutine (SetInvisibility ());
    }
    IEnumerator SetInvisibility () {
        if (INVISIBILITY.VanishTime == 0) yield return 0;

        var sprites = new List<SpriteRenderer> ();
        foreach (Transform child in transform)
            sprites.Add (child.GetComponent<SpriteRenderer> ());

        while (true) {
            yield return new WaitUntil (() => Hidden (sprites, INVISIBILITY.VanishTime * Time.deltaTime, INVISIBILITY.MinimumAlphaVisibility));

            yield return new WaitForSeconds (InvisibleDuration);

            yield return new WaitUntil (() => Show (sprites, INVISIBILITY.VanishTime * Time.deltaTime));

            yield return new WaitForSeconds (Random.Range (INVISIBILITY.Cooldown.min_cooldown, INVISIBILITY.Cooldown.max_cooldown));
        }
    }

    /* --------- */
    /* Vamp Ball */
    /* --------- */
    void VampBall () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (VampBall_Exec ());
        }
    }
    IEnumerator VampBall_Exec () {
        yield return new WaitUntil (_pathing.MoveToCenter);

        yield return new WaitForSeconds (VAMP_BALL.GeneralDelay);

        //put cannon to out
        var _baseGunDest = _baseGun.transform.localPosition + new Vector3 (0f, -0.3f);
        yield return new WaitUntil (() => _pathing.MoveChildToPosition (_baseGun, _baseGunDest, 10f));

        yield return new WaitForSeconds (VAMP_BALL.GeneralDelay);

        //create shoot
        var vampBall = Instantiate (VAMP_BALL.Projectile, _gun.transform.position + (new Vector3 (0, 0, 6)), Quaternion.identity);
        VAMP_BALL.SetShoot (vampBall);

        //Increase Ball
        yield return new WaitUntil (() => VAMP_BALL.IncreaseShoot ());
        yield return new WaitForSeconds (VAMP_BALL.GeneralDelay);

        LookAtPlayer ();
        vampBall.GetComponent<Rigidbody2D> ().velocity = _gun.up.normalized * ProjectileSpeed;

        yield return new WaitUntil (() => vampBall == null);

        _baseGunDest = _baseGun.transform.localPosition + new Vector3 (0f, 0.3f);

        yield return new WaitUntil (() => _pathing.MoveChildToPosition (_baseGun, _baseGunDest, 10f));

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
    }

    /* ------- */
    /* Gravity */
    /* ------- */
    void Gravity () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (Gravity_Exec ());
        }
    }
    IEnumerator Gravity_Exec () {
        yield return new WaitUntil (_pathing.MoveToCenter);

        yield return new WaitForSeconds (GRAVITY.GeneralDelay);

        var gravityField = Instantiate (GRAVITY.Projectile, transform.position + (new Vector3 (0, 0, 6)), Quaternion.identity);
        GRAVITY.SetShoot (gravityField);

        yield return new WaitUntil (() => GRAVITY.IncreaseShoot ());
        yield return new WaitForSeconds (GRAVITY.GeneralDelay);

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
    }

    void ConfigureAbilities () {
        BULLET_WALL.Action = BulletWall;
        MORPH.Action = Morph;
        VAMP_BALL.Action = VampBall;
        GRAVITY.Action = Gravity;

        Abilities[0].Action = NewGun;
        Abilities.Add (BULLET_WALL);
        Abilities.Add (MORPH);
        Abilities.Add (VAMP_BALL);
        Abilities.Add (GRAVITY);
    }

}
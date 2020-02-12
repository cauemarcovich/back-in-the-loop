using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using UnityEngine;

public class Mummy : Boss {    
    float initialHealth;
    public AudioClip Shield_Sound;

    [Header ("Wave Burst")]
    public SuperShootAbility WAVE_BURST;
    public int QuantityOfTurns;

    [Header ("Morph")]
    public MorphAbility MORPH;
    public float SpeedMod;
    bool _morphActive;

    new void Start () {
        base.Start ();

        _gun = transform.Find ("_gun");

        initialHealth = Health;

        ConfigureAbilities ();
        Abilities.ForEach (_ => _.Cooldown.ResetCooldown ());
    }

    /* Colliders */
    new void OnTriggerEnter2D (Collider2D other) {
        var centerCollider = GetComponent<BoxCollider2D> ();
        
        if (centerCollider.IsTouching (other)) {
            base.OnTriggerEnter2D (other);
        } else {
            AudioSource.PlayClipAtPoint (Shield_Sound, Camera.main.transform.position, deathSoundVolume);
            Destroy (other.gameObject);
        }
    }

    void FixedUpdate () {
        if (Health <= initialHealth / 2 && !_morphActive) {
            _sm.SetState (Morph);
        }
    }

    /* ---------- */
    /* WaveBurst */
    /* ---------- */
    void WaveBurst () {
        _pathing.Stop (true);
        if (_isIdle) {
            _isIdle = false;
            StartCoroutine (WaveBurst_Exec ());
        }
    }
    IEnumerator WaveBurst_Exec () {
        //Move To Center
        var initPosition = Camera.main.ViewportToWorldPoint (new Vector3 (0.1f, 0.75f));
        initPosition.z = transform.position.z;
        yield return new WaitUntil (() => _pathing.MoveToPosition (initPosition));

        //Delay
        yield return new WaitForSeconds (WAVE_BURST.GeneralDelay);

        //Execute
        var destination = transform.position;

        var coroutine = StartCoroutine (WaveBurst_Fire ());

        var turns = 1;
        while (turns++ <= QuantityOfTurns) {
            destination.x = -destination.x;
            yield return new WaitUntil (() => _pathing.MoveToPosition (destination));
        }
        QuantityOfTurns++;

        StopCoroutine (coroutine);

        yield return new WaitForSeconds (WAVE_BURST.GeneralDelay);

        //Finish
        _pathing.Stop (false);
        _isIdle = true;
        _sm.SetState (Idle);
    }
    IEnumerator WaveBurst_Fire () {
        LookAtBottom ();
        while (true) {
            Fire (_gun, _gun.up.normalized);
            yield return new WaitForSeconds (WAVE_BURST.TimeBetweenShoots);
        }
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
        var colliders = new PolygonCollider2D[2];
        colliders[0] = transform.Find ("Wing_Left").GetComponent<PolygonCollider2D> ();
        colliders[1] = transform.Find ("Wing_Right").GetComponent<PolygonCollider2D> ();
        var boxCollider = GetComponent<BoxCollider2D> ();

        _morphActive = true;
        colliders[0].enabled = false;
        colliders[1].enabled = false;
        boxCollider.enabled = false;

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
        leftWingDest = _leftWing.localPosition + new Vector3 (0f, 0.225f);
        rightWingDest = _rightWing.localPosition + new Vector3 (0f, 0.225f);

        yield return new WaitUntil (() => {
            var result = false;
            result = _pathing.MoveChildToPosition (_leftWing, leftWingDest, MORPH.MorphSpeedRatio);
            result = _pathing.MoveChildToPosition (_rightWing, rightWingDest, MORPH.MorphSpeedRatio) && result;
            return result;
        });

        //SecondMovement
        leftWingDest = _leftWing.localPosition + new Vector3 (-0.1f, 0.125f);
        rightWingDest = _rightWing.localPosition + new Vector3 (0.1f, 0.125f);

        yield return new WaitUntil (() => {
            var result = false;
            result = _pathing.MoveChildToPosition (_leftWing, leftWingDest, MORPH.MorphSpeedRatio);
            result = _pathing.MoveChildToPosition (_rightWing, rightWingDest, MORPH.MorphSpeedRatio);
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
        colliders[0].enabled = true;
        colliders[1].enabled = true;
        boxCollider.enabled = true;
    }

    /* Configure Abilities */
    void ConfigureAbilities () {
        WAVE_BURST.Action = WaveBurst;
        MORPH.Action = Morph;

        Abilities.Add (WAVE_BURST);
        Abilities.Add (MORPH);
    }
}
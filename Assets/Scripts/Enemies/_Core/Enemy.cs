using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (EnemyFSM))]
[RequireComponent (typeof (EnemyPathing))]
[RequireComponent (typeof (Flasher))]
public class Enemy : MonoBehaviour {
    [Header ("Ship")]
    public float Health = 500;
    protected float Max_Health;
    public EnemyTier Tier = EnemyTier.Normal;

    [Header ("Death")]
    public GameObject DeathVFX;
    public float durationOfExplosion = 0.5f;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range (0, 1)] public float deathSoundVolume = 0.75f;
    float hurtSoundDelay = 0.5f;
    bool dead = false;

    [Header ("Projectile")]
    public GameObject Laser;
    public float ProjectileSpeed;
    public AudioClip projectileSound;
    [Range (0, 1)] public float projectileSoundVolume = 0.75f;
    public float MinTimeBetweenShoots = 0.2f;
    public float MaxTimeBetweenShoots = 3f;

    protected float _moveCounter = 0.5f;
    protected GameObject _player;
    protected EnemyFSM _sm;
    protected EnemyPathing _pathing;
    protected Flasher _flasher;

    protected void Start () {
        SetRandomShootCounter ();
        _player = GameObject.FindGameObjectsWithTag ("Player").Where (_ => _.name == "Player").FirstOrDefault ();
        _sm = GetComponent<EnemyFSM> ();
        _pathing = GetComponent<EnemyPathing> ();
        _flasher = GetComponent<Flasher> ();

        Max_Health = Health;
    }

    void FixedUpdate () { hurtSoundDelay -= Time.deltaTime; }

    protected void OnTriggerEnter2D (Collider2D other) {
        var damageDealer = other.GetComponent<DamageDealer> ();
        
        if (damageDealer != null) {
            ProcessHit (damageDealer);
            _flasher.Flash ();
        }
    }

    void ProcessHit (DamageDealer damageDealer) {
        Health -= damageDealer.Damage;
        damageDealer.Hit ();

        if (hurtSoundDelay <= 0f) {
            AudioSource.PlayClipAtPoint (hurtSound, Camera.main.transform.position, deathSoundVolume);
            hurtSoundDelay = 0f;
        }

        if (Health <= 0 && !dead) {
            dead = true;
            Die ();
        }
    }
    void Die () {
        Debug.Log ("Morreu: " + gameObject.name);
        DropItem ();

        if (gameObject.tag == "Boss") {
            var stageController = GameObject.Find ("StageController").GetComponent<StageController> ();
            stageController.MoveToNextLevel ();

            if (gameObject.name == "Diablo")
                foreach (var vb in GameObject.FindGameObjectsWithTag ("Bomb"))
                    Destroy (vb);

            GameObject.Find ("UI").GetComponent<BossHUD> ().DisableBossHUD ();
        }

        if (DeathVFX != null) {
            var explosion = Instantiate (DeathVFX, transform.position, transform.rotation);
            Destroy (explosion, durationOfExplosion);
        }
        Destroy (gameObject);
        if (deathSound != null) {
            AudioSource.PlayClipAtPoint (deathSound, Camera.main.transform.position, deathSoundVolume);
        }
    }
    void DropItem () {
        var pu_spawner = GameObject.Find ("PowerUpSpawner").GetComponent<PowerUpSpawner> ();
        var en_spawner = GameObject.Find ("EnergySpawner").GetComponent<EnergySpawner> ();

        if (gameObject.tag == "Boss") {
            for (int i = 0; i < 3; i++) {
                var variation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                en_spawner.Spawn (transform.position + variation);
            }
            for (int i = 0; i < 3; i++) {
                var variation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                pu_spawner.Spawn (transform.position + variation);
            }
        } else {
            if (Random.Range (0, 100) <= pu_spawner.dropRate) {
                pu_spawner.Spawn (transform.position);
            }
            if (Random.Range (0, 100) <= en_spawner.droprate) {
                en_spawner.Spawn (transform.position);
            }
        }
    }

    protected void SetRandomShootCounter () {
        _moveCounter = Random.Range (MinTimeBetweenShoots, MaxTimeBetweenShoots);
    }

    protected bool ChangeAllColor (List<Material> materials, float variation, float clampValue) {
        var result = true;

        materials.ForEach (m => {
            var newColorValue = ChangeColor (m, variation);

            if (variation >= 0)
                result = newColorValue.r >= clampValue && result;
            else {
                result = newColorValue.r <= clampValue && result;
            }
        });

        return result;
    }
    protected Color ChangeColor (Material material, float variation) {
        var r = material.GetColor ("_Color").r + variation;
        var g = material.GetColor ("_Color").g + variation;
        var b = material.GetColor ("_Color").b + variation;

        var newColor = new Color (r, g, b, 1);
        material.SetColor ("_Color", newColor);
        return newColor;
    }
    protected bool Show (List<SpriteRenderer> sprites, float variation) {
        var result = true;
        sprites.ForEach (sp => {
            var color = ChangeAlpha (sp, variation);
            result = color.a >= 1;
        });
        return result;
    }
    protected bool Hidden (List<SpriteRenderer> sprites, float variation, float minimumAlpha) {
        var result = true;
        var minAlpha = minimumAlpha / 255f;
        sprites.ForEach (sp => {
            var color = ChangeAlpha (sp, -variation, minAlpha);
            result = color.a <= minAlpha;
        });
        return result;
    }
    protected Color ChangeAlpha (SpriteRenderer sprite, float variation, float minimumAlpha = 0f) {
        var newAlpha = Mathf.Clamp (sprite.color.a + variation, minimumAlpha / 255f, 1f);
        sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);

        return sprite.color;
    }

    public void EnableColliders (bool enable) {
        var polygonCollider = GetComponent<PolygonCollider2D> ();
        if (polygonCollider != null)
            polygonCollider.enabled = enable;

        var boxCollider = GetComponent<BoxCollider2D> ();
        if (boxCollider != null)
            boxCollider.enabled = enable;

        if (gameObject.name.Contains ("Alastor") || gameObject.name.Contains ("Diablo"))
            foreach (Transform child in gameObject.transform) {
                var collider = child.GetComponent<Collider2D> ();
                if (collider != null)
                    collider.enabled = enable;
            }
    }
}
public enum EnemyTier {
    Weak = 1,
    Normal = 2,
    Strong = 3
}
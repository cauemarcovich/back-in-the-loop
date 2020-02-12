using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header ("Ship")]
    public float speed;
    public float max_health = 100f;
    [SerializeField] float health;

    [Header ("Death")]
    public GameObject deathVFX;
    public float durationOfExplosion = 0.5f;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range (0, 1)] public float deathSoundVolume = 0.75f;
    float hurtSoundDelay = 0.5f;

    Boundary _edges;
    float padding = 0.5f;

    Coroutine firingCoroutine;
    ShootController shootController;
    float shootTimer = 0f;
    PlayerHUD UI;
    protected Flasher _flasher;
    StageController _stageController;
    CameraShake _cameraShake;

    public bool _lockMove = true;
    bool gravityEffect;
    bool bossDamaged = false;

    void Start () {
        UI = GameObject.Find ("UI").GetComponent<PlayerHUD> ();

        health = max_health;
        shootController = GetComponent<ShootController> ();

        _flasher = GetComponent<Flasher> ();

        var sc = GameObject.Find ("StageController");
        if (sc != null)
            _stageController = sc.GetComponent<StageController> ();

        _cameraShake = Camera.main.gameObject.GetComponent<CameraShake> ();

        SetBoundaries ();
    }
    #region boundaries
    void SetBoundaries () {
        var camera = Camera.main;
        var downLeft = camera.ViewportToWorldPoint (new Vector3 (0f, 0f, 0f));
        var upRight = camera.ViewportToWorldPoint (new Vector3 (1f, 0.5f, 0f));

        _edges = new Boundary ();
        _edges.Left = downLeft.x + padding;
        _edges.Down = downLeft.y + padding;
        _edges.Right = upRight.x - padding;
        _edges.Up = upRight.y - padding;
    }
    private class Boundary {
        public float Up;
        public float Down;
        public float Left;
        public float Right;
    }
    #endregion

    void FixedUpdate () {
        if (_lockMove) return;

        hurtSoundDelay -= Time.deltaTime;

        Move ();
        Fire ();
    }

    void Move () {
        var horizontal = Input.GetAxisRaw ("Horizontal");
        var vertical = Input.GetAxisRaw ("Vertical");

        var gravityVariation = gravityEffect? .3f : 1f;
        var new_x = transform.position.x + (horizontal * Time.deltaTime * speed * gravityVariation);
        var new_y = transform.position.y + (vertical * Time.deltaTime * speed * gravityVariation);

        var newPos = new Vector3 (
            Mathf.Clamp (new_x, _edges.Left, _edges.Right),
            Mathf.Clamp (new_y, _edges.Down, _edges.Up),
            0f
        );

        transform.position = newPos;
    }

    void Fire () {

        if (Input.GetButton ("Fire1") && shootTimer >= shootController.GetCurrentFireRate ()) {
            firingCoroutine = StartCoroutine (shootController.FireRepeater ());
            shootTimer = 0f;
            StopAllCoroutines ();
        }
        if (Input.GetButtonUp ("Fire1")) {
            if (firingCoroutine != null) StopAllCoroutines ();
        }
        shootTimer += Time.deltaTime;
    }

    void OnTriggerEnter2D (Collider2D collider) {
        var damageDealer = collider.GetComponent<DamageDealer> ();
        if (damageDealer != null) {
            _flasher.Flash ();
            ProcessHit (damageDealer);
        }

        if (collider.tag == "Gravity") {
            gravityEffect = true;
        }
    }
    void ProcessHit (DamageDealer damageDealer) {
        if (bossDamaged) return;

        if (damageDealer.gameObject.tag == "Boss") {
            bossDamaged = true;
            var totalDamage = max_health * (damageDealer.Damage / 100f);
            ChangeHealth (-totalDamage);
        } else {
            damageDealer.Hit ();
            ChangeHealth (-damageDealer.Damage);
        }

        if (hurtSoundDelay <= 0f) {
            AudioSource.PlayClipAtPoint (hurtSound, Camera.main.transform.position, deathSoundVolume);
            hurtSoundDelay = 0f;
        } else hurtSoundDelay -= Time.deltaTime;
        _cameraShake.Shake (0.2f, 0.075f);

        if (health <= 0) {
            _cameraShake.Shake (0.5f, 0.3f);
            Die ();
        }
    }
    IEnumerator Hit () {
        var renderer = GetComponent<SpriteRenderer> ();
        renderer.color = Color.white;
        yield return 0;
    }
    void Die () {
        var explosion = Instantiate (deathVFX, transform.position, transform.rotation);
        Destroy (explosion, durationOfExplosion);
        Destroy (gameObject);
        AudioSource.PlayClipAtPoint (deathSound, Camera.main.transform.position, deathSoundVolume);

        _stageController.GameOverEvent ();
    }
    void OnTriggerExit2D (Collider2D other) { gravityEffect = false; bossDamaged = false; }

    public void ChangeHealth (float amount) {
        health = Mathf.Clamp (health + amount, 0, max_health);
        UI.UpdateLifeBar ();
    }
    public float GetHealth () {
        return health;
    }
}
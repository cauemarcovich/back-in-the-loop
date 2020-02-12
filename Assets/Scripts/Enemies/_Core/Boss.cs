using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss : Enemy {
	protected Transform _gun;
	protected Transform _leftWing;
	protected Transform _rightWing;

	[Header ("HUD")]
	public string BossName;
	public Color Color;
	BossHUD _bossHUD;

	public List<Ability> Abilities;
	protected bool _isIdle;

	[Header ("Basic Gun")]
	public BasicShootAbility BASIC_GUN;

	protected new void Start () {
		base.Start ();

		_gun = transform.Find ("_gun");
		_leftWing = transform.Find ("Wing_Left");
		_rightWing = transform.Find ("Wing_Right");

		ConfigureAbility ();

		_isIdle = true;
		_sm.SetState (Idle);

		_bossHUD = GameObject.Find ("UI").GetComponent<BossHUD> ();
		_bossHUD.EnableBossHUD (BossName, Color);
	}

	protected new void OnTriggerEnter2D (Collider2D other) {
		base.OnTriggerEnter2D (other);

		_bossHUD.UpdateLifeBar (Health, Max_Health);
	}

	protected void Idle () {
		if (_pathing.LockOnFirst) return;

		ReduceCooldowns ();

		foreach (var ability in Abilities) {
			if (ability.Cooldown._cooldown < 0) {
				ability.Cooldown.ResetCooldown ();
				if (!ability.IgnoreCooldownCorrection) CooldownCorrection ();
				_sm.SetState (ability.Action);
			}
		}
	}

	/* Basic Gun */
	#region Basic_Gun
	void Gun () {
		StartCoroutine (Gun_Fire ());
		_sm.SetState (Idle);
	}
	IEnumerator Gun_Fire () {
		for (int i = 0; i < BASIC_GUN.ShootsPerBurst; i++) {
			LookAtPlayer ();

			var direction = _gun.up.normalized;
			Fire (_gun, direction);

			yield return new WaitForSeconds (BASIC_GUN.TimeBetweenShoots);
		}
	}

	protected void Fire (Transform gun, Vector3 direction, bool enableAudio = true) {
		var laser = Instantiate (Laser, gun.position, gun.rotation);
		laser.GetComponent<Rigidbody2D> ().velocity = direction * ProjectileSpeed;

		if (enableAudio)
			AudioSource.PlayClipAtPoint (projectileSound, Camera.main.transform.position, projectileSoundVolume);
	}
	#endregion

	/* Cooldown */
	#region Cooldowns
	protected void ReduceCooldowns () {
		Abilities.ForEach (_ => _.Cooldown._cooldown -= Time.deltaTime);
	}
	protected void CooldownCorrection () {
		Abilities.ForEach (_ => {
			if (_.Cooldown._cooldown <= 4f)
				_.Cooldown._cooldown += _.Cooldown.max_cooldown * 0.25f;
		});
	}
	#endregion

	/* Look At Target */
	#region LookAt
	protected void LookAtPlayer () {
		if (_player == null) return;

		var diff = (_player.transform.position - transform.position);
		diff.Normalize ();

		LookAt (diff);
	}
	protected void LookAtBottom () {
		LookAt (Vector3.down);
	}
	void LookAt (Vector3 target) {
		float rot_z = Mathf.Atan2 (target.y, target.x) * Mathf.Rad2Deg;

		List<Transform> guns = GetListOfGuns (transform);
		guns.ForEach (_ =>
			_.rotation = Quaternion.Euler (0f, 0f, rot_z - 90)
		);
	}
	List<Transform> GetListOfGuns (Transform _transform) {

		var result = new List<Transform> ();

		foreach (Transform child in _transform) {
			if (child.childCount > 0)
				result.AddRange (GetListOfGuns (child));

			if (child.name == "_gun")
				result.Add (child);
		}

		return result;
	}
	#endregion

	/* Configure Basic Gun */
	void ConfigureAbility () {
		Abilities = new List<Ability> ();

		BASIC_GUN.Action = Gun;
		BASIC_GUN.IgnoreCooldownCorrection = true;

		Abilities.Add (BASIC_GUN);
	}
}

[System.Serializable]
public class Cooldown {
	public float min_cooldown;
	public float max_cooldown;

	//[HideInInspector] public float _cooldown;
	public float _cooldown;

	public void ResetCooldown () {
		_cooldown = Random.Range (min_cooldown, max_cooldown);
	}
}

[System.Serializable]
public abstract class Ability {
	public string Name;
	public System.Action Action;
	public Cooldown Cooldown;

	public float GeneralDelay;

	[HideInInspector] public bool IgnoreCooldownCorrection;
}

[System.Serializable]
public class BasicShootAbility : Ability {
	public int ShootsPerBurst;
	public float TimeBetweenShoots;
}

[System.Serializable]
public class SuperShootAbility : BasicShootAbility { }

[System.Serializable]
public class MorphAbility : Ability {
	public float MorphSpeedRatio;
	public float MorphAlphaVariation;

	[HideInInspector] public float MinAlphaValue = 1f;
	[HideInInspector] public float MaxAlphaValue = 10f;
}

[System.Serializable]
public class SpinChaseAbility : Ability {
	public float MaxSpinSpeed;
	public float MaxSpinSpeedTime;

	public float ChaseDuration;
	[HideInInspector] public float ChaseDurationCounter;

	public bool InvisibleChase;

	Spinner Spinner;

	public void SetSpinner (Spinner spinner) {
		Spinner = spinner;
		Spinner.rotateSpeed = 0f;
	}

	public bool MaximizeSpinSpeed () {
		var variation = (MaxSpinSpeed / MaxSpinSpeedTime) * Time.deltaTime;
		Spinner.rotateSpeed += variation;
		Spinner.rotateSpeed = Mathf.Clamp (Spinner.rotateSpeed, 0f, MaxSpinSpeed);
		return Spinner.rotateSpeed == MaxSpinSpeed;
	}

	public bool MinimizeSpinSpeed () {
		var variation = (MaxSpinSpeed / MaxSpinSpeedTime) * Time.deltaTime;
		Spinner.rotateSpeed -= variation;
		Spinner.rotateSpeed = Mathf.Clamp (Spinner.rotateSpeed, 0f, MaxSpinSpeed);
		return Spinner.rotateSpeed == 0;
	}
}

[System.Serializable]
public class Invisibility : Ability {
	public float VanishTime = 1f;
	public float MinimumAlphaVisibility = 0f;
}

[System.Serializable]
public class BigShootAbility : SuperShootAbility {
	public GameObject Projectile;
	public float IncreaseSpeed;
	public float MaxSize;

	Transform _shoot;
	public void SetShoot (GameObject shoot) {
		_shoot = shoot.transform;
		_shoot.localScale = Vector3.zero;
	}

	public bool IncreaseShoot () {
		var newShootSize = Mathf.Clamp (_shoot.localScale.x + IncreaseSpeed, 0f, MaxSize);
		_shoot.localScale = new Vector2 (newShootSize, newShootSize);

		return newShootSize == MaxSize;
	}
}
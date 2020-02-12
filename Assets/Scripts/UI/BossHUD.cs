using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHUD : MonoBehaviour {
	TextMeshProUGUI _bossName;
	Image _bossEnergyHUDImage;
	RectTransform _bossEnergyHUD;

	Vector2 _maxOriginalSize;

	void Start () {
		_bossName = transform.Find ("Boss/Name").GetComponent<TextMeshProUGUI> ();
		_bossEnergyHUDImage = transform.Find ("Boss/BossHealth/Image").GetComponent<Image> ();
		_bossEnergyHUD = transform.Find ("Boss/BossHealth/Image").GetComponent<RectTransform> ();

		_bossName.color = new Color (_bossName.color.r, _bossName.color.g, _bossName.color.b, 0f);
		_bossEnergyHUDImage.color = new Color (_bossEnergyHUDImage.color.r, _bossEnergyHUDImage.color.g, _bossEnergyHUDImage.color.b, 0f);

		_maxOriginalSize = new Vector2 (_bossEnergyHUD.rect.width, _bossEnergyHUD.rect.height);
	}

	public void EnableBossHUD (string bossName, Color color) {
		_bossName.text = bossName;
		_bossEnergyHUDImage.color = new Color (color.r, color.g, color.b, 0f);
		_bossEnergyHUD.sizeDelta = _maxOriginalSize;

		StartCoroutine (_bossName.GetComponent<FadeController> ().FadeOut (1f));
		StartCoroutine (_bossEnergyHUDImage.GetComponent<FadeController> ().FadeOut (1f));
	}

	public void DisableBossHUD () {
		StartCoroutine (_bossName.GetComponent<FadeController> ().FadeIn (1f));
		StartCoroutine (_bossEnergyHUDImage.GetComponent<FadeController> ().FadeIn (1f));
	}

	public void UpdateLifeBar (float currentHealth, float maxHealth) {
		var healthPercent = currentHealth / maxHealth;

		_bossEnergyHUD.sizeDelta = new Vector2 (_maxOriginalSize.x * healthPercent, _maxOriginalSize.y);
		_bossEnergyHUD.anchoredPosition = new Vector2 (0, 0);
	}
}
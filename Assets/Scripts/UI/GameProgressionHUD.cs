using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameProgressionHUD : MonoBehaviour {
	public Color[] BarColors;
	public float ChangeColorDuration;

	RectTransform _parentProgressionBar;
	RectTransform _progressionBar;
	List<Image> _bossImages;
	Vector2 _maxOriginalSize;

	void Start () {
		_parentProgressionBar = transform.Find ("ProgressionBar/Progression").GetComponent<RectTransform> ();

		_progressionBar = _parentProgressionBar.transform.Find ("Image").GetComponent<RectTransform> ();
		_bossImages = new List<Image> ();
		foreach (Transform child in _parentProgressionBar.parent) {
			if (child.name.Contains ("Boss"))
				_bossImages.Add (child.GetComponent<Image> ());
		}
		_parentProgressionBar.GetComponent<Image> ().color = new Color (BarColors[0].r, BarColors[0].g, BarColors[0].b, 0);
		_progressionBar.GetComponent<Image> ().color = new Color (BarColors[0].r, BarColors[0].g, BarColors[0].b, 0);
		_bossImages.ForEach (_ => _.color = new Color (1, 1, 1, 0));

		_maxOriginalSize = new Vector2 (_parentProgressionBar.rect.width, _parentProgressionBar.rect.height);
	}

	public void EnableProgressionHUD () {
		StartCoroutine (_progressionBar.parent.GetComponent<FadeController> ().FadeOut (1f, 0.15f, true));
		StartCoroutine (_progressionBar.GetComponent<FadeController> ().FadeOut (1f));
	}

	public void DisableProgressionHUD () {
		StartCoroutine (_progressionBar.parent.GetComponent<FadeController> ().FadeIn (1f));
		StartCoroutine (_progressionBar.GetComponent<FadeController> ().FadeIn (1f));
	}

	public void UpdateProgressionBar (float currentHealth, float maxHealth) {
		var healthPercent = currentHealth / maxHealth;

		_progressionBar.sizeDelta = new Vector2 (_maxOriginalSize.x, _maxOriginalSize.y * healthPercent);
		_progressionBar.anchoredPosition = new Vector2 (0, 0);
	}

	public void SetNewProgression (int level) {
		var alpha = 0;
		for (var i = level - 1; i < 4; i++) {
			var oldColor = _bossImages[i].color;
			_bossImages[i].color = new Color (oldColor.r, oldColor.g, oldColor.b, 1 - (0.25f * alpha));
			alpha++;
		}
		
		_bossImages[level - 1].material = null;
		StartCoroutine (ChangeColor (level - 1));
	}

	public IEnumerator ChangeColor (int colorIndex) {
		var progressionImage = _progressionBar.GetComponent<Image> ();
		var parentProgressionImage = _parentProgressionBar.GetComponent<Image> ();
		var color = progressionImage.color;
		var newColor = BarColors[colorIndex];

		for (float i = 0; i <= 1; i += Time.deltaTime / ChangeColorDuration) {
			var new_r = Mathf.Lerp (color.r, newColor.r, i);
			var new_g = Mathf.Lerp (color.g, newColor.g, i);
			var new_b = Mathf.Lerp (color.b, newColor.b, i);

			var progressionColor = new Color (new_r, new_g, new_b, 1);
			progressionImage.color = progressionColor;

			var progressionParentColor = new Color (new_r, new_g, new_b, 0.14f);
			parentProgressionImage.color = progressionParentColor;

			yield return null;
		}
	}
}
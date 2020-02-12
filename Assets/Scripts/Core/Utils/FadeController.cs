using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour {
	public FadeElementType ElementType;
	FadeElementType _elementType;

	System.Action<Color> _setColor;
	System.Func<Color> _getColor;

	SpriteRenderer _spriteRenderer;
	Image _image;
	TextMeshProUGUI _textMeshProUGUI;
	Material _material;

	void _setSpriteRendererColor (Color _color) { _spriteRenderer.color = _color; }
	void _setImageColor (Color _color) { _image.color = _color; }
	void _setTextMeshColor (Color _color) { _textMeshProUGUI.color = _color; }
	void _setMaterialColor (Color _color) { _material.color = _color; }

	Color _getSpriteRendererColor () { return _spriteRenderer.color; }
	Color _getImageColor () { return _image.color; }
	Color _getTextMeshColor () { return _textMeshProUGUI.color; }
	Color _getMaterialColor () { return _material.color; }

	void Start () {
		_elementType = ElementType;

		if (ElementType == FadeElementType.Auto) {
			if (gameObject.HasComponent<SpriteRenderer> ())
				_elementType = FadeElementType.SpriteRenderer;
			else if (gameObject.HasComponent<Image> ())
				_elementType = FadeElementType.Image;
			else if (gameObject.HasComponent<TextMeshProUGUI> ())
				_elementType = FadeElementType.TextMeshProUGUI;

		}

		if (_elementType == FadeElementType.SpriteRenderer) {
			_spriteRenderer = GetComponent<SpriteRenderer> ();
			_setColor = _setSpriteRendererColor;
			_getColor = _getSpriteRendererColor;
		} else if (_elementType == FadeElementType.Image) {
			_image = GetComponent<Image> ();
			_setColor = _setImageColor;
			_getColor = _getImageColor;
		} else if (_elementType == FadeElementType.TextMeshProUGUI) {
			_textMeshProUGUI = GetComponent<TextMeshProUGUI> ();
			_setColor = _setTextMeshColor;
			_getColor = _getTextMeshColor;
		} else if (_elementType == FadeElementType.Material) {
			_material = GetComponent<Renderer> ().sharedMaterial;
			_setColor = _setMaterialColor;
			_getColor = _getMaterialColor;
		} else {
			Debug.LogError ("Element Type not found");
		}
	}

	/* FADE IN */
	public IEnumerator FadeIn (float duration, bool keepObjectEnabled = true) {
		yield return StartCoroutine (Fade (FadeDirection.In, duration, 0f, _getColor ().a));
		gameObject.SetActive (keepObjectEnabled);
	}
	public IEnumerator FadeIn (float duration, float minFadeValue, bool keepObjectEnabled = true) {
		yield return StartCoroutine (Fade (FadeDirection.In, duration, minFadeValue, _getColor ().a));
		gameObject.SetActive (keepObjectEnabled);
	}

	/* FADE OUT */
	public IEnumerator FadeOut (float duration, bool keepObjectEnabled = true) {
		yield return StartCoroutine (Fade (FadeDirection.Out, duration, _getColor ().a, 1f));
		gameObject.SetActive (keepObjectEnabled);
	}
	public IEnumerator FadeOut (float duration, float maxFadeValue, bool keepObjectEnabled = true) {
		gameObject.SetActive(true);
		yield return StartCoroutine (Fade (FadeDirection.Out, duration, _getColor ().a, maxFadeValue));
		gameObject.SetActive (keepObjectEnabled);
	}

	/* FADE */
	IEnumerator Fade (FadeDirection dir, float duration, float minFadeValue, float maxFadeValue) {
		var _color = _getColor ();
		if (dir == FadeDirection.In) {
			for (float i = 1; i >= 0; i -= Time.deltaTime / duration) {
				var newColor = new Color (_color.r, _color.g, _color.b, Mathf.Lerp (minFadeValue, maxFadeValue, i));
				_setColor (newColor);
				yield return null;
			}
			_setColor(new Color(_color.r, _color.g, _color.b, 0f));
		} else {
			for (float i = 0; i <= 1; i += Time.deltaTime / duration) {
				var newColor = new Color (_color.r, _color.g, _color.b, Mathf.Lerp (minFadeValue, maxFadeValue, i));
				_setColor (newColor);
				yield return null;
			}
			_setColor(new Color(_color.r, _color.g, _color.b, 1f));
		}
	}

	/* CROSS FADE */
	public IEnumerator CrossFadeMaterials (Material material1, Material material2, float duration) {
		for (float i = 1; i >= 0; i -= Time.deltaTime / duration) {
			var newColor1 = new Color (material1.color.r, material1.color.g, material1.color.b, Mathf.Lerp (0, 1, i));
			var newColor2 = new Color (material2.color.r, material2.color.g, material2.color.b, Mathf.Lerp (0, 1, i * -1));
			material1.color = newColor1;
			material2.color = newColor2;
			yield return null;
		}
	}

	enum FadeDirection { In, Out }

	[System.Serializable]
	public enum FadeElementType { Auto, SpriteRenderer, Image, TextMeshProUGUI, Material }
}
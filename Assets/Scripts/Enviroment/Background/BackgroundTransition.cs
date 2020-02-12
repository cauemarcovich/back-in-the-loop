using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTransition : MonoBehaviour {
	public GameObject[] Maps;
	public float FadeDuration;

	FadeController _fadeController;
	Transform _environment;
	GameObject _currentMap;	

	private void Start () {
		_fadeController = GetComponent<FadeController> ();
		_environment = GameObject.Find("Environment").transform;
		
		var firstMap = Instantiate(Maps[0], _environment);
		_currentMap = firstMap;
	}

	public IEnumerator TransitionMaps (int newMapIndex) {		
		var newMap = Instantiate(Maps[newMapIndex], _environment);

		yield return new WaitForSeconds(.01f);

		StartCoroutine(_currentMap.GetComponent<FadeController>().FadeIn(FadeDuration));
		StartCoroutine(newMap.GetComponent<FadeController>().FadeOut(FadeDuration));
		
		yield return new WaitForSeconds(FadeDuration + 1f);

		_currentMap = newMap;
	}
}
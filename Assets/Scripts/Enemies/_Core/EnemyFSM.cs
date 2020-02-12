using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour {
	private System.Action _action;

	public void SetState (System.Action x) {
		_action = x;
	}
	public string GetStateName () {
		return _action.Method.Name;
	}

	void Update () {
		if (_action != null) {
			_action ();
			//Debug.Log(_action.Method.Name);
		}
	}
}
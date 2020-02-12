using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevScript : MonoBehaviour {
	public List<GameObject> ObjectsToDelete;

	void Awake () {
		ObjectsToDelete.ForEach(_ => Destroy(_));
	}
}
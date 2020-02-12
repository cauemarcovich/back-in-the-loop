using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitScreenResolution : MonoBehaviour {
	private void Awake () {
		//Set screen size for Standalone
#if UNITY_STANDALONE
		Screen.SetResolution (564, 960, false);
		Screen.fullScreen = false;
#endif
	}
}
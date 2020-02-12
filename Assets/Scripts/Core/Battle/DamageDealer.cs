using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour {
    public int Damage = 100;
    public bool DontDestroy = false;

    public void Hit () {
        if (!DontDestroy)
            Destroy (gameObject);
    }
}
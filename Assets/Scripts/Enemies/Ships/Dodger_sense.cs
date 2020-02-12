using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodger_sense : MonoBehaviour {
    void OnTriggerEnter2D (Collider2D collision) {
        transform.parent.GetComponent<Dodger> ().SetEvadeState ();
    }
}
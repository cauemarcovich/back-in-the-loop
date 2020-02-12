using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    public float rotateSpeed;

    void Update () {
        transform.Rotate (Vector3.forward * Time.deltaTime * rotateSpeed);
    }
}
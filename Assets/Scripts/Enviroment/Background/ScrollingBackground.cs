using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour {
    public float Speed = 0.2f;
    Material myMaterial;
    Vector2 offset;

    void Start () {
        myMaterial = GetComponent<Renderer> ().material;
        offset = new Vector2 (0f, Speed);
    }

    void Update () {
        myMaterial.mainTextureOffset += offset * Time.deltaTime;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;

public class Flasher : MonoBehaviour {
    public float FlashDuration = 0.05f;

    float _fduration;
    bool _isFlashing;

    List<SpriteRenderer> elements;
    Color oldColor;

    void Start () {
        elements = GetSpriteRenderers ();
        oldColor = elements[0].material.GetColor ("_Color");
    }

    void FixedUpdate () {
        if (_isFlashing) {
            if (_fduration >= 0f) {
                _fduration -= Time.deltaTime;
            } else {
                _isFlashing = false;
                elements.ForEach (_ => {
                    var material = _.material;
                    material.SetColor ("_Color", oldColor);
                });
            }
        }
    }

    public void Flash () {
        elements.ForEach (_ => {
            var material = _.material;
            material.SetColor ("_Color", new Color (10, 10, 10, 1));
        });

        _isFlashing = true;
        _fduration = FlashDuration;
    }

    List<SpriteRenderer> GetSpriteRenderers () {
        var objects = transform.GetAllElements ();
        return objects.Select (_ => _.GetComponent<SpriteRenderer> ()).Where (_ => _ != null).ToList ();
    }
}
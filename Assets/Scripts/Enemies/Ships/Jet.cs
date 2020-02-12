using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using UnityEngine;

public class Jet : Enemy {
    float velocity = 0.3f;

    new void Start () {
        base.Start ();
        _sm.SetState (Search);
    }

    void Search () {
        if (_player == null) return;

        var xDiff = transform.position.x - _player.transform.position.x;
        if (xDiff.IsBetween (-0.02f, 0.02f))
            _sm.SetState (Attack);
    }

    void Attack () {
        _pathing.Stop (true);

        if (velocity <= 40f)
            velocity += 0.2f;

        transform.position += new Vector3 (0f, -velocity * Time.deltaTime, transform.position.z);
    }

    public void ForceAttack () {
        _sm.SetState (Attack);
    }
}
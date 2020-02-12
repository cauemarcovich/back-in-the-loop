using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using UnityEngine;

public class Boomer : Enemy {
    new void Start () {
        base.Start ();
        _sm.SetState (Move);
    }

    void Move () {
        _moveCounter -= Time.deltaTime;

        if (_moveCounter <= 0) {
            _sm.SetState (Search);
            SetRandomShootCounter ();
        }
    }

    void Search () {
        if (_player == null) {
            _sm.SetState (Move);
            return;
        }
        var xDiff = transform.position.x - _player.transform.position.x;
        if (xDiff.IsBetween (-0.02f, 0.02f))
            _sm.SetState (Attack);
    }

    void Attack () {
        if (!_pathing.IsStopped ())
            StartCoroutine (Attacking ());
    }

    IEnumerator Attacking () {
        _sm.SetState (Attack);

        _pathing.Stop (true);

        yield return new WaitForSeconds (0.25f);

        var holdPosition = transform.position;
        var preparePosition = new Vector3 (transform.position.x, transform.position.y + 0.25f, 0f);

        while (transform.position != preparePosition) {
            transform.position = Vector3.MoveTowards (transform.position, preparePosition, 0.5f * Time.deltaTime);
            yield return new WaitForEndOfFrame ();
        }

        var velocity = 0.3f;
        while (transform.position.y > _player.transform.position.y) {
            if(_player == null) break;
            if (velocity <= 30f)
                velocity += 0.3f;
            var target = new Vector3 (transform.position.x, _player.transform.position.y, 0f);
            transform.position = Vector3.MoveTowards (transform.position, target, velocity * Time.deltaTime);
            yield return new WaitForEndOfFrame ();
        }
        while (transform.position.y < holdPosition.y) {
            if (velocity > 0.3f)
                velocity -= 0.3f;
            transform.position = Vector3.MoveTowards (transform.position, holdPosition, velocity * Time.deltaTime);
            yield return new WaitForEndOfFrame ();
        }

        yield return new WaitForSeconds (1f);

        _sm.SetState (Move);
        _pathing.Stop (false);
    }
}
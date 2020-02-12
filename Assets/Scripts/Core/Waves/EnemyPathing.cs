using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathing : MonoBehaviour {
    public WaveConfig WaveConfig;
    public List<Transform> _waypoints;

    public bool LockOnFirst;
    public bool unlockCorout;

    public float MoveSpeedModifier = 1f;

    public int _pathIndex;

    public bool stopped = false;
    public void Stop (bool stop) { stopped = stop; }
    public bool IsStopped () { return stopped; }

    protected Vector3 _centerOfScreen;

    void Start () {
        _centerOfScreen = Camera.main.ViewportToWorldPoint (new Vector3 (0.5f, 0.75f));
        _centerOfScreen.z = 0f; //????????????????????

        if (WaveConfig != null) {
            _waypoints = WaveConfig.Waypoints ();
            transform.position = _waypoints[_pathIndex].position;
        }
    }

    void FixedUpdate () {
        if (_waypoints != null && !stopped)
            Move ();
    }

    public void SetWaveConfig (WaveConfig _waveConfig) {
        WaveConfig = _waveConfig;
    }

    void Move () {
        if (_pathIndex < _waypoints.Count) {
            var targetPosition = _waypoints[_pathIndex].position;

            var moveSpeed = WaveConfig.MoveSpeed * MoveSpeedModifier * Time.deltaTime;

            transform.position = Vector2.MoveTowards (transform.position, targetPosition, moveSpeed);

            if (LockOnFirst && _pathIndex == 1) {
                GetComponent<Enemy> ().EnableColliders (false);

                if (Vector3.SqrMagnitude (targetPosition - transform.position) < 0.1f) {
                    if (LockOnFirst && _pathIndex == 1) {
                        if (unlockCorout) return;
                        StartCoroutine (UnlockPath ());
                        return;
                    }
                }
            }

            if (Vector3.SqrMagnitude (targetPosition - transform.position) < 0.1f) {
                _pathIndex++;

                if (WaveConfig.RepeatPathing && _pathIndex == _waypoints.Count)
                    _pathIndex = 1;
            }
        } else {
            Destroy (gameObject);
        }
    }

    IEnumerator UnlockPath () {
        unlockCorout = true;

        yield return new WaitForSeconds (1f);
        LockOnFirst = false;
        unlockCorout = false;
        GetComponent<Enemy> ().EnableColliders (true);
    }

    public bool MoveToCenter () {
        return MoveToPosition (_centerOfScreen);
    }

    public bool MoveToPosition (Vector3 destination) {
        transform.position = Vector3.MoveTowards (transform.position, destination, WaveConfig.MoveSpeed * Time.deltaTime);
        return (transform.position == destination);
    }

    public bool MoveToPositionZigzag (Vector3 destination) {
        var newPos = Vector3.MoveTowards (transform.position, destination, WaveConfig.MoveSpeed * Time.deltaTime);
        newPos = new Vector3 (newPos.x, Mathf.PingPong (Time.time, 1), newPos.z);
        transform.position = newPos;
        return (transform.position == destination);
    }
    public bool MoveChildToPosition (Transform child, Vector3 destination, float moveSlowdown = 1f) {
        child.localPosition = Vector3.MoveTowards (child.localPosition, destination, (WaveConfig.MoveSpeed / moveSlowdown) * Time.deltaTime);
        return (child.localPosition == destination);
    }
}
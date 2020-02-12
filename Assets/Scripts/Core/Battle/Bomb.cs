using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour {
    public float speed = 1f;
    //public float rotateSpeed = 200f;
    //public bool chase;

    //Transform target;
    Rigidbody2D rb;

    void Start () {
        rb = GetComponent<Rigidbody2D> ();
        //StartCoroutine(UpdateTarget());
    }
    void FixedUpdate () {
        rb.velocity += new Vector2 (0, 1 * speed) * Time.deltaTime;
    }

    //void FixedUpdate()
    //{
    //    if (chase && target != null)
    //    {
    //        var direction = target.position - transform.position;
    //        direction.Normalize();
    //        var rotateAmount = Vector3.Cross(direction, transform.up).z;

    //        rb.angularVelocity = -rotateAmount * rotateSpeed;
    //        rb.velocity = transform.up * speed;
    //    }
    //}

    //IEnumerator UpdateTarget()
    //{
    //    while (true)
    //    {
    //        var allEnemies = GameObject.FindGameObjectsWithTag("Enemy").Select(_ => _.transform).ToArray();

    //        if (allEnemies.Length == 0)
    //            allEnemies = GameObject.FindGameObjectsWithTag("Shredder").Where(_ => _.name == "Shredder_top").Select(_ => _.transform).ToArray();

    //        target = GetClosestEnemy(allEnemies);

    //        yield return new WaitForSeconds(0.75f);
    //    }
    //}

    //Transform GetClosestEnemy(Transform[] enemies)
    //{
    //    Transform enemyClosest = null;
    //    float minDistance = Mathf.Infinity;

    //    foreach (Transform enemy in enemies)
    //    {
    //        float distance = Vector3.Distance(enemy.position, transform.position);
    //        if (distance < minDistance)
    //        {
    //            enemyClosest = enemy;
    //            minDistance = distance;
    //        }
    //    }
    //    return enemyClosest;
    //}
}
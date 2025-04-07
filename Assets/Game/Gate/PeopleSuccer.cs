using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PeopleSuccer : MonoBehaviour
{
    [Header("Succ Physics")]
    public float toVel = 2.5f;
    public float maxVel = 4.0f;
    public float maxForce = 10.0f;
    public float gain = 3f;

    public List<GameObject> affectedObjects;
    public Transform target;
    public LayerMask affectedLayer = 1 << 7;

    void OnTriggerEnter2D(Collider2D collidee)
    {
        if (((1 << collidee.gameObject.layer) & affectedLayer) == 0)
        {
            return;
        }
        affectedObjects.Add(collidee.gameObject);
    }
    void OnTriggerExit2D(Collider2D collidee)
    {
        if (((1 << collidee.gameObject.layer) & affectedLayer) == 0)
        {
            return;
        }
        affectedObjects.Remove(collidee.gameObject);
    }
    void FixedUpdate()
    {
        foreach (var go in affectedObjects)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            Vector2 dist = new Vector2(target.position.x - go.transform.position.x, target.position.y - go.transform.position.y);
            Vector2 tgtVel = Vector2.ClampMagnitude(toVel * dist, maxVel);
            Vector2 error = tgtVel - rb.linearVelocity;
            Vector2 force = Vector2.ClampMagnitude(gain * error, maxForce);
            rb.AddForce(force);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ResourceField : MonoBehaviour
{
    public List<GameObject> affectedObjects;
    public Vector3 forceVector = Vector3.up;
    public Transform centerAxis;
    public LayerMask affectedLayer = 1 << 6;

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
        foreach(var go in affectedObjects)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                continue;
            }

            rb.AddForce(forceVector);

            Vector3 attractionToCenter = (centerAxis.transform.position - go.transform.position).normalized;
            rb.AddForce(attractionToCenter);
        }
    }
}

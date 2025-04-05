using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MakoOrb : MonoBehaviour
{
    public bool Collectable { get; set; }
    public bool Captured { get; set; }

    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    public void ApplySpawningImpulse(float impulseScale)
    {
        var impulseForce = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(0.0f, 1.0f)).normalized * impulseScale;
        _rb.AddForce(impulseForce, ForceMode2D.Impulse);
    }

    public void ApplyAccleratingImpulse(float impulseScale)
    {
        var impulseForce = new Vector2(Random.Range(-0.7f, 0.7f), Random.Range(0.0f, 1.0f)).normalized * impulseScale;
        _rb.AddForce(impulseForce, ForceMode2D.Impulse);
    }

    public void DriftTowardsTarget(Transform target, float toVel, float maxVel, float maxForce, float gain)
    {
        Vector2 dist = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
        Vector2 tgtVel = Vector2.ClampMagnitude(toVel * dist, maxVel);
        // calculate the velocity error
        Vector2 error = tgtVel - _rb.linearVelocity;
        // calc a force proportional to the error (clamped to maxForce)
        Vector2 force = Vector2.ClampMagnitude(gain * error, maxForce);
        _rb.AddForce(force);
    }

    public void StopDrifting()
    {
        _rb.linearVelocity = Vector2.zero;
    }
}

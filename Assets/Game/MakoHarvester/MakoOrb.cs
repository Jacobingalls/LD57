using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MakoOrb : MonoBehaviour
{
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
}

using UnityEngine;

public class PeopleEater : MonoBehaviour
{
    public LayerMask affectedLayer = 1 << 7;

    private GateController _gate;

    public void Start()
    {
        _gate = GetComponentInParent<GateController>();    
    }
    void OnTriggerEnter2D(Collider2D collidee)
    {
        if (((1 << collidee.gameObject.layer) & affectedLayer) == 0)
        {
            return;
        }

        _gate.AttemptToOpenGate();
        Destroy(collidee.gameObject);
    }
}

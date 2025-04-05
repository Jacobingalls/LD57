using UnityEngine;

public class MakoAccelerator : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collidee)
    {
        var makoOrb = collidee.gameObject.GetComponent<MakoOrb>();
        if (makoOrb == null)
        {
            return;
        }
        const float impulseMin = 3.0f;
        const float impulseMax = 5.5f;
        makoOrb.ApplyAccleratingImpulse(Random.Range(impulseMin, impulseMax));
        makoOrb.Collectable = true;
        AudioManager.Instance.Play("Mako/Accelerate", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
    }
}

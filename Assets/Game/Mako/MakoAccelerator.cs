using info.jacobingalls.jamkit;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class MakoAccelerator : MonoBehaviour
{
    private PubSubSender _pubsub;

    private void Start()
    {
        _pubsub = GetComponent<PubSubSender>();
    }

    void OnTriggerEnter2D(Collider2D collidee)
    {
        var makoOrb = collidee.gameObject.GetComponent<MakoOrb>();
        if (makoOrb == null)
        {
            return;
        }
        if (makoOrb.Collectable)
        {
            return;
        }
        const float impulseMin = 3.0f;
        const float impulseMax = 5.5f;
        makoOrb.ApplyAccleratingImpulse(Random.Range(impulseMin, impulseMax));
        AudioManager.Instance.Play2D("Mako/Accelerate", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);

        StartCoroutine(MakeMakoOrbCollectable(makoOrb, afterDelay: 1.0f));
    }

    IEnumerator MakeMakoOrbCollectable(MakoOrb makoOrb, float afterDelay)
    {
        makoOrb.ShowAcceleratedVisuals();
        makoOrb.Flash(duration: afterDelay);

        yield return new WaitForSeconds(afterDelay);

        makoOrb.Collectable = true;
        _pubsub.Publish("mako.harvested");
    }
}

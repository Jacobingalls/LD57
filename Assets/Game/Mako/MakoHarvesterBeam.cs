using Unity.VisualScripting;
using UnityEngine;

public class MakoHarvesterBeam : MonoBehaviour
{
    public int pixelsPerUnit = 16;
    public AnimationCurve opacityCurve = AnimationCurve.EaseInOut(0.0f, 0.02f, 0.2f, 0.04f);

    public Transform Target;
    private bool _tracking;

    private void Update()
    {
        if (Target == null)
        {
            if (_tracking)
            {
                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    child.gameObject.SetActive(false);
                }
                _tracking = false;
            }
            return;
        }

        if (!_tracking)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.gameObject.SetActive(true);
            }
            _tracking = true;
        }

        TrackToTarget();
    }

    private void TrackToTarget()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            Vector2 direction = Target.transform.position - child.transform.position;

            child.transform.localScale = new Vector3(1.0f, direction.magnitude * pixelsPerUnit, 1.0f);
            child.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, opacityCurve.Evaluate(Time.time));
            }
        }
    }
}

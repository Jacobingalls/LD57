using UnityEngine;

public class JumpingMortal : MonoBehaviour
{
    public GameObject visuals;
    public AnimationCurve animationCurve;

    [Range(0, 2.0f)]
    public float yScale = 2.0f / 16.0f;

    [Range(0, 10.0f)]
    public float speed = 1.0f;

    private Vector3 _startingPosition;

    private bool _jumping = false;
    private float _time = 0.0f;
    public bool jumping
    {
        get
        {
            return _jumping;
        }
        set
        {
            if (value == _jumping)
            {
                return;
            }

            if (value == true)
            {
                _time = 0.0f;
            }

            _jumping = value;
        }
    }

    void Start()
    {
        _startingPosition = visuals.transform.position;
    }

    void Update()
    {
        _time += Time.deltaTime;
        var time = jumping ? _time : 0.0f;
        var offset = new Vector3(0.0f, animationCurve.Evaluate(time) * yScale, 0.0f);
        visuals.transform.position = _startingPosition + offset;
    }
}

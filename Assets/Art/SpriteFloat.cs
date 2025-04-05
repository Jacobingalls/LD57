using UnityEngine;

public class SpriteFloat : MonoBehaviour
{
    [Header("Float")]
    public bool floating = true;
    public AnimationCurve floatCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [Range(0.0f, 10.0f)] public float floatSpeed = 1.0f;
    [Range(0.0f, 5.0f)] public float floatDistance = 1.0f;

    [Header("Jitter")]
    [Range(0.0f, 1.0f)] public float jitterDuration = 0.5f;
    public AnimationCurve jitterCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [Range(0.0f, 10.0f)] public float jitterSpeed = 2.5f;
    [Range(0.0f, 5.0f)] public float jitterDistance = 0.5f;
    private float _jitterTime = 0.0f;

    private Vector3 _startPosition;
    private float _startOffset;

    void Start()
    {
        _startPosition = transform.localPosition;
        _startOffset = Random.Range(0, 1.0f);
    }

    void Update()
    {
        var offset = Vector3.zero;

        if (floating)
        {
            offset += new Vector3(0.0f, floatCurve.Evaluate((Time.time + _startOffset) * floatSpeed) * floatDistance, 0.0f);
        }

        if (_jitterTime > 0)
        {
            float t = jitterDuration - _jitterTime;
            offset += new Vector3((jitterCurve.Evaluate(t * jitterSpeed) * jitterDistance) - jitterDistance / 2.0f, 0.0f, 0.0f);
            _jitterTime -= Time.deltaTime;
        }

        transform.localPosition = _startPosition + offset;
    }

    public void Jitter(float jitterDuration)
    {
        this.jitterDuration = jitterDuration;
        _jitterTime = jitterDuration;
    }
}

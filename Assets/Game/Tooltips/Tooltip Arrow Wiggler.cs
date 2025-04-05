using UnityEngine;

public class TooltipArrowWiggler : MonoBehaviour
{

    private float currentWiggleTime = 0f;
    public float wiggleSpeed = 1f;
    public Vector3 wiggleAmount = Vector3.one;
    public AnimationCurve wiggleCurve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentWiggleTime += Time.deltaTime * wiggleSpeed;
        currentWiggleTime = Mathf.Repeat(currentWiggleTime, 1f);
        gameObject.transform.localPosition = wiggleCurve.Evaluate(currentWiggleTime) * wiggleAmount;
    }
}

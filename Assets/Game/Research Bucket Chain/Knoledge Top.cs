using UnityEngine;

public class KnoledgeTop : MonoBehaviour
{

    public AnimationCurve curve;
    private float currentTime = 1f;

    public SpriteRenderer litSpriteRenderer;
    public SpriteRenderer unlitSpriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unlitSpriteRenderer.gameObject.SetActive(true);
        litSpriteRenderer.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        currentTime = Mathf.Clamp01(currentTime);
        unlitSpriteRenderer.color = new Color(1, 1, 1, curve.Evaluate(currentTime));
        litSpriteRenderer.color = new Color(1, 1, 1, 1 - curve.Evaluate(currentTime));
    }

    public void DidGetKnowledge() {
        currentTime = 0;
    }
}

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

        AudioManager.Instance.Play2D(
            "Factory/TowerHit", 
            loop: false,
            pitchMin: 0.9f,
            pitchMax: 1.1f,
            volumeMin: 0.3f, 
            volumeMax: 0.5f, 
            position: transform.position
        );
    }
}

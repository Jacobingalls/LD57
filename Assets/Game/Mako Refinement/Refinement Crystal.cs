using UnityEngine;

public class RefinementCrystal : MonoBehaviour
{

    public SpriteRenderer unlitSprite;
    public SpriteRenderer litSprite;

    public SpriteRenderer laserSprite;

    public AnimationCurve animationCurve;

    private float animationTime = 1f;

    public float animationSpeed = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unlitSprite.gameObject.SetActive(true);
        litSprite.gameObject.SetActive(true);
        animationTime = 1f;
        unlitSprite.color = new Color(1, 1, 1, 1);
        litSprite.color = new Color(1, 1, 1, 0);
        laserSprite.color = new Color(1, 1, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        animationTime += Time.deltaTime;
        animationTime = Mathf.Clamp01(animationTime);
        unlitSprite.color = new Color(1, 1, 1, animationCurve.Evaluate(animationTime * animationSpeed));
        litSprite.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(animationTime * animationSpeed));
        laserSprite.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(animationTime * animationSpeed * 2));
    }

    public void LaserDidFire() {
        animationTime = 0f;
    }
}

using UnityEngine;

public class RefinementCrystal : MonoBehaviour
{

    public int laserLevel = 1;

    public SpriteRenderer unlitSprite;
    public SpriteRenderer litSprite;

    public SpriteRenderer laserSprite1, laserSprite2, laserSprite3;

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
        
        laserSprite1.color = new Color(1, 1, 1, 0);
        laserSprite2.color = new Color(1, 1, 1, 0);
        laserSprite3.color = new Color(1, 1, 1, 0);

        laserSprite1.gameObject.SetActive(true);
        laserSprite2.gameObject.SetActive(false);
        laserSprite3.gameObject.SetActive(false);

        laserLevel = 1;
        SetLaserLevel(laserLevel);
    }

    // Update is called once per frame
    void Update()
    {
        animationTime += Time.deltaTime;
        animationTime = Mathf.Clamp01(animationTime);
        unlitSprite.color = new Color(1, 1, 1, animationCurve.Evaluate(animationTime * animationSpeed));
        litSprite.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(animationTime * animationSpeed));
       
        laserSprite1.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(animationTime * animationSpeed * 2));
        laserSprite2.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(animationTime * animationSpeed * 2));
        laserSprite3.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(animationTime * animationSpeed * 2));

        laserSprite1.gameObject.SetActive(laserLevel == 1);
        laserSprite2.gameObject.SetActive(laserLevel == 2);
        laserSprite3.gameObject.SetActive(laserLevel == 3);
    }

    public void LaserDidFire() {
        animationTime = 0f;
    }

    public void UpdateLaserLevel() {
        int level = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementIncreaseLaserLevel).timesPurchased + 1;
        SetLaserLevel(level);
    }

    public void SetLaserLevel(int level) {
        laserLevel = Mathf.Clamp(level, 1, 3);
    }
}

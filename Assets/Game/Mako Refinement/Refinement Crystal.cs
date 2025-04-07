using UnityEngine;

public class RefinementCrystal : MonoBehaviour
{

    // Beter orbs, different sprites
    public int laserLevel = 1;

    //More Orbs, more opaque
    public int laserPower = 1;

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

        laserPower = 1;
        SetLaserPower(laserPower);
    }

    // Update is called once per frame
    void Update()
    {
        animationTime += Time.deltaTime;
        animationTime = Mathf.Clamp01(animationTime);
        float curvePos = animationTime * animationSpeed;
        unlitSprite.color = new Color(1, 1, 1, animationCurve.Evaluate(curvePos));
        litSprite.color = new Color(1, 1, 1, 1 - animationCurve.Evaluate(curvePos));
       
        float attenuatation = 1f - animationCurve.Evaluate(curvePos * 2);
        attenuatation *= Mathf.Clamp01(laserLevel / 3f);
        laserSprite1.color = new Color(1, 1, 1, attenuatation);
        laserSprite2.color = new Color(1, 1, 1, attenuatation);
        laserSprite3.color = new Color(1, 1, 1, attenuatation);

        laserSprite1.gameObject.SetActive(laserPower == 1);
        laserSprite2.gameObject.SetActive(laserPower == 2);
        laserSprite3.gameObject.SetActive(laserPower == 3);
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

    public void UpdateLaserPower() {
        int power = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementIncreaseLaserPower).timesPurchased + 1;
        SetLaserPower(power);
    }

    public void SetLaserPower(int power) {
        laserPower = Mathf.Clamp(power, 1, 3);
    }
}

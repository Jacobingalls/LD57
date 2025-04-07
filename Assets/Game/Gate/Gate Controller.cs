using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GateController : MonoBehaviour
{

    public float closingForcePerSecond = 1f;
    public float progress = 0f;
    public float currentOffset = 0f;
    public float forcePerOpenAttempt = 0.1f;
    public float progressToAnimateToWhenOpen = 1.2f;

    public float totalSizeToOpen = 1f;

    public GameObject leftGate;
    public GameObject rightGate;

    public GameObject coin;
    public GameObject gradient;

    public UnityEvent onOpenGate;

    public bool isOpen = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen) {
            progress = progressToAnimateToWhenOpen;
        }
        else 
        {
            progress -= closingForcePerSecond * Time.deltaTime;
            progress = Mathf.Clamp01(progress);
        }
        
        UpdateGate();
    }

    public void UpdateGate() {
        float offset = totalSizeToOpen * progress;
        currentOffset = expDecay(currentOffset, offset, 4f, Time.deltaTime);

        Vector3 leftGatePosition = leftGate.transform.localPosition;
        leftGatePosition.x = -currentOffset;
        leftGate.transform.localPosition = leftGatePosition;

        Vector3 rightGatePosition = rightGate.transform.localPosition;
        rightGatePosition.x = currentOffset;
        rightGate.transform.localPosition = rightGatePosition;


        Mathf.InverseLerp(0f, totalSizeToOpen * 0.1f, currentOffset);

        float coinScale = Mathf.Lerp(1f, 0.8f, Mathf.InverseLerp(0f, totalSizeToOpen * 0.05f, currentOffset));
        coin.transform.localScale = new Vector3(coinScale, coinScale, coinScale);
        float coinAlpha = Mathf.Lerp(1f, 0.75f, Mathf.InverseLerp(0f, totalSizeToOpen * 0.03f, currentOffset));
        coin.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, coinAlpha);
    }

    public void AttemptToOpenGate() {
        if (isOpen) {
            return;
        }

        coin.GetComponent<SpriteFloat>().Jitter(0.1f);

        progress += forcePerOpenAttempt;
        progress = Mathf.Clamp01(progress);
        if (progress >= 1f) {
            ForceOpenGate();
        }

        AudioManager.Instance.Play2D(
            "Gate/PersonHit", 
            pitchMin: 0.8f, 
            pitchMax: 1.2f, 
            volumeMin: 0.25f, 
            volumeMax: 0.5f, 
            position: transform.position
        );
    }

    // https://acegikmo.substack.com/p/lerp-smoothing-is-broken
    float expDecay(float a, float b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }

    public void ForceOpenGate() {
        isOpen = true;
        onOpenGate.Invoke();
        FadeOutGradient(0.25f);
    }

    public void FadeOutGradient(float duration)
    {
        StartCoroutine(FadeOutGradientCoroutine(duration));
    }

    private IEnumerator FadeOutGradientCoroutine(float duration)
    {
        SpriteRenderer[] spriteRenderers = gradient.GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            foreach (var spriteRenderer in spriteRenderers)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure all sprites are fully transparent at the end
        foreach (var spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }
}

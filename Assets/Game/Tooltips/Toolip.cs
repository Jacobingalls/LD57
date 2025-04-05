using UnityEngine;
using TMPro;

public class Toolip : MonoBehaviour
{

    public TextMeshProUGUI tooltipTitle;
    public TextMeshProUGUI tooltipText;

    public GameObject tooltipContent;

    private Canvas canvas;

    private int currentReceipt = 0;

    private Vector3 targetPosition;

    public CanvasGroup canvasGroup;

    public float fadeDuration = 0.5f;
    public float fadeCurrentTime = 0f;
    public bool isShowingTooltip = false;

    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = gameObject.GetComponentInParent<Canvas>();
        tooltipContent.SetActive(false);
        targetPosition = gameObject.transform.localPosition;
        canvasGroup.alpha = 0f;
        isShowingTooltip = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isShowingTooltip) {
            tooltipContent.SetActive(true);
            fadeCurrentTime += Time.deltaTime;
        } else if (fadeCurrentTime == 0f) {
            tooltipContent.SetActive(false);
        } else {
            fadeCurrentTime -= Time.deltaTime;
        }
        fadeCurrentTime = Mathf.Clamp(fadeCurrentTime, 0f, fadeDuration);
        canvasGroup.alpha = fadeCurve.Evaluate(fadeCurrentTime / fadeDuration);
        gameObject.transform.localPosition = expDecay(gameObject.transform.localPosition, targetPosition, 10f, Time.deltaTime);
    }

    // Returns a recipe for the tooltip that needs to be passed to us again to hide it.
    public int ShowTooltip(TooltipProvider tooltipProvider)
    {
        isShowingTooltip = true;
        currentReceipt += 1;
        tooltipTitle.text = tooltipProvider.tooltipTitle;
        tooltipText.text = tooltipProvider.tooltipText;

        GameObject obj = tooltipProvider.gameObject;
        RectTransform objRect = obj.GetComponent<RectTransform>();
        Vector3 localPosition = canvas.transform.InverseTransformPoint(objRect.position);
        targetPosition = new Vector3(
            localPosition.x - (objRect.rect.width / 2) - 5,
            localPosition.y,
            localPosition.z
        );

        return currentReceipt;
    }

    public void HideTooltip(int receipt)
    {
        if (receipt != currentReceipt) { return; }
        isShowingTooltip = false;
    }

        // https://acegikmo.substack.com/p/lerp-smoothing-is-broken
    float expDecay(float a, float b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }

    Vector3 expDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return new Vector3(
            expDecay(a.x, b.x, decay, deltaTime),
            expDecay(a.y, b.y, decay, deltaTime),
            expDecay(a.z, b.z, decay, deltaTime)
        );
    }
}

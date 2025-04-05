using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipProvider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipTitle;

    [TextArea(5, 10)]
    public string tooltipText;

    private Toolip tooltip;

    private bool isHovering = false;

    public bool isShowingTooltip = false;

    /// The ID we get back from the toolip to know which one to hide
    public int? reciept = null;

    void Start()
    {
        Canvas canvas = gameObject.GetComponentInParent<Canvas>();
        tooltip = canvas.GetComponentInChildren<Toolip>();
        if (tooltip == null)
        {
            var toolipPrefab = LD57GameManager.Instance.tooltipPrefab;
            var newTooltip = Instantiate(toolipPrefab, canvas.transform);
            tooltip = newTooltip;
        }
    }

    void Update()
    {
        
    }

    private void ShowTooltip()
    {
        if (isShowingTooltip) { return; };
        isShowingTooltip = true;
        if (tooltip == null)
        {
            Debug.LogError("Tooltip is not assigned in the inspector.");
            return;
        }

        reciept = tooltip.ShowTooltip(this);
    }

    private void HideTooltip()
    {
        if (!isShowingTooltip) { return; };
        isShowingTooltip = false;

        tooltip.HideTooltip(reciept ?? 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        HideTooltip();
    }
}

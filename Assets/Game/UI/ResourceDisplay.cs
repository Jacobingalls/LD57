using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class ResourceDisplay : MonoBehaviour
{
    public ResourceType resourceType;
    public Image resourceImage;
    public TextMeshProUGUI resourceLabel;

    public void Start()
    {
        resourceImage.sprite = ResourceManager.Instance.GetResourceSprite(resourceType);
    }
    void Update()
    {
        var amt = ResourceManager.Instance.GetResourceAmount(resourceType);
        var valueStr = amt > 1e4 ? string.Format("{0:0.##E+0}", amt) : amt.ToString();

        resourceLabel.text = $"{valueStr}";
    }
}

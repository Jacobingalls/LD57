using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        resourceLabel.text = $"{ResourceManager.Instance.GetResourceAmount(resourceType)}";
    }
}

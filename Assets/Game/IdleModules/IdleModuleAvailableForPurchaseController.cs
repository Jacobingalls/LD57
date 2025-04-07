using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IdleModuleAvailableForPurchaseController : MonoBehaviour
{

    public IdleModule idleModule;

    public Button purchaseButton;

    public TextMeshProUGUI costText;
    public TextMeshProUGUI titleText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleText.text = $"{idleModule.modulePurchaseTeaserName}";
        costText.text = $"<sprite name=\"Mako\"> {idleModule.purchaseCost}";
    }

    // Update is called once per frame
    void Update()
    {
        purchaseButton.interactable = idleModule.CanBePurchased();
        costText.color = purchaseButton.interactable ? Color.white : Color.white * 0.5f;
    }
}

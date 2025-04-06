using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRow : MonoBehaviour
{
    public TooltipProvider tooltipProvider;
    public Button purchaseButton;
    public TextMeshProUGUI upgradeTitleLabel;
    public TextMeshProUGUI upgradePriceLabel;

    private UpgradeType _upgradeType;

    public void SetUpgradeType(UpgradeType upgradeType)
    {
        _upgradeType = upgradeType;

        Redraw();
    }

    public void PurchaseUpgrade()
    {
        UpgradeManager.Instance.Purchase(_upgradeType);
    }

    private void Redraw()
    {
        var upgrade = UpgradeManager.Instance.GetUpgrade(_upgradeType);
        tooltipProvider.tooltipTitle = upgrade.name;
        tooltipProvider.tooltipText = upgrade.description;
        upgradeTitleLabel.text = upgrade.name;

        var costText = "";

        foreach (var entry in upgrade.cost)
        {
            costText += $"{entry.Value},";
        }

        upgradePriceLabel.text = costText;

        purchaseButton.interactable = upgrade.Unlocked;
    }
}

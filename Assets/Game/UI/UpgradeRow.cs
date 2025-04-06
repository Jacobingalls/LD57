using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRow : MonoBehaviour
{
    public GameObject lockedGO;
    public GameObject checkedGO;
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

    public void Update()
    {
        purchaseButton.interactable = UpgradeManager.Instance.CanAfford(_upgradeType);
    }

    private void Redraw()
    {
        var upgrade = UpgradeManager.Instance.GetUpgrade(_upgradeType);
        tooltipProvider.tooltipTitle = upgrade.name;
        tooltipProvider.tooltipText = upgrade.description;
        upgradeTitleLabel.text = upgrade.name;

        var costText = "";

        for (var i = 0; i < upgrade.cost.Count; i++)
        {
            var entry = upgrade.cost.ElementAt(i);
            costText += $"<sprite name=\"{entry.Key}\"> {entry.Value}";
            if (i != upgrade.cost.Count - 1)
            {
                costText += " ";
            }
        }

        upgradePriceLabel.text = costText;

        purchaseButton.interactable = UpgradeManager.Instance.CanAfford(_upgradeType);

        purchaseButton.gameObject.SetActive(upgrade.Unlocked && !upgrade.Complete);
        lockedGO.SetActive(!upgrade.Unlocked);
        checkedGO.SetActive(upgrade.Complete);
    }
}

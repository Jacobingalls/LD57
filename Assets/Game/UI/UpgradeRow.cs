using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Rendering.Universal;

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

        var romanNumeral = "";
        if (upgrade.maxPurchases > 1)
        {
            // thank you, Tsundere Dev
            romanNumeral += " ";
            var timesPurchased = upgrade.Complete ? upgrade.maxPurchases - 1 : upgrade.timesPurchased;
            if (timesPurchased == 0)
            {
                romanNumeral += "I";
            } 
            else if (timesPurchased == 1)
            {
                romanNumeral += "II";
            }
            else if (timesPurchased == 2)
            {
                romanNumeral += "III";
            }
            else if (timesPurchased == 3)
            {
                romanNumeral += "IV";
            }
            else if (timesPurchased == 4)
            {
                romanNumeral += "V";
            }
            else if (timesPurchased == 5)
            {
                romanNumeral += "VI";
            }
            else
            {
                romanNumeral += $"{timesPurchased + 1}";
            }
        }
        var upgradeName = $"{upgrade.name}{romanNumeral}";

        tooltipProvider.tooltipTitle = upgradeName;
        tooltipProvider.tooltipText = upgrade.description;
        upgradeTitleLabel.text = upgradeName;

        var costText = "";

        for (var i = 0; i < upgrade.Costs.Count; i++)
        {
            var entry = upgrade.Costs.ElementAt(i);
            costText += $"<sprite name=\"{entry.Key}\"> {entry.Value}";
            if (i != upgrade.Costs.Count - 1)
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

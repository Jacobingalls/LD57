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
        upgradePriceLabel.color = purchaseButton.interactable ? Color.white : Color.white * 0.5f;
    }

    private string ConvertToRomanNumerals(int number)
    {
        if (number < 1 || number > 100)
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 1 and 100.");

        var romanNumerals = new Dictionary<int, string>
        {
            { 100, "C" },
            { 90, "XC" },
            { 50, "L" },
            { 40, "XL" },
            { 10, "X" },
            { 9, "IX" },
            { 5, "V" },
            { 4, "IV" },
            { 1, "I" }
        };

        var result = string.Empty;

        foreach (var pair in romanNumerals)
        {
            while (number >= pair.Key)
            {
                result += pair.Value;
                number -= pair.Key;
            }
        }

        return result;
    }

    private void Redraw()
    {
        var upgrade = UpgradeManager.Instance.GetUpgrade(_upgradeType);

        var numeral = upgrade.timesPurchased + (upgrade.Complete ? 0 : 1);
        var romanNumeral = numeral <= 1 ? "" : ConvertToRomanNumerals(numeral);

        var upgradeName = $"{upgrade.name} {romanNumeral}";

        string description = upgrade.description;
        if (!upgrade.Unlocked && upgrade.lockedAdditionalDescription is string lockedAdditionalDescription)
        {
            description += $"\n\n<sprite name=\"Lock\"> {lockedAdditionalDescription}";
        } else if (upgrade.Unlocked && upgrade.unlockedAdditionalDescription is string unlockedAdditionalDescription)
        {
            description += $"\n\n{unlockedAdditionalDescription}";
        }

        tooltipProvider.tooltipTitle = upgradeName;
        tooltipProvider.tooltipText = description;
        upgradeTitleLabel.text = upgradeName;

        var costText = "";

        for (var i = 0; i < upgrade.Costs.Count; i++)
        {
            var entry = upgrade.Costs.ElementAt(i);

            var valueStr = entry.Value > 1e4 ? string.Format("{0:0.##E+0}", entry.Value) : entry.Value.ToString();

            costText += $"<sprite name=\"{entry.Key}\"> {valueStr}";
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

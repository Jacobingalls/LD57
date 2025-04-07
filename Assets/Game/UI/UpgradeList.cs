using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeList : MonoBehaviour
{
    public GameObject UpgradeRowPrefab;

    public List<UpgradeType> upgrades = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Redraw();
    }

    public void Redraw()
    {
        for(var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }

        var rectTransform = GetComponent<RectTransform>();
        foreach (var upgradeType in upgrades) {
            var upgrade = UpgradeManager.Instance.GetUpgrade(upgradeType);
            if (upgrade.Hidden) {
                continue; 
            }
            if (upgrade.Complete && upgrade.hideOnComplete)
            {
                continue;
            }
            var upgradeRowGO = Instantiate(UpgradeRowPrefab);
            upgradeRowGO.GetComponent<RectTransform>().SetParent(rectTransform, false);
            upgradeRowGO.GetComponent<RectTransform>().name = upgradeType.ToString();

            var upgradeRow = upgradeRowGO.GetComponent<UpgradeRow>();
            upgradeRow.SetUpgradeType(upgradeType);
        }
    }
}

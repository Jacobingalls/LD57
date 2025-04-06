using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PeopleManager : MonoBehaviour
{
    public GameObject housePrefab;
    public GameObject largeHousePrefab;

    private List<PeopleCityLedge> _ledges;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ledges = GetComponentsInChildren<PeopleCityLedge>().ToList();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleSmallHouse, u =>
        {
            ConstructHouse();
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleLargeHouse, u =>
        {
            ConstructLargeHouse();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private PeopleCityLedge getNextLedge()
    {
        PeopleCityLedge bestLedge = null;

        for (int i = 0; i < _ledges.Count; i++)
        {
            var ledge = _ledges[i];
            if (ledge.CanBuildMore())
            {
                bestLedge = ledge;
                break;
            }
        }

        if (bestLedge == null)
        {
            return null;
        }

        var bestAmount = bestLedge.buildings.Count;
        for(int i = 0; i < _ledges.Count; i++)
        {
            var ledge = _ledges[i];
            if (ledge.buildings.Count < bestAmount)
            {
                bestAmount = ledge.buildings.Count;
                bestLedge = ledge;
            }
        }

        var ledgesForBestAmount = new List<PeopleCityLedge>();
        for (int i = 0; i < _ledges.Count; i++)
        {
            var ledge = _ledges[i];
            if (ledge.buildings.Count == bestAmount)
            {
                ledgesForBestAmount.Add(ledge);
            }
        }
        bestLedge = ledgesForBestAmount[Random.Range(0, ledgesForBestAmount.Count)];

        return bestLedge;
    }


    private void ConstructHouse(GameObject prefab)
    {
        getNextLedge().ConstructHouse(prefab);
    }

    public void ConstructHouse()
    {
        ConstructHouse(housePrefab);
    }

    public void ConstructLargeHouse()
    {
        ConstructHouse(largeHousePrefab);
    }
}

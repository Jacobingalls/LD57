using info.jacobingalls.jamkit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class PeopleManager : MonoBehaviour
{
    public GameObject housePrefab;
    public GameObject largeHousePrefab;

    private List<PeopleCityLedge> _ledges;


    public bool GeneratePeople;

    public float YeetPeopleCooldown = 1.0f;

    private int peopleToYeet = 0;
    private PubSubSender _pubSub;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ledges = GetComponentsInChildren<PeopleCityLedge>().ToList();
        _pubSub = GetComponent<PubSubSender>();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleSmallHouse, u =>
        {
            ConstructHouse();
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleLargeHouse, u =>
        {
            ConstructLargeHouse();
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleConvinceResidents, u =>
        {
            GeneratePeople = true;
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleBreedingPrograms, u =>
        {
            GeneratePeople = true;
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleHireRitekeeper, u =>
        {
            peopleToYeet += 1;
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleUnlockCathedral, u =>
        {
            GameObject.FindFirstObjectByType<Cathedral>(findObjectsInactive: FindObjectsInactive.Include).gameObject.SetActive(true);
        });
    }

    private float _currentYeetPeopleTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (peopleToYeet == 0)
        {
            return;
        }

        _currentYeetPeopleTime -= Time.deltaTime;
        if (_currentYeetPeopleTime < 0)
        {
            _pubSub.Publish("people.yeet.request");

            var targetCooldown = YeetPeopleCooldown / peopleToYeet;
            _currentYeetPeopleTime = targetCooldown;
        }
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

using info.jacobingalls.jamkit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MakoManager : MonoBehaviour
{
    [Header("Mako Orbs")]
    public GameObject makoOrbPrefab1;
    public GameObject makoOrbPrefab2;
    public GameObject makoOrbPrefab3;
    public Transform makoOrbSpawnPoint;

    public List<MakoOrb> makoOrbs = new();

    private MakoHarvester _harvester;
    private List<MakoAutoHarvester> _autoHarvesters;
    private List<MakoCollector> _collectors;
    private MakoAccelerator _accelerator;

    void Start()
    {
        _harvester = GetComponentInChildren<MakoHarvester>();
        _autoHarvesters = GetComponentsInChildren<MakoAutoHarvester>(includeInactive: true).ToList();
        _collectors = GetComponentsInChildren<MakoCollector>(includeInactive: true).ToList();
        _accelerator = GetComponentInChildren<MakoAccelerator>();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoManualAdditionalCollector, u => {
            var inactiveCollectors = _collectors.Where(c => c.gameObject.activeSelf == false && c.AllowsManual);
            if (inactiveCollectors.Count() == 0)
            {
                return;
            }
            var firstInactiveCollector = inactiveCollectors.First();
            firstInactiveCollector.gameObject.SetActive(true);
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoAutoAdditionalCollector, u => {
            var inactiveCollectors = _collectors.Where(c => c.gameObject.activeSelf == false && c.AllowsAuto);
            if (inactiveCollectors.Count() == 0)
            {
                return;
            }
            var firstInactiveCollector = inactiveCollectors.First();
            firstInactiveCollector.gameObject.SetActive(true);
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoAutoAdditionalHarvester, u => {
            var inactiveHarvesters = _autoHarvesters.Where(c => c.gameObject.activeSelf == false);
            if (inactiveHarvesters.Count() == 0)
            {
                return;
            }
            var firstInactiveHarvester = inactiveHarvesters.First();
            firstInactiveHarvester.gameObject.SetActive(true);
        });
        
        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.ResearchProcessingMoreMako, u => {
            ResourceManager.Instance.AddMultiplicativeModifier(ResourceType.Mako, 4.0f);
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoRefinementMoreValuableMako, u => {
            ResourceManager.Instance.AddAdditiveModifier(ResourceType.Mako, 10.0f);
        });
    }

    public void ModulePurchased()
    {
        ResourceManager.Instance.AddMultiplicativeModifier(ResourceType.Mako, 2.0f);
    }

    // We unlock higher level orbs with laser level, but it is random what kind of orb we get.
    private int pickRandomOrbLevel() {
        
        bool hasUnlockedCrystal = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementUnlockCrystal).timesPurchased > 0;
        int orbLevel = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementIncreaseLaserLevel).timesPurchased + 1;
        

        // The chances are determined by the laser level.
        // They are evaluated individually.
        float chanceOfOrbLevel2 = 0f;
        float chanceOfOrbLevel3 = 0f;

        if (hasUnlockedCrystal)
        {
            chanceOfOrbLevel2 = 0.1f;
            chanceOfOrbLevel3 = 0.01f;
        }

        if (orbLevel >= 2)
        {
            chanceOfOrbLevel2 += 0.1f;
        } 
        
        if (orbLevel >= 3)
        {
            chanceOfOrbLevel2 += 0.2f;
            chanceOfOrbLevel3 += 0.1f;
        }

        float randomValue = Random.Range(0f, 1f);
        if (randomValue < chanceOfOrbLevel3)
        {
            return 3;
        }
        else if (randomValue < chanceOfOrbLevel2)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    // As we increase the power of the *spookbuster 9001* we can spawn more orbs.
    private int pickNumberOfOrbs() {
        int numberOfOrbs = 1;
        int laserPower = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementIncreaseLaserPower).timesPurchased + 1;
        bool hasUnlockedCrystal = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementUnlockCrystal).timesPurchased > 0;
        int critChance = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoRefinementCriticalHitChance).timesPurchased * 2;
        

        if (hasUnlockedCrystal && Random.Range(0f, 1f) < 0.1f)
        {
            numberOfOrbs += 1;
        }

        if (laserPower >= 2 && Random.Range(0f, 1f) < 0.5f)
        {
            numberOfOrbs += 1;
        }

        if (laserPower >= 3 && Random.Range(0f, 1f) < 0.5f)
        {
            numberOfOrbs += 1;
        }

        if (critChance > Random.Range(0f, 100f))
        {
            numberOfOrbs *= 2;
        }

        return numberOfOrbs;
    }

    public void SpawnMakoOrbs(Vector3 spawnPosition) {
        for (int i = 0; i < pickNumberOfOrbs(); i++)
        {
            SpawnMakoOrb(spawnPosition);
        }
    }

    public void SpawnMakoOrb(Vector3 spawnPosition)
    {

        GameObject makoOrbPrefab = makoOrbPrefab1;
        int orbLevel = pickRandomOrbLevel();
        if (orbLevel == 2)
        {
            makoOrbPrefab = makoOrbPrefab2;
        }
        else if (orbLevel == 3)
        {
            makoOrbPrefab = makoOrbPrefab3;
        }

        var makoOrbGO = Instantiate(makoOrbPrefab);
        makoOrbGO.transform.position = spawnPosition;
        var makoOrb = makoOrbGO.GetComponent<MakoOrb>();
        const float impulseMin = 1.0f;
        const float impulseMax = 1.5f;
        makoOrb.ApplySpawningImpulse(Random.Range(impulseMin, impulseMax));
        makoOrb.transform.parent = transform;

        makoOrbs.Add(makoOrb);
    }

    public void ConsumeMakoOrb(MakoOrb makoOrb)
    {
        if (!makoOrbs.Contains(makoOrb))
        {
            Debug.LogError("Unable to consume mako orb not tracked by this mako manager.");
            return;
        }

        int gain = makoOrb.baseValue;
        makoOrbs.Remove(makoOrb);
        Destroy(makoOrb.gameObject);

        ResourceManager.Instance.IncrementResource(ResourceType.Mako, gain);
    }
}

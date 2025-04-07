using info.jacobingalls.jamkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// WARNING: You must not change the value of the enum after creating it, 
// the ids are what the editor stores the enum as, so if the id changes for a row, 
// the editor will reference the wrong upgrade.
public enum UpgradeType
{
    MakoClickAndHold = 0,
    MakoManualSummonIncrease = 1,
    MakoManualCollectIncrease = 2,
    MakoManualAdditionalCollector = 3,

    // MARK: - Mako Refinement
    MakoRefinementUnlockCrystal = 4,
    MakoRefinementIncreaseLaserLevel = 5,
    MakoRefinementIncreaseLaserPower = 9,
    MakoRefinementCriticalHitChance = 10,

    // MARK: - Research Processing
    ResearchProcessingUnlockFactory = 6,
    ResearchProcessingBuyBucket = 11,
    ResearchProcessingFasterChain = 12,
    ResearchProcessingIncresedEfficiency = 13,

    // MARK: - People Upgrades
    PeopleSmallHouse = 7,
    PeopleLargeHouse = 8,
}

public delegate void ApplyUpgradeEffectDelegate(Upgrade u);
public delegate void UpgradePurchasedDelegate(Upgrade u);

public class Upgrade
{
    public UpgradeType type;

    public Dictionary<ResourceType, int> baseCosts = new();
    public Dictionary<ResourceType, float> costsScaleFactors = new();

    public string name = "Upgrade";
    public string description = "Upgrade Description";

    public int timesPurchased = 0;
    public int maxPurchases = 1;

    public bool forcedLocked = false;
    public bool forcedHidden = false;

    public List<UpgradeType> requirements = new();
    public List<UpgradeType> hiddenRequirements = new();

    public event UpgradePurchasedDelegate UpgradePurchased;
    public ApplyUpgradeEffectDelegate ApplyUpgradeEffect;

    public List<String> pubSubNotifications = new();

    public bool Hidden
    {
        get
        {
            var requirementsHidden = requirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Hidden).Count() > 0;
            var hiddenRequirementsNotPurchased = hiddenRequirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Purchased == false).Count() > 0;
            return forcedHidden || requirementsHidden || hiddenRequirementsNotPurchased;
        }
    }
    public bool Unlocked
    {
        get
        {
            var requirementsPurchased = requirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Purchased == false).Count() == 0;
            var hiddenRequirementsPurchased = hiddenRequirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Purchased == false).Count() == 0;
            return !forcedLocked && requirementsPurchased && hiddenRequirementsPurchased;
        }
    }

    public bool Purchased
    {
        get
        {
            return timesPurchased > 0;
        }
    }

    public bool Complete
    {
        get
        {
            return timesPurchased == maxPurchases;
        }
    }

    public void Purchase()
    {
        timesPurchased += 1;
        UpgradePurchased?.Invoke(this);
        ApplyUpgradeEffect?.Invoke(this);
    }

    public Dictionary<ResourceType, int> Costs
    {
        get
        {
            return baseCosts.ToDictionary(kvp => kvp.Key, kvp => GetCost(kvp.Key));
        }
    }

    public int GetCost(ResourceType resourceType)
    {
        if (!baseCosts.ContainsKey(resourceType))
        {
            return 0;
        }
        var scaleFactor = 1.0f;
        if (costsScaleFactors.ContainsKey(resourceType))
        {
            scaleFactor = costsScaleFactors[resourceType];
        }
        return (int)(Mathf.Pow(scaleFactor, timesPurchased) * baseCosts[resourceType]);
    }
}

[RequireComponent(typeof(PubSubSender))]
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    private readonly Dictionary<UpgradeType, Upgrade> _upgrades = new();

    private PubSubSender _pubSub;

    public Upgrade GetUpgrade(UpgradeType upgradeType)
    {
        return _upgrades[upgradeType];
    }

    private void RegisterUpgrade(Upgrade upgrade)
    {
        _upgrades[upgrade.type] = upgrade;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _pubSub = GetComponent<PubSubSender>();

        RegisterUpgrade(new Upgrade 
        {
            type = UpgradeType.MakoClickAndHold,
            name = "Divine Guidance",
            description = "By the grace of God, we can begin to uncover the mysteries of this strange substance. Unlocks click + hold on the channeling device.",
            baseCosts =
            {
                [ResourceType.Mako] = 5,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualCollectIncrease,
            name = "Alignment",
            description = "Improve alignment of the collector arrays. Substance is attracted at a faster rate.",
            hiddenRequirements = { UpgradeType.MakoClickAndHold },
            baseCosts =
            {
                [ResourceType.Mako] = 10,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualSummonIncrease,
            name = "Calibration",
            description = "Calibrate the channeling device. Substance is drawn more easily from the depths.",
            hiddenRequirements = { UpgradeType.MakoClickAndHold },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 10,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2,
            }
        });


        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualAdditionalCollector,
            name = "Collector",
            description = "Construct an additional substance collector.",
            hiddenRequirements = { UpgradeType.MakoClickAndHold },
            baseCosts =
            {
                [ResourceType.Mako] = 25,
            },
        });

        // MARK: - Mako Refinement
        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoRefinementUnlockCrystal,
            name = "Construct Refinement Crystal",
            description = "Refines the beam to attract refined substance.",
            baseCosts =
            {
                [ResourceType.Mako] = 50,
            },
            pubSubNotifications = 
            { 
                "refinement.crystal.unlocked" 
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoRefinementIncreaseLaserLevel,
            name = "Attenuate",
            description = "Attenuates the crystal to increase the effectiveness of the beam on the substance. Increases the chance of attracting high-quality substance from the depths.",
            hiddenRequirements = { UpgradeType.MakoRefinementUnlockCrystal },
            maxPurchases = 2,
            baseCosts =
            {
                [ResourceType.Mako] = 50,
            },
            pubSubNotifications = 
            { 
                "refinement.crystal.attenuated" 
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoRefinementIncreaseLaserPower,
            name = "Amplify",
            description = "Increases the power of the beam to attract refined substance. Increases the chance for additional substance from the depths.",
            hiddenRequirements = { UpgradeType.MakoRefinementUnlockCrystal },
            maxPurchases = 2,
            baseCosts =
            {
                [ResourceType.Mako] = 50,
            },
            pubSubNotifications = 
            { 
                "refinement.crystal.amplified" 
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoRefinementCriticalHitChance,
            name = "Luck",
            description = "It's not luck, it's skill. A chance of getting double the substances. Each upgrade increases the chance by 1%.",
            hiddenRequirements = { UpgradeType.MakoRefinementUnlockCrystal },
            maxPurchases = 10,
            baseCosts =
            {
                [ResourceType.Mako] = 200,
            },
            pubSubNotifications = 
            { 
                "refinement.crystal.criticalHitChanceImproved" 
            }
        });

        // MARK: - Research Processing
        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingUnlockFactory,
            name = "Construe",
            description = "Construct a distillary to process the <i>Cognitio Aqua Diluta</i> from the depths.",
            baseCosts =
            {
                [ResourceType.Mako] = 100,
            },
            pubSubNotifications =
            {
                "research.factory.unlocked"
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingBuyBucket,
            name = "Situla",
            description = "Increase the amount of <i>Cognitio Aqua Diluta</i> drawn from the well. Add another bucket to the factory well.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 100,
            baseCosts =
            {
                [ResourceType.Mako] = 100,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 1.1f,
            },
            pubSubNotifications =
            {
                "research.factory.bucketAdded"
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingFasterChain,
            name = "Aliquid",
            description = "Draw the <i>Cognitio Aqua Diluta</i> faster from the well. Increases the speed of the chain.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 1000,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 10,
            },
            pubSubNotifications =
            {
                "research.factory.fasterChain"
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingIncresedEfficiency,
            name = "Studeo",
            description = "Learn how to use the <i>Cognitio Aqua Diluta</i> more efficiently. Increases the efficiency of the factory.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 1000,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 10,
            },
            pubSubNotifications =
            {
                "research.factory.increasedEfficiency"
            }
        });

        // MARK: - People

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleSmallHouse,
            name = "Cottage",
            description = "A damp, cold cottage. Provides housing for 1 mortal soul.",
            maxPurchases = 16,
            baseCosts =
            {
                [ResourceType.Mako] = 5,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2,
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleLargeHouse,
            name = "Flat",
            description = "A cramped, towering flat. Provides housing for 4 mortal souls.",
            maxPurchases = 16,
            baseCosts =
            {
                [ResourceType.Mako] = 25,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2,
            }
        });
    }

    public void RegisterUpgradePurchaseHandler(UpgradeType upgradeType, UpgradePurchasedDelegate handler)
    {
        var upgrade = GetUpgrade(upgradeType);
        upgrade.UpgradePurchased += handler;
    }

    public bool CanAfford(UpgradeType upgradeType)
    {
        var upgrade = GetUpgrade(upgradeType);
        foreach (var cost in upgrade.Costs)
        {
            if (ResourceManager.Instance.GetResourceAmount(cost.Key) < cost.Value)
            {
                return false;
            }
        }
        return true;
    }

    public bool Purchase(UpgradeType upgradeType)
    {
        var upgrade = GetUpgrade(upgradeType);

        if (!CanAfford(upgradeType))
        {
            return false;
        }

        if (upgrade.timesPurchased >= upgrade.maxPurchases)
        {
            return false;
        }

        foreach (var cost in upgrade.Costs)
        {
            ResourceManager.Instance.PayResource(cost.Key, cost.Value);
        }

        upgrade.Purchase();


        PubSubSender sender = GetComponent<PubSubSender>();
        if (sender != null) 
        {
            sender.Publish("upgrade.purchased");
            foreach (var notification in upgrade.pubSubNotifications)
            {
                _pubSub.Publish(notification);
            }
        }

        AudioManager.Instance.Play2D("Upgrade/Purchase", pitchMin: 0.9f, pitchMax: 1.1f);

        return true;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}

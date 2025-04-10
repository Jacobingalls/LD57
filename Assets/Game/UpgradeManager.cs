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
    // MARK: - Mako Mining
    MakoClickAndHold = 0,
    MakoManualSummonIncrease = 1,
    MakoManualCollectIncrease = 2,
    MakoManualAdditionalCollector = 3,
    MakoAutoAdditionalCollector = 100,
    MakoAutoAdditionalHarvester = 101,

    // MARK: - Mako Refinement
    MakoRefinementUnlockCrystal = 4,
    MakoRefinementIncreaseLaserLevel = 5,
    MakoRefinementIncreaseLaserPower = 9,
    MakoRefinementCriticalHitChance = 10,
    MakoRefinementMoreValuableMako = 205,

    // MARK: - Research Processing
    ResearchProcessingUnlockFactory = 6,
    ResearchProcessingBuyBucket = 11,
    ResearchProcessingFasterChain = 12,
    ResearchProcessingIncresedEfficiency = 13,
    ResearchProcessingMoreMako = 305,
    ResearchProcessingMoreMakoSuck = 306,

    // MARK: - People Upgrades
    PeopleSmallHouse = 7,
    PeopleLargeHouse = 8,
    PeopleUnlockLargeHouse = 403,
    PeopleUnlockCathedral = 404,
    PeopleConvinceResidents = 405,
    PeopleHireRitekeeper = 406,
    PeopleBreedingPrograms = 407,
    PeopleUpgradeRitekeeper = 408,

    // MARK: - Gate Upgrades
    GateStory1 = 501,
    GateStory2 = 502,
    GateStory3 = 503,
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
    public bool hideOnComplete = false;

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
        if (_upgrades.ContainsKey(upgrade.type))
        {
            Debug.LogError("Unable to register the same upgrade type multiple times! Keeping previous value.");
            return;
        }
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

        // MARK: - Mako Mining

        RegisterUpgrade(new Upgrade 
        {
            type = UpgradeType.MakoClickAndHold,
            name = "Divine Guidance",
            description = "By the grace of God, we can begin to uncover the mysteries of this strange substance. Unlocks click + hold on the channeling device.",
            baseCosts =
            {
                [ResourceType.Mako] = 1,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualCollectIncrease,
            name = "Alignment",
            description = "Improve alignment of the collector arrays. <sprite name=\"Mako\"> is attracted at a faster rate.",
            hiddenRequirements = { UpgradeType.MakoClickAndHold },
            baseCosts =
            {
                [ResourceType.Mako] = 5,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualSummonIncrease,
            name = "Calibration",
            description = "Calibrate the channeling device. <sprite name=\"Mako\"> is drawn more easily from the depths.",
            hiddenRequirements = { UpgradeType.MakoClickAndHold },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 5,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 3,
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualAdditionalCollector,
            name = "Collector",
            description = "Construct an additional <sprite name=\"Mako\"> collector.",
            hiddenRequirements = { UpgradeType.MakoClickAndHold },
            baseCosts =
            {
                [ResourceType.Mako] = 15,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoAutoAdditionalCollector,
            name = "Manifest Pylon",
            description = "Raise a small collector pylon. A mortal soul toils away, collecting <sprite name=\"Mako\"> sphaera from the skies.",
            hiddenRequirements = { UpgradeType.PeopleSmallHouse },
            maxPurchases = 6,
            baseCosts =
            {
                [ResourceType.Mako] = 10,
                [ResourceType.People] = 1,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2.0f,
                [ResourceType.People] = 1,
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoAutoAdditionalHarvester,
            name = "Channeler",
            description = "Raise a harvester platform from the depths. A mortal soul toils away, channeling for <sprite name=\"Mako\"> sphaera.",
            hiddenRequirements = { UpgradeType.PeopleSmallHouse },
            maxPurchases = 8,
            baseCosts =
            {
                [ResourceType.Mako] = 10,
                [ResourceType.People] = 1,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2.0f,
                [ResourceType.People] = 1,
            }
        });

        // MARK: - People

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleSmallHouse,
            name = "Cottage",
            description = "A damp, cold cottage, provides housing for a mortal soul. +1 <sprite name=\"People\">",
            maxPurchases = 16,
            baseCosts =
            {
                [ResourceType.Mako] = 5,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 1.5f,
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleUnlockLargeHouse,
            name = "Sturdier Supports",
            description = "Reinforce New Duskhollow's support structures. Permits the construction of larger Flats.",
            maxPurchases = 1,
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            baseCosts =
            {
                [ResourceType.Mako] = 500,
                [ResourceType.Science] = 5,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleLargeHouse,
            name = "Flat",
            description = "A cramped, towering flat for mortal souls. +2 <sprite name=\"People\">",
            maxPurchases = 16,
            hiddenRequirements = { UpgradeType.PeopleUnlockLargeHouse },
            baseCosts =
            {
                [ResourceType.Mako] = 500,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 1.5f,
            }
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleUnlockCathedral,
            name = "Cathedral",
            description = "A fine place for a fine end.",
            hiddenRequirements = { UpgradeType.GateStory3 },
            baseCosts =
            {
                [ResourceType.Mako] = 100000,
                [ResourceType.Science] = 100,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleConvinceResidents,
            name = "Extended Flock",
            description = "Persuasive letters from residents fly across distant lands, luring new blood to New Gloomhollow. Each home provides passive <sprite name=\"People\"> generation.",
            hiddenRequirements = { UpgradeType.PeopleUnlockCathedral },
            baseCosts =
            {
                [ResourceType.Mako] = (int)1e5,
                [ResourceType.Science] = 100,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleHireRitekeeper,
            name = "Ritekeeper",
            description = "Leads willing and unwilling alike to meet their fate.",
            hiddenRequirements = { UpgradeType.PeopleUnlockCathedral },
            maxPurchases = 8,
            baseCosts =
            {
                [ResourceType.Mako] = (int)1e5,
                [ResourceType.People] = 1,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 1.1f,
                [ResourceType.People] = 1.0f,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleBreedingPrograms,
            name = "Fecund Harvest",
            description = "It is the solemn duty of each soul in New Gloomhollow to produce and raise the next generation. For the greater good. Each home provides additional passive <sprite name=\"People\"> generation.",
            hiddenRequirements = { UpgradeType.PeopleConvinceResidents },
            baseCosts =
            {
                [ResourceType.Mako] = (int)2e6,
                [ResourceType.Science] = 100,
            },
            ApplyUpgradeEffect = u => {
                ResourceManager.Instance.AddAdditiveModifier(ResourceType.People, 1);
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.PeopleUpgradeRitekeeper,
            name = "Twin Reaping Rite",
            description = "The darkness in the depths must be sated. Two hapless souls are now cast into the gate's maw by the righteous Ritekeeper.",
            hiddenRequirements = { UpgradeType.PeopleHireRitekeeper },
            baseCosts =
            {
                [ResourceType.Mako] = (int)1e6,
                [ResourceType.Science] = 200,
            },
        });

        // MARK: - Mako Refinement
        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoRefinementUnlockCrystal,
            name = "Grow Crystal",
            description = "Alters the beam to attract refined <sprite name=\"Mako\"> sphaera.",
            hideOnComplete = true,
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
            description = "Attenuates the crystal to increase the effectiveness of the beam on the substance. Increases the chance of attracting high-quality <sprite name=\"Mako\"> sphaera from the depths.",
            hiddenRequirements = { UpgradeType.MakoRefinementUnlockCrystal },
            maxPurchases = 2,
            baseCosts =
            {
                [ResourceType.Mako] = 50,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2.0f,
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
            description = "Increases the power of the beam to attract refined substance. Increases the chance for additional <sprite name=\"Mako\"> sphaera from the depths.",
            hiddenRequirements = { UpgradeType.MakoRefinementUnlockCrystal },
            maxPurchases = 2,
            baseCosts =
            {
                [ResourceType.Mako] = 50,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2.0f,
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
            description = "It's not luck, it's skill. A chance of getting double <sprite name=\"Mako\"> sphaera from the depths. Each upgrade increases the chance by 2%.",
            hiddenRequirements = { UpgradeType.MakoRefinementUnlockCrystal },
            maxPurchases = 10,
            baseCosts =
            {
                [ResourceType.Mako] = 200,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 1.5f,
            },
            pubSubNotifications = 
            { 
                "refinement.crystal.criticalHitChanceImproved" 
            }
        }); 

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoRefinementMoreValuableMako,
            name = "Imbumentum",
            description = "Imbue the crystal with <i>Cognitio Aqua Diluta</i>. Each upgrade increases the base value of <sprite name=\"Mako\"> sphaera by 10.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 1000,
                [ResourceType.Science] = 2,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 2.0f,
                [ResourceType.Science] = 2.5f,
            },
        });

        // MARK: - Research Processing
        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingUnlockFactory,
            name = "Construe",
            description = "Construct a distillary to process the <i>Cognitio Aqua Diluta</i> from the depths, a powerful liquid capable of being distilled into <sprite name=\"Science\">.",
            hideOnComplete = true,
            baseCosts =
            {
                [ResourceType.Mako] = 100,
                [ResourceType.People] = 2,
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
            description = "Increase the amount of <i>Cognitio Aqua Diluta</i> drawn from the well. Add another bucket to the distillery well.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 100,
            baseCosts =
            {
                [ResourceType.Mako] = 50,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 1.13f,
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
                [ResourceType.Mako] = 250,
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
            description = "Learn how to use the <i>Cognitio Aqua Diluta</i> more efficiently. Reduces <i>Cognitio Aqua Diluta</i> needed by the distillary to produce <sprite name=\"Science\">.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 250,
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

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingMoreMako,
            name = "Intellectus",
            description = "Understand the link between <i>Cognitio Aqua Diluta</i> and the substance. (2x <sprite name=\"Mako\"> extracted)",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 100,
                [ResourceType.Science] = 1,
                [ResourceType.People] = 1,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 10,
                [ResourceType.Science] = 2,
                [ResourceType.People] = 1,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.ResearchProcessingMoreMakoSuck,
            name = "Divina Guidantia",
            description = "Per Dei gratiam pergere possumus mysteria huius substantiae alienae detegere. <sprite name=\"Mako\"> is attracted at a faster rate.",
            hiddenRequirements = { UpgradeType.ResearchProcessingUnlockFactory },
            maxPurchases = 3,
            baseCosts =
            {
                [ResourceType.Mako] = 200,
                [ResourceType.Science] = 2,
                [ResourceType.People] = 1,
            },
            costsScaleFactors =
            {
                [ResourceType.Mako] = 10,
                [ResourceType.Science] = 2,
                [ResourceType.People] = 1,
            },
        });

        // MARK: - Gate

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.GateStory1,
            name = "Investigate",
            description = "Fragmented ancient carvings whisper of riches held beyond the gate. What little can be gleaned points unerringly back to Old Gloomhollow's darkest secrets.",
            baseCosts =
            {
                [ResourceType.Mako] = (int)1e4,
                [ResourceType.Science] = 5,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.GateStory2,
            name = "Study",
            description = "Worm-eaten tomes and half-deciphered symbols unveil a terrifying truth - but those who remain skeptical demand further study.",
            hiddenRequirements = { UpgradeType.GateStory1 },
            baseCosts =
            {
                [ResourceType.Mako] = (int)1e5,
                [ResourceType.Science] = 25,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.GateStory3,
            name = "Understand",
            description = "At last the truth is laid bare. The gate demands blood from New Gloomhollow to break its seal. No denial remains - its very mechanisms feed upon the despair of lost souls, twisting their fates into a key for entry.",
            hiddenRequirements = { UpgradeType.GateStory2 },
            baseCosts =
            {
                [ResourceType.Mako] = (int)1e6,
                [ResourceType.Science] = 100,
            },
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
            sender.Publish($"upgrade.purchased.{upgrade.type}");
            foreach (var notification in upgrade.pubSubNotifications)
            {
                _pubSub.Publish(notification);
            }
        }

        AudioManager.Instance.Play2D("Upgrade/Purchase", volumeMin: 0.2f, volumeMax: 0.4f);

        return true;
    }

    public void Reset()
    {
        foreach (var upgrade in _upgrades.Values)
        {
            upgrade.timesPurchased = 0;
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}

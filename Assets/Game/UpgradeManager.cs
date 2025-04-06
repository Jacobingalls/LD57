using info.jacobingalls.jamkit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum UpgradeType
{
    MakoClickAndHold,
    MakoManualSummonIncrease,
    MakoManualCollectIncrease
}

public delegate void ApplyUpgradeEffectDelegate(Upgrade u);

public class Upgrade
{
    public UpgradeType type;

    public Dictionary<ResourceType, int> cost = new();
    public Dictionary<ResourceType, float> costScaleFactor = null;

    public string name = "Upgrade";
    public string description = "Upgrade Description";

    public int timesPurchased = 0;
    public int maxPurchases = 1;

    public bool forcedLocked = false;
    public bool forcedHidden = false;

    public List<UpgradeType> requirements = new();

    public ApplyUpgradeEffectDelegate ApplyUpgradeEffect;

    public bool Hidden
    {
        get
        {
            var requirementsHidden = requirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Hidden).Count() > 0;
            return forcedHidden || requirementsHidden;
        }
    }
    public bool Unlocked
    {
        get
        {
            var requirementsPurchased = requirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Purchased == false).Count() == 0;
            Debug.Log($"{type} Requirements Purchased " + requirements.Where(r => UpgradeManager.Instance.GetUpgrade(r).Purchased == false).Count());
            return !forcedLocked && requirementsPurchased;
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
}

[RequireComponent(typeof(PubSubSender))]
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    private readonly Dictionary<UpgradeType, Upgrade> _upgrades = new();

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

        RegisterUpgrade(new Upgrade 
        {
            type = UpgradeType.MakoClickAndHold,
            name = "Divine Guidance",
            description = "By the grace of God, we can begin to uncover the mysteries of this strange substance. Unlocks click + hold on the channeling device.",
            cost =
            {
                [ResourceType.Mako] = 5,
            },
            ApplyUpgradeEffect = (u =>
            {
                Debug.Log($"Purchased {u}!");
            })
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualCollectIncrease,
            name = "Alignment",
            requirements = { UpgradeType.MakoClickAndHold },
            cost =
            {
                [ResourceType.Mako] = 10,
            },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualSummonIncrease,
            name = "Calibration",
            requirements = { UpgradeType.MakoClickAndHold },
            cost =
            {
                [ResourceType.Mako] = 10,
            },
        });
    }

    public bool CanAfford(UpgradeType upgradeType)
    {
        var upgrade = GetUpgrade(upgradeType);
        foreach (var cost in upgrade.cost)
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

        foreach (var cost in upgrade.cost)
        {
            ResourceManager.Instance.PayResource(cost.Key, cost.Value);
        }

        upgrade.timesPurchased += 1;

        upgrade.ApplyUpgradeEffect?.Invoke(upgrade);

        GetComponent<PubSubSender>().Publish("upgrade.purchased");

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

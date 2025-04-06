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
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualCollectIncrease,
            name = "Alignment",
            requirements = { UpgradeType.MakoClickAndHold },
        });

        RegisterUpgrade(new Upgrade
        {
            type = UpgradeType.MakoManualSummonIncrease,
            name = "Calibration",
            requirements = { UpgradeType.MakoClickAndHold },
        });
    }

    public bool Purchase(UpgradeType upgradeType)
    {
        var upgrade = GetUpgrade(upgradeType);

        upgrade.timesPurchased += 1;

        GetComponent<PubSubSender>().Publish("upgrade.purchased");

        return true;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}

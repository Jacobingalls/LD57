using info.jacobingalls.jamkit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResourceType
{
    Mako,
    People,
    Science,
    Artifacts,
}

public class Resource
{
    public ResourceType type;

    // DO NOT directly manipulate
    public int amount = 0;

    // DO NOT directly manipulate
    public int baseGain = 1;

    // DO NOT directly manipulate
    public bool unlocked = false;

    // DO NOT directly manipulate
    public bool hidden = false;
}

[System.Serializable]
public class ResourceSpriteConfig
{
    public ResourceType Type;
    public Sprite Sprite;
}

[RequireComponent(typeof(PubSubSender))]
public class ResourceManager : MonoBehaviour
{
    public bool cheatMode;

    public static ResourceManager Instance { get; private set; }
    private readonly Dictionary<ResourceType, Resource> _resources = new();

    private readonly Dictionary<ResourceType, List<float>> _additiveModifiers = new();
    private readonly Dictionary<ResourceType, List<float>> _mutliplicativeModifiers = new();

    [SerializeField]
    public List<ResourceSpriteConfig> _resourceSpriteConfigs = new();

    public int Mako
    {
        get
        {
            return _resources[ResourceType.Mako].amount;
        }
        set
        {
            _resources[ResourceType.Mako].amount = value;
        }
    }

    public int People
    {
        get
        {
            return _resources[ResourceType.People].amount;
        }
        set
        {
            _resources[ResourceType.People].amount = value;
        }
    }

    public int Science
    {
        get
        {
            return _resources[ResourceType.Science].amount;
        }
        set
        {
            _resources[ResourceType.Science].amount = value;
        }
    }

    public int Artifacts
    {
        get
        {
            return _resources[ResourceType.Artifacts].amount;
        }
        set
        {
            _resources[ResourceType.Artifacts].amount = value;
        }
    }
    private Resource GetResource(ResourceType resourceType)
    {
        return _resources[resourceType];
    }

    public int GetResourceAmount(ResourceType resourceType)
    {
        return _resources[resourceType].amount;
    }
    public int PayResource(ResourceType resourceType, int amount)
    {
        return _resources[resourceType].amount -= amount;
    }

    public Sprite GetResourceSprite(ResourceType resourceType)
    {
        foreach (var r in _resourceSpriteConfigs)
        {
            if (r.Type == resourceType)
            {
                return r.Sprite;
            }
        }

        return null;
    }

    public List<Resource> GetUnlockedResources() => _resources.Values.Where(r => r.unlocked).ToList();

    public void AddAdditiveModifier(ResourceType resourceType, float modifier)
    {
        _additiveModifiers[resourceType].Add(modifier);
        Debug.Log($"{resourceType}: insert {modifier} additive modifier");
    }

    public void AddMultiplicativeModifier(ResourceType resourceType, float modifier)
    {
        _mutliplicativeModifiers[resourceType].Add(modifier);
        Debug.Log($"{resourceType}: insert {modifier} multiplicative modifier");
    }

    /// <summary>
    /// Increments the resource according to active modifiers.
    /// </summary>
    /// <param name="resourceType"></param>
    /// <param name="gain">Number of times to add the resource. Each addition takes into account modifiers.</param>
    public void IncrementResource(ResourceType resourceType, int gain = 1)
    {
        var resource = GetResource(resourceType);

        float additiveModifier = _additiveModifiers[resourceType].Aggregate(0.0f, (cur, next) => cur + next);
        float multiplicativeModifier = _mutliplicativeModifiers[resourceType].Aggregate(1.0f, (cur, next) => cur * next);

        var resourceGain = resource.baseGain * gain * additiveModifier * multiplicativeModifier;

        Debug.Log("additive modifier = " + additiveModifier);
        Debug.Log("multiplicativeModifier = " + additiveModifier);

        resource.amount += (int)resourceGain;
    }

    public void UnlockResource(ResourceType resourceType)
    {
        var resource = GetResource(resourceType);
        if (resource != null)
        {
            resource.unlocked = true;
            GetComponent<PubSubSender>().Publish("resource.unlocked");
        }
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

        _resources[ResourceType.Mako] = new Resource
        {
            type = ResourceType.Mako,
            unlocked = true,
        };

        _resources[ResourceType.People] = new Resource
        {
            type = ResourceType.People,
        };

        _resources[ResourceType.Science] = new Resource
        {
            type = ResourceType.Science,
        };

        _resources[ResourceType.Artifacts] = new Resource
        {
            type = ResourceType.Artifacts,
        };

        foreach(var entry in _resources)
        {
            _additiveModifiers[entry.Key] = new List<float> { 1.0f };
            _mutliplicativeModifiers[entry.Key] = new List<float> { 1.0f };
        }
    }

    void Start()
    {
        if (cheatMode)
        {
            foreach(var entry in _resources)
            {
                entry.Value.unlocked = true;
                entry.Value.amount = (int)10e5;
            }
            GetComponent<PubSubSender>().Publish("resource.unlocked");
        }
    }

    void Update()
    {
        
    }
}

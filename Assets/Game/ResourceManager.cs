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

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    private readonly Dictionary<ResourceType, Resource> _resources = new();

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

    /// <summary>
    /// Increments the resource according to active modifiers.
    /// </summary>
    /// <param name="resourceType"></param>
    public void IncrementResource(ResourceType resourceType)
    {
        var resource = GetResource(resourceType);
        var resourceGain = resource.baseGain;

        // TODO: calculate modifiers

        resource.amount += resourceGain;
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
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}

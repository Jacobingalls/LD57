using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cathedral : MonoBehaviour
{
    public GameObject sacrificedMortalSoulPrefab;
    public Transform peopleSpawnPoint;

    [Range(0.0f, 20.0f)]
    public float initialForce = 5.0f;
    [Range(0.0f, 1.0f)]
    public float forceVariance = 0.2f;

    [Range(0.0f, 20.0f)]
    public float initialTorque = 1.0f;
    [Range(0.0f, 1.0f)]
    public float torqueVariance = 0.2f;

    public float minDirectionX = -0.2f;
    public float maxDirectionX = 0.2f;

    private List<SpriteFloat> _spriteFloats = new();

    public bool doubleYeetUnlocked = false;

    void Start()
    {
        _spriteFloats = GetComponentsInChildren<SpriteFloat>().ToList();

        if (UpgradeManager.Instance.GetUpgrade(UpgradeType.PeopleUnlockCathedral).Complete == false)
        {
            gameObject.SetActive(false);
        }

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.PeopleUpgradeRitekeeper, u =>
        {
            doubleYeetUnlocked = true;
        });
    }

    void Update()
    {
        if (debug)
        {
            if (_debugTimer < 0)
            {
                if (doubleYeetUnlocked)
                {
                    var pixelsPerUnit = 16;
                    Vector3 offset = new Vector3(2.0f / pixelsPerUnit, 0.0f, 0.0f);
                    YeetMortalSoul(peopleSpawnPoint.transform.position - offset);
                    YeetMortalSoul(peopleSpawnPoint.transform.position + offset);
                }
                else
                {
                    YeetMortalSoul();
                }
                _debugTimer = _debugSpawnInterval;
            }
            else
            {
                _debugTimer -= Time.deltaTime;
            }
        }
    }

    public bool debug = false;
    private float _debugTimer = 0.0f;
    private float _debugSpawnInterval = 0.1f;

    public void YeetMortalSoul()
    {
        if (doubleYeetUnlocked)
        {
            var pixelsPerUnit = 16;
            Vector3 offset = new Vector3(2.0f / pixelsPerUnit, 0.0f, 0.0f);
            YeetMortalSoul(peopleSpawnPoint.transform.position - offset);
            YeetMortalSoul(peopleSpawnPoint.transform.position + offset);
        }
        else
        {
            YeetMortalSoul(peopleSpawnPoint.transform.position);
        }
    }

    public void YeetMortalSoul(Vector3 spawnPosition)
    {
        var people = ResourceManager.Instance.GetResourceAmount(ResourceType.People);
        if (people == 0)
        {
            return;
        }
        ResourceManager.Instance.PayResource(ResourceType.People, 1);

        var go = Instantiate(sacrificedMortalSoulPrefab);

        go.transform.parent = transform;
        go.transform.position = spawnPosition;

        var rb = go.GetComponent<Rigidbody2D>();
        var forceDirection = new Vector3(Random.Range(minDirectionX, maxDirectionX), Random.Range(-1.0f, -0.5f), 0.0f).normalized;
        var force = forceDirection * Random.Range(initialForce - (initialForce * forceVariance), initialForce + (initialForce * forceVariance));
        rb.AddForce(force, ForceMode2D.Impulse);

        var torque = Random.Range(initialTorque - (initialTorque * torqueVariance), initialTorque + (initialTorque * torqueVariance));
        rb.AddTorque(initialTorque);

        foreach(var sf in _spriteFloats)
        {
            sf.Jitter(0.25f);
        }
    }
}

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MakoManager : MonoBehaviour
{
    [Header("Mako Orbs")]
    public GameObject makoOrbPrefab;
    public Transform makoOrbSpawnPoint;

    public List<MakoOrb> makoOrbs = new();

    private MakoHarvester _harvester;
    private List<MakoCollector> _collectors;
    private MakoAccelerator _accelerator;

    void Start()
    {
        _harvester = GetComponentInChildren<MakoHarvester>();
        _collectors = GetComponentsInChildren<MakoCollector>(includeInactive: true).ToList();
        _accelerator = GetComponentInChildren<MakoAccelerator>();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoManualAdditionalCollector, u => {
            var firstInactiveCollector = _collectors.Where(c => c.gameObject.activeSelf == false).First();
            if (firstInactiveCollector != null)
            {
                firstInactiveCollector.gameObject.SetActive(true);
            }
        });
    }
    public void SpawnMakoOrb(Vector3 spawnPosition)
    {
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

        makoOrbs.Remove(makoOrb);
        Destroy(makoOrb.gameObject);

        ResourceManager.Instance.IncrementResource(ResourceType.Mako);
    }
}

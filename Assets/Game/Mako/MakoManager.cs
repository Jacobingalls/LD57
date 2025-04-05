using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MakoManager : MonoBehaviour
{
    [Header("Mako Orbs")]
    public GameObject makoOrbPrefab;
    public Transform makoOrbSpawnPoint;

    public List<MakoOrb> makoOrbs = new();

    private MakoHarvester _harvester;
    private MakoCollector _collector;
    private MakoAccelerator _accelerator;

    void Start()
    {
        _harvester = GetComponentInChildren<MakoHarvester>();
        _collector = GetComponentInChildren<MakoCollector>();
        _accelerator = GetComponentInChildren<MakoAccelerator>();
    }

    void Update()
    {
        
    }

    public void SpawnMakoOrb(Vector3 spawnPosition)
    {
        var makoOrbGO = Instantiate(makoOrbPrefab);
        makoOrbGO.transform.position = spawnPosition;
        var makoOrb = makoOrbGO.GetComponent<MakoOrb>();
        const float impulseMin = 1.0f;
        const float impulseMax = 1.5f;
        makoOrb.ApplySpawningImpulse(Random.Range(impulseMin, impulseMax));

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

        LD57GameManager.Instance.Mako += 100;
    }
}

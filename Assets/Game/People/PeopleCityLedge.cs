using System.Collections.Generic;
using UnityEngine;

public class PeopleCityLedge : MonoBehaviour
{
    [Range(0, 10)]
    public int maxBuildings = 6;
    public GameObject ledgeGO;

    public List<PeopleBuilding> buildings = new();
    private GameObject _buildPoint;
    private GameObject _supportsVisual;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _buildPoint = new GameObject("Build Point");
        _buildPoint.transform.parent = transform;
        _buildPoint.transform.position = transform.position;

        _supportsVisual = gameObject.transform.Find("SupportsVisual").gameObject;
        _supportsVisual.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanBuildMore()
    {
        return buildings.Count < maxBuildings;
    }

    public void ConstructHouse(GameObject prefab)
    {
        if (buildings.Count == maxBuildings) {
            Debug.LogError("Unable to construct a house, max building limit reached!");
            return; 
        }

        _supportsVisual.SetActive(true);
        var go = Instantiate(prefab);

        var sizeProvider = go.GetComponent<SizeProvider>();
        var house = go.GetComponent<PeopleBuilding>();

        var halfOffset = new Vector3(sizeProvider.size.x / 2.0f, 0.0f, 0.0f);
        go.transform.parent = transform;
        go.transform.localPosition = _buildPoint.transform.localPosition - halfOffset;

        var offset = new Vector3(sizeProvider.size.x, 0.0f, 0.0f);
        _buildPoint.transform.localPosition -= offset;

        buildings.Add(house);
        house.Construct();

        ledgeGO.transform.position += (offset * transform.localScale.x);
    }
}

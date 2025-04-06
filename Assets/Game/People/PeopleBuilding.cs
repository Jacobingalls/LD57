using UnityEngine;

public enum HouseType
{
    Small,
    Medium,
    Large
}

public class PeopleBuilding : MonoBehaviour
{
    [Range(0, 10)]
    public int basePeopleGain = 1;

    public HouseType houseType = HouseType.Small;
    public int currentLevel = 0;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Construct()
    {
        ResourceManager.Instance.IncrementResource(ResourceType.People, basePeopleGain);
        AudioManager.Instance.Play2D("People/Building/Construct", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
    }
}
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

    [Range(0, 10.0f)]
    public float PeopleGenerationTime = 2.0f;
    private float _peopleGenerationTimer = 0.0f;

    private PeopleManager _peopleManager;

    private void Start()
    {
        _peopleGenerationTimer = Random.Range(0.0f, PeopleGenerationTime);
        _peopleManager = GetComponentInParent<PeopleManager>();
    }

    void Update()
    {
        if (!_peopleManager.GeneratePeople)
        {
            return;
        }

        if (_peopleGenerationTimer < 0)
        {
            _peopleGenerationTimer = PeopleGenerationTime;
            GeneratePeople();
        }
        else
        {
            _peopleGenerationTimer -= Time.deltaTime;
        }
    }

    public void GeneratePeople()
    {
        var sf = GetComponentInChildren<SpriteFloat>();
        sf.Jitter(0.25f);

        ResourceManager.Instance.IncrementResource(ResourceType.People);
    }

    public void Construct()
    {
        ResourceManager.Instance.IncrementResource(ResourceType.People, basePeopleGain);
        AudioManager.Instance.Play2D("People/Building/Construct", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
    }
}
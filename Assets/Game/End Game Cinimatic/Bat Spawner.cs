using UnityEngine;

public class BatSpawner : MonoBehaviour
{

    float width = 10f;
    public float rate = 50f;

    float shouldSpawn = 0f;

    float batSpeed = 5f;

    public GameObject batPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shouldSpawn += Time.deltaTime * rate;


        while (shouldSpawn >= 1f)
        {
            shouldSpawn -= 1f;
            SpawnBat();
        }
    }

    void SpawnBat()
    {
        float x = Random.Range(-width, width);
        GameObject bat = Instantiate(batPrefab, transform);
        bat.transform.localPosition = new Vector3(x, 0f, 0f);
        bat.GetComponent<BatController>().speed = batSpeed;
    }

    public void SetSpawnRate(float rate)
    {
        this.rate = rate;
    }

    public void SetSpeed(float speed)
    {
        this.batSpeed = speed;
    }
}

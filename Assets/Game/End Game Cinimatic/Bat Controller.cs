using UnityEngine;

public class BatController : MonoBehaviour
{

    public float jitter = 1f;
    public float speed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 position = transform.localPosition;
        position.x += Random.Range(-jitter, jitter) * Time.deltaTime;
        position.y += Random.Range(-jitter, jitter) * Time.deltaTime;
        position.y += speed * Time.deltaTime;
        transform.localPosition = position;
        
        if (position.y > 100f)
        {
            Destroy(gameObject);
        }
    }
}

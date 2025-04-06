using UnityEngine;

public class KnowledgeParticle : MonoBehaviour
{

    public float speed = 100f;
    public float acceleration = -1f;
    public float minSpeed = 1f;
    public float maxY = 0f;

    public ResearchCenter researchCenter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speed = Mathf.Max(speed + (acceleration * Time.deltaTime), minSpeed);
        transform.position += Vector3.up * speed * Time.deltaTime;

        if (transform.position.y >= maxY)
        {
            researchCenter.KnoledgeParticleDidReachTop(this);
            Destroy(gameObject);
        }
    }
}

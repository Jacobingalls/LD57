using UnityEngine;

public class KnowledgeGuage : MonoBehaviour
{
    
    public Vector3 startPosition;
    public float percentage;
    public Vector2 maxHeight;

    public SpriteRenderer spriteRenderer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        maxHeight = spriteRenderer.size;
        startPosition = transform.position;
        spriteRenderer.size = maxHeight * new Vector2(1, percentage);
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.size = maxHeight * new Vector2(1, percentage);
        transform.position = startPosition - new Vector3(0, maxHeight.y / 2, 0) + new Vector3(0, spriteRenderer.size.y / 2, 0);
    }
}

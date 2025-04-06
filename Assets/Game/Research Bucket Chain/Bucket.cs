using UnityEngine;

public class Bucket : MonoBehaviour
{

    public bool isGoingUp = true;

    public bool isBucket = true;
    
    public BucketChain chain;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.localPosition;
        currentPosition.y += (isGoingUp ? 1 : -1) * chain.chainSpeed * Time.deltaTime;
        transform.localPosition = currentPosition;

        if (isGoingUp && currentPosition.y >= chain.topY)
        {
            chain.BucketDidReachTop(this);
        }
        else if (!isGoingUp && currentPosition.y <= chain.bottomY)
        {
            chain.BucketDidReachBottom(this);
        }
    }
}

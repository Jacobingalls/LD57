using UnityEngine;

public class BucketChain : MonoBehaviour
{

    public Bucket bucketGoingUpPrefab;
    public Bucket bucketGoingDownPrefab;
    public Bucket chainGoingUpPrefab;
    public Bucket chainGoingDownPrefab;

    public float chainSpeed = 1f;

    public float topY = 0f;
    public float bottomY = -10f;

    public int bucketFreeCount = 0;

    private Bucket lastBucketGoingDown;

    private Bucket lastBucketGoingUp;

    public UnityEngine.Events.UnityEvent onNewBucketOfGoo;

    public void SetSpeed(float speed)
    {
        chainSpeed = speed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateEmptyChain(bottomY, topY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateEmptyChain(float top, float bottom) {
        int start = Mathf.RoundToInt(top);
        int end = Mathf.RoundToInt(bottom);
        for (int i = start; i < end; i++) {
            Bucket newBucket = Instantiate(chainGoingUpPrefab, transform);
            newBucket.chain = this;
            newBucket.isGoingUp = true;
            newBucket.transform.localPosition = new Vector3(
                newBucket.transform.localPosition.x, 
                i, 
                newBucket.transform.localPosition.z
            );
            lastBucketGoingUp = newBucket;
        }
        for (int i = start + 1; i <= end; i++) {
            Bucket newBucket = Instantiate(chainGoingDownPrefab, transform);
            newBucket.chain = this;
            newBucket.isGoingUp = false;
            newBucket.transform.localPosition = new Vector3(
                newBucket.transform.localPosition.x, 
                i, 
                newBucket.transform.localPosition.z
            );
            lastBucketGoingDown = newBucket;
        }
    }

    

    public void BucketDidReachTop(Bucket bucket) {
        float error = bucket.transform.localPosition.y - topY;
        if (bucket.isBucket) {
            onNewBucketOfGoo.Invoke();
        }

        Destroy(bucket.gameObject);

        Bucket prefab = chainGoingDownPrefab;
        if (bucket.isBucket) {
            prefab = bucketGoingDownPrefab;
        } else if (bucketFreeCount > 0) {
            bucketFreeCount--;
            prefab = bucketGoingDownPrefab;
        }

        // Instantiate a new bucket going down
        Bucket newBucket = Instantiate(prefab, transform);
        newBucket.chain = this;
        newBucket.isGoingUp = false;
        newBucket.transform.localPosition = new Vector3(
            newBucket.transform.localPosition.x, 
            topY - error, 
            newBucket.transform.localPosition.z
        );
        lastBucketGoingDown = newBucket;
    }

    public void BucketDidReachBottom(Bucket bucket) {
        float error = bottomY-bucket.transform.localPosition.y;
        Destroy(bucket.gameObject);

        Bucket prefab = bucket.isBucket ? bucketGoingUpPrefab : chainGoingUpPrefab;

        // Instantiate a new bucket going up
        Bucket newBucket = Instantiate(prefab, transform);
        newBucket.chain = this;
        newBucket.isGoingUp = true;
        newBucket.transform.localPosition = new Vector3(
            newBucket.transform.localPosition.x, 
            bottomY + error, 
            newBucket.transform.localPosition.z
        );
        lastBucketGoingUp = newBucket;
    }
}

using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float MinHeight = 0f;
    public float MaxHeight = 10f;

    public float keyHoldTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveSpeed = Mathf.Min(20f * Mathf.Pow(2f, 10f * keyHoldTime), 100f);
        Vector3 position = transform.position;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            position.y += moveSpeed * Time.deltaTime;
            keyHoldTime += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            position.y -= moveSpeed * Time.deltaTime;
            keyHoldTime += Time.deltaTime;
        }
        else if (Input.mouseScrollDelta.y != 0)
        {
            position.y += Input.mouseScrollDelta.y * 100 * Time.deltaTime;
            // Don't use keyhold time as users can control speed.
        } 
        else 
        {
            keyHoldTime = 0;
        }

        position.y = Mathf.Clamp(position.y, MinHeight, MaxHeight);
        transform.position = position;
    }
}

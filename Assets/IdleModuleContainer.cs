using UnityEngine;

public class IdleModuleContainer : MonoBehaviour
{

    private CameraController cameraController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
        (float minY, float maxY) = GetMinMaxGlobalPositionY();
        cameraController.MinHeight = minY;
        cameraController.MaxHeight = maxY;
        cameraController.transform.position = new Vector3(
            cameraController.transform.position.x, 
            maxY, 
            cameraController.transform.position.z
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public (float minY, float maxY) GetMinMaxGlobalPositionY()
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<CameraControllerConsiderHeight>() == null)
            {
                continue;
            }
            float globalY = child.position.y;
            if (globalY < minY) minY = globalY;
            if (globalY > maxY) maxY = globalY;
        }

        return (minY, maxY);
    }
}

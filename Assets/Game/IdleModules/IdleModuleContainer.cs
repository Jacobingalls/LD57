using System.Linq;
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
            cameraController.transform.position.y, 
            cameraController.transform.position.z
        );
    }

    public Bounds CalculateBoundsForLowestActiveModule()
    {
        var activeModules = GetComponentsInChildren<IdleModule>().Where(m => m.state == IdleModuleState.Purchased);

        if (activeModules.Count() == 0)
        {
            return new Bounds();
        }

        var lowestBound = activeModules.First().Bounds;
        foreach (var module in activeModules)
        {
            if (module.Bounds.min.y < lowestBound.min.y)
            {
                lowestBound = module.Bounds;
            }
        }
        return lowestBound;
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

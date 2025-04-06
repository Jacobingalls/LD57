using UnityEngine;
using UnityEngine.Events;

public class OnStart : MonoBehaviour
{

    public UnityEvent onStartEvent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onStartEvent.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

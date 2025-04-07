using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EndGameCinimatic : MonoBehaviour
{

    public UnityEvent onEndGameCinimaticEventDidFinish;

    public GameObject doSomethingHeSaid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        doSomethingHeSaid.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayEndGameCinimatic()
    {
        // Play the end game cinimatic here
        // This could be a video, animation, or any other type of cinimatic
        Debug.Log("Playing end game cinimatic...");
        StartCoroutine(PlayCinimaticWithDelay());

        IEnumerator PlayCinimaticWithDelay()
        {
            doSomethingHeSaid.SetActive(true);
            yield return new WaitForSeconds(5f);
            Debug.Log("End game cinimatic finished.");
            onEndGameCinimaticEventDidFinish?.Invoke();
        }
    }
}

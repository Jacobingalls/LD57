using UnityEngine;

public class AnimateHidden : MonoBehaviour
{

    public float progress = 0f;
    public float speed = 2f;

    public CanvasGroup canvasGroup;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        progress += Time.deltaTime * speed;
        progress = Mathf.Clamp01(progress);
        canvasGroup.alpha = 1f - progress;
    }
}

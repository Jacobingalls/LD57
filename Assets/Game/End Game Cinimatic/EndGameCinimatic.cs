using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EndGameCinimatic : MonoBehaviour
{

    public UnityEvent onEndGameCinimaticEventDidFinish;

    public SpriteRenderer handSpriteRenderer;

    public AnimationCurve handAnimationCurve;
    public AnimationCurve handAnimationCurveOpacity;

    public CanvasGroup atLongLast;
    public AnimationCurve atLongLastAnimationCurve;
    public CanvasGroup freeFromTheDepths;
    public AnimationCurve freeFromTheDepthsAnimationCurve;
    public CanvasGroup againElipsis;
    public AnimationCurve againElipsisAnimationCurve;
    public float progress = 0f;
    public float totalDuration = 5f;

    public bool isEndGame = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        handSpriteRenderer.gameObject.SetActive(false);
        atLongLast.gameObject.SetActive(false);
        freeFromTheDepths.gameObject.SetActive(false);
        againElipsis.gameObject.SetActive(false);
        UpdateCinimatic();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEndGame) { 
            progress = 0f;
            return; 
        }

        progress += Time.deltaTime / totalDuration;
        UpdateCinimatic();
    }

    public void UpdateCinimatic() {
        Vector3 position = handSpriteRenderer.transform.localPosition;
        position.y = handAnimationCurve.Evaluate(progress);
        handSpriteRenderer.transform.localPosition = position;

        Color color = handSpriteRenderer.color;
        color.a = handAnimationCurveOpacity.Evaluate(progress);
        handSpriteRenderer.color = color;

        atLongLast.alpha = atLongLastAnimationCurve.Evaluate(progress);
        freeFromTheDepths.alpha = freeFromTheDepthsAnimationCurve.Evaluate(progress);
        againElipsis.alpha = againElipsisAnimationCurve.Evaluate(progress);
    }

    public void PlayEndGameCinimatic()
    {
        // Play the end game cinimatic here
        // This could be a video, animation, or any other type of cinimatic
        Debug.Log("Playing end game cinimatic...");
        StartCoroutine(PlayCinimaticWithDelay());

        IEnumerator PlayCinimaticWithDelay()
        {

            // Give time for the camera and music to swell.
            yield return new WaitForSeconds(4.75f);

            progress = 0f;
            isEndGame = true;
            UpdateCinimatic();
            handSpriteRenderer.gameObject.SetActive(true);
            atLongLast.gameObject.SetActive(true);
            freeFromTheDepths.gameObject.SetActive(true);
            againElipsis.gameObject.SetActive(true);

            yield return new WaitForSeconds(5f);
            Debug.Log("End game cinimatic finished.");
            onEndGameCinimaticEventDidFinish?.Invoke();
        }
    }
}

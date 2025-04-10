using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject LeftClickTutorial;
    public GameObject LeftClickAndHoldTutorial;
    public GameObject ScrollTutorial;

    private bool _leftClickShown = false;
    private bool _leftClickAndHoldShown = false;
    private bool _scrollShown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LeftClickTutorial.SetActive(true);
        LeftClickAndHoldTutorial.SetActive(false);
        ScrollTutorial.SetActive(false);
        _leftClickShown = true;
    }

    public void ShowLeftClickAndHoldTutorial()
    {
        if (_leftClickAndHoldShown)
        {
            return;
        }
        LeftClickTutorial.SetActive(false);
        LeftClickAndHoldTutorial.SetActive(true);
        ScrollTutorial.SetActive(false);
        _leftClickAndHoldShown = true;
    }

    public void ShowScrollTutorial()
    {
        if (_scrollShown)
        {
            return;
        }
        LeftClickTutorial.SetActive(false);
        LeftClickAndHoldTutorial.SetActive(false);
        ScrollTutorial.SetActive(true);
        _scrollShown = true;
    }

    public void End()
    {
        LeftClickTutorial.SetActive(false);
        LeftClickAndHoldTutorial.SetActive(false);
        ScrollTutorial.SetActive(false);
        Destroy(gameObject);
    }
}

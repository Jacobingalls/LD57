using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject LeftClickTutorial;
    public GameObject LeftClickAndHoldTutorial;
    public GameObject ScrollTutorial;

    private bool _leftClickShown = false;
    private bool _leftClickAndHoldShown = false;
    private bool _scrollShown = false;
    private bool _sufficientlyScrolled = false;

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
        if (!_sufficientlyScrolled) {
            ScrollTutorial.SetActive(true);
        }
        _scrollShown = true;
    }

    // Can be called before the ui is first shown.
    public void SufficientlyScrolled()
    {
        ScrollTutorial.SetActive(false);
        _sufficientlyScrolled = true;
    }
}

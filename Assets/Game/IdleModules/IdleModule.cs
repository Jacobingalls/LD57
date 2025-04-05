using UnityEngine;
using UnityEngine.UIElements;

public class IdleModule : MonoBehaviour
{

    public IdleModuleState state = IdleModuleState.UnavailableForPurchase;
    private IdleModuleState lastState = IdleModuleState.UnavailableForPurchase;

    public GameObject templateView;
    public GameObject unableForPurchaseView;
    public GameObject availableForPurchaseView;
    public GameObject purchasedView;

    public string modulePurchaseTeaserName = "Go Deeper";
    public int purchaseCost = 100;

    public UnityEngine.Events.UnityEvent onPurchase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastState = state;
        SetState(state);

        // Make sure this doesn't show up in the game.
        if (templateView != null) {
            Destroy(templateView);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lastState != state)
        {
            lastState = state;
            SetState(state);
        }
    }

    public void SetState(IdleModuleState newState)
    {
        state = newState;

        unableForPurchaseView.SetActive(state == IdleModuleState.UnavailableForPurchase);
        availableForPurchaseView.SetActive(state == IdleModuleState.AvailableForPurchase);
        purchasedView.SetActive(state == IdleModuleState.Purchased);
    }

    public void MakeAvailableForPurchase()
    {
        if (state == IdleModuleState.UnavailableForPurchase)
        {
            SetState(IdleModuleState.AvailableForPurchase);
        }
    }

    public void Purchase()
    {
        if (CanBePurchased())
        {
            LD57GameManager.Instance.Mako -= purchaseCost;
            SetState(IdleModuleState.Purchased);
            onPurchase.Invoke();
        }
    }

    public bool CanBePurchased()
    {
        return state == IdleModuleState.AvailableForPurchase && LD57GameManager.Instance.Mako >= purchaseCost;
    }
}

[System.Serializable]
public enum IdleModuleState
{
    UnavailableForPurchase,

    AvailableForPurchase,

    Purchased,
}
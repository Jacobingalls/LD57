using UnityEngine;

public class IdleModule : MonoBehaviour
{

    public float height = 14f;

    public IdleModuleState state = IdleModuleState.UnavailableForPurchase;
    private IdleModuleState lastState = IdleModuleState.UnavailableForPurchase;

    public GameObject templateView;
    public GameObject unableForPurchaseView;
    public GameObject availableForPurchaseView;
    public GameObject purchasedView;

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
}

[System.Serializable]
public enum IdleModuleState
{
    UnavailableForPurchase,

    AvailableForPurchase,

    Purchased,
}
using info.jacobingalls.jamkit;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PubSubSender))]
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


    public bool doesUnlockResourceType = false;
    public ResourceType resourceTypeToUnlock = ResourceType.Mako;

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
            ResourceManager.Instance.Mako -= purchaseCost;
            SetState(IdleModuleState.Purchased);
            onPurchase.Invoke();

            if (doesUnlockResourceType)
            {
                ResourceManager.Instance.UnlockResource(resourceTypeToUnlock);
            }

            GetComponent<PubSubSender>().Publish("module.purchased");
        }
    }

    public bool CanBePurchased()
    {
        return state == IdleModuleState.AvailableForPurchase && ResourceManager.Instance.Mako >= purchaseCost;
    }

    private Bounds _bounds;
    private bool _boundsDirty = true;
    public Bounds Bounds
    {
        get {
            if (!_boundsDirty)
            {
                return _bounds;
            }
            _boundsDirty = false;
            var newBounds = new Bounds();
            foreach(var sr in GetComponentsInChildren<SpriteRenderer>())
            {
                newBounds.Encapsulate(sr.bounds);
                Debug.Log($"New Bounds is now {newBounds}");
            }
            _bounds = newBounds;

            return _bounds;
        }
    }
}

[System.Serializable]
public enum IdleModuleState
{
    UnavailableForPurchase,

    AvailableForPurchase,

    Purchased,
}
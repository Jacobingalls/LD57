using UnityEngine;

public class FooterModule : MonoBehaviour
{

    public GameObject unableForPurchaseView;
    public GameObject availableForPurchaseView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unableForPurchaseView.SetActive(true);
        availableForPurchaseView.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UnlockedLastModule()
    {
        unableForPurchaseView.SetActive(false);
        availableForPurchaseView.SetActive(true);
    }
}

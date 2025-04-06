using info.jacobingalls.jamkit;
using UnityEngine;

[RequireComponent(typeof(PubSubListener))]
public class ResourceDisplayManager : MonoBehaviour
{
    public GameObject ResourceDisplayPrefab;
    void Start()
    {
        Recalculate();
    }

    public void Recalculate()
    {
        for (var i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }

        var rectTransform = GetComponent<RectTransform>();
        foreach (var r in ResourceManager.Instance.GetUnlockedResources())
        {
            var go = Instantiate(ResourceDisplayPrefab);
            go.transform.GetComponent<RectTransform>().SetParent(rectTransform, false);
            go.GetComponent<ResourceDisplay>().resourceType = r.type;
        }
    }
}

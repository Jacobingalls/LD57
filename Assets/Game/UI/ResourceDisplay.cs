using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    public TextMeshProUGUI makoCounterLabel;

    private LD57GameManager _gm;

    public void Start()
    {
        _gm = FindFirstObjectByType<LD57GameManager>();
    }

    void Update()
    {
        makoCounterLabel.text = $"{_gm.Mako}";
    }
}

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class WinScreenInfo : MonoBehaviour
{

    public TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        List<String> parts = new List<String>();

        if (ResourceManager.Instance == null) {
            text.text = "No ResourceManager found";
            return;
        }

        int mako = ResourceManager.Instance.GetResourceLifetimeAmount(ResourceType.Mako);
        if (mako > 0) { parts.Add("<sprite name=\"mako\"> " + mako.ToString()); }

        int people = ResourceManager.Instance.GetResourceLifetimeAmount(ResourceType.People);
        if (people > 0) { parts.Add("<sprite name=\"people\"> " + people.ToString()); }

        int science = ResourceManager.Instance.GetResourceLifetimeAmount(ResourceType.Science);
        if (science > 0) { parts.Add("<sprite name=\"science\"> " + science.ToString()); }

        int artifacts = ResourceManager.Instance.GetResourceLifetimeAmount(ResourceType.Artifacts);
        if (artifacts > 0) { parts.Add("<sprite name=\"artifacts\"> " + artifacts.ToString()); }

        string textString = String.Join("\n", parts);
        text.text = textString;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

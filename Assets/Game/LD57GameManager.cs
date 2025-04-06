using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LD57GameManager : MonoBehaviour
{
    public static LD57GameManager Instance;

    public Toolip tooltipPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

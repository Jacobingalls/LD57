using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LD57GameManager : MonoBehaviour
{
    public static LD57GameManager Instance;

    private int mako = 0;

    public int Mako
    {
        get { return mako; }
        set { mako = value; }
    }

    public void IncrementMako(int amount)
    {
        Mako += amount;
    }
    public void DecrementMako(int amount)
    {
        Mako -= amount;
    }

    // Start is called before the first frame update
    void Start()
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

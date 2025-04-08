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

    public void PlayBGM() {
        AudioManager.Instance.Play2D(
            "LD57 BGM", 
            loop: true,
            volumeMin: 0.5f, 
            volumeMax: 0.5f, 
            position: transform.position,
            isMusic: true
        );
    }

}

using UnityEngine;
using System;
using System.Collections.Generic;

public class NumberTumbler : MonoBehaviour
{

    public List<NumberSlider> sliders;

    [SerializeField]
    private int numberOfSliders = 4;

    public GameObject numbersContainer;
    public NumberSlider sliderPrefab;

    [Range(0, 9999)]
    public int currentNumber = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in numbersContainer.transform)
        {
            Destroy(child.gameObject);
        }

        sliders = new List<NumberSlider>();
        for (int i = 0; i < numberOfSliders; i++)
        {
            NumberSlider newSlider = Instantiate(sliderPrefab, numbersContainer.transform);
            newSlider.weight = 10.0f - i;
            sliders.Add(newSlider);
        }

        SetNumber(currentNumber);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetNumber(int number)
    {
        int currentNumber = number;
        for (int i = 0; i < numberOfSliders; i++)
        {
            sliders[i].SetNumberToShow(currentNumber % 10, number > this.currentNumber);
            currentNumber /= 10;
        }
        this.currentNumber = number;
    }

    public void IncrementNumber(int increment)
    {
        SetNumber(currentNumber + increment);
    }
    
    public void DecrementNumber(int decrement)
    {
        SetNumber(currentNumber - decrement);
    }
}

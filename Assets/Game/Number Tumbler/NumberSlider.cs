using UnityEngine;
using TMPro;
using System;

public class NumberSlider : MonoBehaviour
{

    public TextMeshProUGUI numbers;
    public float verticalOffset = 0;
    public float verticalIncrement = 50;

    [Range(0, 9)]
    public int numberToShow;

    public bool goingUp;

    public float weight = 8.0f;

    public void SetNumberToShow(int number)
    {
        int sanitizedNumber = Mathf.Abs(number) % 10;
        int diff = sanitizedNumber - numberToShow;
        if (diff == 0) { return; }

        // Determine the shortest path (considering wrap-around)
        if (Mathf.Abs(diff) > 5)
        {
            goingUp = diff < 0;
        }
        else
        {
            goingUp = diff > 0;
        }

        numberToShow = sanitizedNumber;
    }

    public void SetNumberToShow(int number, bool goUp)
    {
        
        
        int newNumberToShow = Mathf.Abs(number % 10);
        if (newNumberToShow == numberToShow) { return; }
        numberToShow = newNumberToShow;
        if (number < 0) {
            goingUp = !goUp;
        } else {
            goingUp = goUp;
        }
    }

    // 0 1 2 3 4 5 6 7 8 9
    //   |             |

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial position of the numbers
        Vector3 currentPosition = numbers.transform.localPosition;
        float targetY = (Mathf.Clamp(numberToShow, 0, 9) * verticalIncrement) + verticalOffset;
        currentPosition.y = targetY;
        numbers.transform.localPosition = currentPosition;

        // Set the initial number to show
        numberToShow = Mathf.Clamp(numberToShow, 0, 9);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = numbers.transform.localPosition;
        float endYForLastLetter = getMaxOffset() + verticalOffset;

        // Wrap-around logic for current position
        if (currentPosition.y > endYForLastLetter)
        {
            currentPosition.y -= getMaxOffset();
        }
        else if (currentPosition.y < verticalOffset)
        {
            currentPosition.y += getMaxOffset();
        }

        float targetY = (numberToShow * verticalIncrement) + verticalOffset;
        if (!goingUp) {
            if (targetY > currentPosition.y) {
                targetY -= getMaxOffset();
            }
        } else {
            if (targetY < currentPosition.y) {
                targetY += getMaxOffset();
            }
        }

        Vector3 targetPosition = new Vector3(
            currentPosition.x,
            targetY,
            currentPosition.z
        );

        // Smoothly move towards the target position
        numbers.transform.localPosition = expDecay(currentPosition, targetPosition, weight, Time.deltaTime);
    }

    float getCurrentNumber()
    {
        return (numbers.transform.localPosition.y - verticalOffset) / verticalIncrement;
    }

    float getMaxOffset()
    {
        return 10 * verticalIncrement;
    }    

    // https://acegikmo.substack.com/p/lerp-smoothing-is-broken
    float expDecay(float a, float b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }

    Vector3 expDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return new Vector3(
            expDecay(a.x, b.x, decay, deltaTime),
            expDecay(a.y, b.y, decay, deltaTime),
            expDecay(a.z, b.z, decay, deltaTime)
        );
    }
}

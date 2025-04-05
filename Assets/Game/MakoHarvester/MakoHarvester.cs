using System.Collections;
using UnityEngine;

public class MakoHarvester : MonoBehaviour
{
    public SpriteFloat spriteFloat;
    public Animator laserAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked!");

        const float jitterDuration = 0.25f;
        ShowLaser(hideAfter: jitterDuration);
        spriteFloat.Jitter(jitterDuration: jitterDuration);
    }

    void ShowLaser(float hideAfter)
    {
        AudioManager.Instance.Play("Mako/Charge", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
        laserAnimator.gameObject.SetActive(true);
        laserAnimator.SetTrigger("ChargeUp");
        StartCoroutine(HideLaser(hideAfter));
    }

    IEnumerator HideLaser(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        laserAnimator.gameObject.SetActive(false);
    }
}

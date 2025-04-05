using System.Collections;
using UnityEngine;

public class MakoHarvester : MonoBehaviour
{
    [Header("Mako Orbs")]
    public GameObject makoOrbPrefab;
    public Transform makoOrbSpawnPoint;

    [Header("Visuals")]
    public SpriteFloat harvesterSpriteFloat;
    public Animator laserAnimator;
    public Animator harvesterAnimator;

    private int _maxChargeState = 9;
    private int _chargeState = 0;
    private bool _discharging = false;

    private GameObject _makoFocusGO;
    private SpriteFloat[] _makoFocusSpriteFloats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _makoFocusGO = GameObject.Find("MakoFocus");
        _makoFocusSpriteFloats = _makoFocusGO.GetComponentsInChildren<SpriteFloat>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (_discharging)
        {
            return;
        }

        _chargeState += 1;
        if (_chargeState == _maxChargeState)
        {
            harvesterAnimator.ResetTrigger("ChargeUp");
            harvesterAnimator.speed = 1.0f;
            harvesterAnimator.Play("MakoHarvesterChargedAnimation", 0, 0);
            _discharging = true;
            const float jitterDuration = 0.25f;
            ShowLaser(hideAfter: jitterDuration);
        }
        else
        {
            harvesterAnimator.SetTrigger("ChargeUp");
            harvesterAnimator.speed = 0.0f;
            harvesterAnimator.Play("MakoHarvesterChargeAnimation", 0, (float)_chargeState / _maxChargeState);

            AudioManager.Instance.Play("Mako/Focus", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
            const float jitterDuration = 0.15f;
            harvesterSpriteFloat.Jitter(jitterDuration: jitterDuration);
        }
    }

    void ShowLaser(float hideAfter)
    {
        harvesterSpriteFloat.Jitter(jitterDuration: hideAfter);

        foreach (var spriteFloat in _makoFocusSpriteFloats)
        {
            spriteFloat.Jitter(jitterDuration: Random.Range(hideAfter * 0.8f, hideAfter * 1.2f));
        }

        AudioManager.Instance.Play("Mako/Charge", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
        laserAnimator.gameObject.SetActive(true);
        laserAnimator.SetTrigger("ChargeUp");
        StartCoroutine(HideLaser(hideAfter));
    }

    IEnumerator HideLaser(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        var makoOrbGO = Instantiate(makoOrbPrefab);
        makoOrbGO.transform.position = makoOrbSpawnPoint.position;
        var makoOrb = makoOrbGO.GetComponent<MakoOrb>();
        const float impulseMin = 2.0f;
        const float impulseMax = 5.5f;
        makoOrb.ApplySpawningImpulse(Random.Range(impulseMin, impulseMax));

        laserAnimator.gameObject.SetActive(false);

        _discharging = false;
        _chargeState = 0;
        harvesterAnimator.ResetTrigger("Charged");

        harvesterAnimator.SetTrigger("ChargeUp");
        harvesterAnimator.speed = 0.0f;
        harvesterAnimator.Play("MakoHarvesterChargeAnimation", 0, 0.0f);
    }
}

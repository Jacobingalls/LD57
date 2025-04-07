using info.jacobingalls.jamkit;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class MakoHarvester : MonoBehaviour
{
    [Header("Mako Orbs")]
    public Transform makoOrbSpawnPoint;

    [Header("Visuals")]
    public SpriteFloat harvesterSpriteFloat;
    public Animator laserAnimator;
    public Animator harvesterAnimator;

    public List<MakoOrb> makoOrbs = new();

    private int _maxChargeState = 9;
    private int _chargeState = 0;
    private bool _discharging = false;

    public bool Discharging
    {
        get { return _discharging; }
    }
    private GameObject _makoFocusGO;
    private SpriteFloat[] _makoFocusSpriteFloats;

    private bool _pressing = false;
    private float _pressTime = 0.0f;
    private float _triggerTime = 0.0f;

    [UnityEngine.Range(0.0f, 1.0f)]
    public float shortPressThreshold = 0.200f;
    public bool pressAndHoldEnabled = false;
    [UnityEngine.Range(0.0f, 2.0f)]
    public float pressAndHoldTriggerTime = 0.500f;

    private int manualClickGain = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _makoFocusGO = GameObject.Find("MakoFocus");
        _makoFocusSpriteFloats = _makoFocusGO.GetComponentsInChildren<SpriteFloat>();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoClickAndHold, (u =>
        {
            pressAndHoldEnabled = true;
        }));

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoManualSummonIncrease, (u =>
        {
            manualClickGain *= 2;
        }));

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoRefinementUnlockCrystal, (u =>
        {
            laserAnimator.transform.localScale = new Vector3(1.0f, 150.0f, 1.0f);
        }));
    }

    // Update is called once per frame
    void Update()
    {
        if (_pressing)
        {
            _pressTime += Time.deltaTime;
            _triggerTime -= Time.deltaTime;
            
            if (_triggerTime < 0.0f && pressAndHoldEnabled)
            {
                _triggerTime = pressAndHoldTriggerTime;
                ChargeHarvester(manualClickGain);
            }
        }
    }

    public void RepositionFocus()
    {
        var moduleContainer = (IdleModuleContainer)FindFirstObjectByType(typeof(IdleModuleContainer));
        var lowestBounds = moduleContainer.CalculateBoundsForLowestActiveModule();

        _makoFocusGO.transform.position = new Vector3(_makoFocusGO.transform.position.x, lowestBounds.min.y, _makoFocusGO.transform.position.z);
    }

    private void OnMouseDown()
    {
        if (_discharging)
        {
            return;
        }

        _pressing = true;
        _pressTime = 0.0f;
        _triggerTime = pressAndHoldTriggerTime;
    }

    private void OnMouseUp()
    {
        if (_pressTime <= shortPressThreshold || !pressAndHoldEnabled)
        {
            ChargeHarvester(manualClickGain);
        }

        _pressing = false;
    }

    public void AutoChargeHarvester()
    {
        ChargeHarvester();
    }

    private void ChargeHarvester(int increaseAmount = 1)
    {
        _chargeState = Mathf.Min(_chargeState + increaseAmount, _maxChargeState);
        if (_chargeState == _maxChargeState)
        {
            if (_discharging) { return; }
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

            AudioManager.Instance.Play2D("Mako/Focus", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
            const float jitterDuration = 0.15f;
            harvesterSpriteFloat.Jitter(jitterDuration: jitterDuration);
        }
    }

    void ShowLaser(float hideAfter)
    {
        harvesterSpriteFloat.Jitter(jitterDuration: hideAfter);
        GetComponent<PubSubSender>().Publish("mako.harvester.laser.begin");

        foreach (var spriteFloat in _makoFocusSpriteFloats)
        {
            spriteFloat.Jitter(jitterDuration: Random.Range(hideAfter * 0.8f, hideAfter * 1.2f));
        }

        AudioManager.Instance.Play2D("Mako/Charge", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
        laserAnimator.gameObject.SetActive(true);
        laserAnimator.SetTrigger("ChargeUp");
        StartCoroutine(HideLaser(hideAfter));
    }

    IEnumerator HideLaser(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        GetComponentInParent<MakoManager>().SpawnMakoOrbs(makoOrbSpawnPoint.position);

        laserAnimator.gameObject.SetActive(false);

        _discharging = false;
        _chargeState = 0;
        harvesterAnimator.ResetTrigger("Charged");

        harvesterAnimator.SetTrigger("ChargeUp");
        harvesterAnimator.speed = 0.0f;
        harvesterAnimator.Play("MakoHarvesterChargeAnimation", 0, 0.0f);
        GetComponent<PubSubSender>().Publish("mako.harvester.laser.end");
    }
}

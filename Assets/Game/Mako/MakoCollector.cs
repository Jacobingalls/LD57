using UnityEngine;
using System.Linq;
using System.Collections;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class MakoCollector : MonoBehaviour
{
    [Header("Succ Physics")]
    public float toVel = 2.5f;
    public float maxVel = 4.0f;
    public float maxForce = 10.0f;
    public float gain = 3f;

    private float maxVelMultiplier = 1.0f;
    private float maxForceAdditive = 0.0f;

    [Header ("Manual Controls")]
    public bool AllowsManual = true;

    [Header("Auto Controls")]
    public bool AllowsAuto = false;
    [Range(0.0f, 10.0f)]
    public float AutoCooldown = 2.0f;

    public GameObject onCooldownVisuals;
    public GameObject offCooldownVisuals;

    public MakoHarvesterBeam makoCollectorBeam;

    public SpriteFloat spriteFloat;
    public Animator laserAnimator;

    public Transform drawTarget;

    private MakoManager _mm;
    private bool _isSucking;
    private bool _isLasering;
    private AudioSource _pullInAudioSource;
    private PubSubSender _pubsub;
    private float _currentCooldown = 0.0f;
    private JumpingMortal _jumpingMortal;
    private float _timeHoldingOrb;


    void Start()
    {
        _pubsub = GetComponent<PubSubSender>();
        _mm = GetComponentInParent<MakoManager>();
        _jumpingMortal = GetComponentInChildren<JumpingMortal>();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoManualSummonIncrease, u =>
        {
            SetMaxVelMultiplier();
        });

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.ResearchProcessingMoreMakoSuck, u =>
        {
            SetMaxVelMultiplier();
        });

        SetMaxVelMultiplier();
    }

    private void SetMaxVelMultiplier()
    {
        var makuManualSummonIncrease = UpgradeManager.Instance.GetUpgrade(UpgradeType.MakoManualSummonIncrease).timesPurchased;
        var researchProcessingMoreMakoSuck = UpgradeManager.Instance.GetUpgrade(UpgradeType.ResearchProcessingMoreMakoSuck).timesPurchased;
        var totalUpgrades = makuManualSummonIncrease + researchProcessingMoreMakoSuck;
        maxVelMultiplier = Mathf.Pow(2, totalUpgrades);
        maxForceAdditive = totalUpgrades;
    }

    void FixedUpdate()
    {
        if (AllowsAuto)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown -= Time.fixedDeltaTime;
                _isSucking = false;
            }
            else
            {
                _isSucking = true;
            }
        }

        if (onCooldownVisuals != null && offCooldownVisuals != null)
        {
            if (_currentCooldown > 0)
            {
                onCooldownVisuals.SetActive(true);
                offCooldownVisuals.SetActive(false);
            }
            else
            {
                onCooldownVisuals.SetActive(false);
                offCooldownVisuals.SetActive(true);
            }
        }

        if (_isSucking)
        {
            SelectTarget();
            PullInTarget();
        }

        if (_jumpingMortal != null)
        {
            _jumpingMortal.jumping = _makoOrbTarget != null;
        }
    }

    private MakoOrb _makoOrbTarget = null;

    public void BeginSuck()
    {
        _isSucking = true;
    }

    public void StopSuck()
    {
        _isSucking = false;

        if (!_isLasering)
        {
            CleanUp();
        }
    }

    private void OnMouseDown()
    {
        if (AllowsManual)
        {
            _pubsub.Publish("mako.collector.manual.begin");
        }
    }

    private void OnMouseUp()
    {
        if (AllowsManual)
        {
            _pubsub.Publish("mako.collector.manual.end");
        }
    }

    private void CleanUp()
    {
        if (_makoOrbTarget != null)
        {
            _makoOrbTarget.GetComponent<CircleCollider2D>().enabled = true;
            _makoOrbTarget.BeingSucked = false;
        }
        _makoOrbTarget = null;
        makoCollectorBeam.Target = null;
        if (_pullInAudioSource != null)
        {
            _pullInAudioSource.Stop();
            _pullInAudioSource = null;
        }
    }

    private void SelectTarget()
    {
        if (_makoOrbTarget != null)
        {
            return;
        }

        var collectableMakoOrbs = _mm.makoOrbs.Where(x => x.Collectable).ToList();

        if (collectableMakoOrbs.Count == 0)
        {
            return;
        }

        var candidateClosest = collectableMakoOrbs.First();
        var candidateDistance = (drawTarget.position - candidateClosest.transform.position).sqrMagnitude;
        foreach (var makoOrb in collectableMakoOrbs)
        {
            var distance = (drawTarget.position - makoOrb.transform.position).sqrMagnitude;
            if (distance < candidateDistance)
            {
                candidateDistance = distance;
                candidateClosest = makoOrb;
            }
        }

        _makoOrbTarget = candidateClosest;
        _makoOrbTarget.BeingSucked = true;
        makoCollectorBeam.Target = _makoOrbTarget.transform.Find("Visuals").transform;

        _pullInAudioSource = AudioManager.Instance.Play2D("Mako/PullIn", loop: true, pitchMin: 0.9f, pitchMax: 1.1f, volumeMin: 0.3f, volumeMax: 0.6f, position: transform.position);
    }

    private void PullInTarget()
    {
        if (_makoOrbTarget == null)
        {
            return;
        }

        if (_makoOrbTarget.Captured)
        {
            return;
        }

        _timeHoldingOrb += Time.fixedDeltaTime;
        if (_timeHoldingOrb > 5.0f)
        {
            _makoOrbTarget.GetComponent<CircleCollider2D>().enabled = false;
        }

        _makoOrbTarget.DriftTowardsTarget(drawTarget, toVel, maxVel * maxVelMultiplier, maxForce + maxForceAdditive, gain);

        float distanceToTarget = (drawTarget.position - _makoOrbTarget.transform.position).magnitude;
        if (distanceToTarget < 0.025f && !_isLasering)
        {
            // Lock that bad boy in
            _makoOrbTarget.Captured = true;
            _makoOrbTarget.StopDrifting();
            AudioManager.Instance.Play2D("Mako/Capture", pitchMin: 0.9f, pitchMax: 1.1f, volumeMin: 0.3f, volumeMax: 0.6f, position: transform.position);
            _makoOrbTarget.transform.position = drawTarget.transform.position;

            const float jitterDuration = 0.25f;
            ShowLaser(hideAfter: jitterDuration);
        }
    }

    void ShowLaser(float hideAfter)
    {
        _isLasering = true;
        spriteFloat.Jitter(jitterDuration: hideAfter);

        AudioManager.Instance.Play2D("Mako/Charge", pitchMin: 0.9f, pitchMax: 1.1f, volumeMin: 0.3f, volumeMax: 0.6f, position: transform.position);
        laserAnimator.gameObject.SetActive(true);
        laserAnimator.SetTrigger("ChargeUp");
        StartCoroutine(HideLaser(hideAfter));
    }

    IEnumerator HideLaser(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        _mm.ConsumeMakoOrb(_makoOrbTarget);
        _makoOrbTarget = null;
        makoCollectorBeam.Target = null;
        AudioManager.Instance.Play2D("Mako/Harvest", pitchMin: 0.9f, pitchMax: 1.1f, volumeMin: 0.3f, volumeMax: 0.6f, position: transform.position);

        _pubsub.Publish("mako.collected");

        laserAnimator.gameObject.SetActive(false);
        CleanUp();
        _isLasering = false;
        _currentCooldown = AutoCooldown;
    }
}

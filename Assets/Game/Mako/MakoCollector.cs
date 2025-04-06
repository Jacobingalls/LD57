using UnityEngine;
using System.Linq;
using System.Collections;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class MakoCollector : MonoBehaviour
{
    public float toVel = 2.5f;
    public float maxVel = 4.0f;
    public float maxForce = 10.0f;
    public float gain = 3f;

    public MakoHarvesterBeam makoCollectorBeam;

    public SpriteFloat spriteFloat;
    public Animator laserAnimator;

    public Transform drawTarget;

    private MakoManager _mm;
    private bool _isSucking;
    private bool _isLasering;
    private AudioSource _pullInAudioSource;
    private PubSubSender _pubsub;


    void Start()
    {
        _pubsub = GetComponent<PubSubSender>();
        _mm = GetComponentInParent<MakoManager>();

        UpgradeManager.Instance.RegisterUpgradePurchaseHandler(UpgradeType.MakoManualSummonIncrease, u =>
        {
            maxVel *= 2f;
        });
    }

    void FixedUpdate()
    {
        if (_isSucking)
        {
            SelectTarget();
            PullInTarget();
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
        _pubsub.Publish("mako.collector.begin");
    }

    private void OnMouseUp()
    {
        _pubsub.Publish("mako.collector.end");
    }

    private void CleanUp()
    {
        if (_makoOrbTarget != null)
        {
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

        _pullInAudioSource = AudioManager.Instance.Play2D("Mako/PullIn", loop: true, pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
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

        _makoOrbTarget.DriftTowardsTarget(drawTarget, toVel, maxVel, maxForce, gain);

        float distanceToTarget = (drawTarget.position - _makoOrbTarget.transform.position).magnitude;
        if (distanceToTarget < 0.025f && !_isLasering)
        {
            // Lock that bad boy in
            _makoOrbTarget.Captured = true;
            _makoOrbTarget.StopDrifting();
            AudioManager.Instance.Play2D("Mako/Capture", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
            _makoOrbTarget.transform.position = drawTarget.transform.position;

            const float jitterDuration = 0.25f;
            ShowLaser(hideAfter: jitterDuration);
        }
    }

    void ShowLaser(float hideAfter)
    {
        _isLasering = true;
        spriteFloat.Jitter(jitterDuration: hideAfter);

        AudioManager.Instance.Play2D("Mako/Charge", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
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
        AudioManager.Instance.Play2D("Mako/Harvest", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);

        _pubsub.Publish("mako.collected");

        laserAnimator.gameObject.SetActive(false);
        CleanUp();
        _isLasering = false;
    }
}

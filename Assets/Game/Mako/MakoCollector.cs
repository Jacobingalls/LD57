using UnityEngine;
using System.Linq;
using System.Collections;

public class MakoCollector : MonoBehaviour
{
    public float toVel = 2.5f;
    public float maxVel = 4.0f;
    public float maxForce = 10.0f;
    public float gain = 3f;

    public SpriteFloat spriteFloat;
    public Animator laserAnimator;

    public Transform drawTarget;

    private MakoManager _mm;
    private bool _isSucking;

    void Start()
    {
        _mm = GetComponentInParent<MakoManager>();
    }

    void FixedUpdate()
    {
        if (_isSucking)
        {
            SelectTarget();
            PullInTarget();
            CollectTarget();
        }
    }

    private MakoOrb _makoOrbTarget = null;

    private void OnMouseDown()
    {
        _isSucking = true;
    }

    private void OnMouseUp()
    {
        _isSucking = false;
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
        if (distanceToTarget < 0.025f)
        {
            // Lock that bad boy in
            _makoOrbTarget.Captured = true;
            _makoOrbTarget.StopDrifting();
            AudioManager.Instance.Play("Mako/Capture", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
            _makoOrbTarget.transform.position = drawTarget.transform.position;

            const float jitterDuration = 0.25f;
            ShowLaser(hideAfter: jitterDuration);
        }
    }

    private void CollectTarget()
    {

        if (_makoOrbTarget == null)
        {
            return;
        }

        if (!_makoOrbTarget.Captured)
        {
            return;
        }

        Debug.Log("Capture that mofo!!!");
    }

    void ShowLaser(float hideAfter)
    {
        spriteFloat.Jitter(jitterDuration: hideAfter);

        AudioManager.Instance.Play("Mako/Charge", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);
        laserAnimator.gameObject.SetActive(true);
        laserAnimator.SetTrigger("ChargeUp");
        StartCoroutine(HideLaser(hideAfter));
    }

    IEnumerator HideLaser(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        _mm.ConsumeMakoOrb(_makoOrbTarget);
        _makoOrbTarget = null;
        AudioManager.Instance.Play("Mako/Harvest", pitchMin: 0.9f, pitchMax: 1.1f, position: transform.position);

        laserAnimator.gameObject.SetActive(false);
    }
}

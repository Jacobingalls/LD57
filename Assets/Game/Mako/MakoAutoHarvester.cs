using System.Collections;
using UnityEngine;

public class MakoAutoHarvester : MonoBehaviour
{
    [Range(0.0f, 5.0f)]
    public float cooldown = 1.0f;
    private float _currentCooldown = 0.0f;

    private MakoHarvester _makoHarvester;
    public SpriteFloat spriteFloat;
    public Transform laserTarget;

    void Start()
    {
        _makoHarvester = GetComponentInParent<MakoHarvester>();
        _currentCooldown = cooldown;
        GetComponentInChildren<JumpingMortal>().jumping = true;
    }

    void Update()
    {
        _currentCooldown -= Time.deltaTime;

        if (_currentCooldown < 0 && !_makoHarvester.Discharging)
        {
            StartCoroutine(AutoHarvestRoutine());
            _currentCooldown = cooldown;
        }
    }

    private IEnumerator AutoHarvestRoutine()
    {
        var makoHarvesterBeam = GetComponentInChildren<MakoHarvesterBeam>();

        var target = laserTarget != null ? laserTarget : _makoHarvester.transform;
        makoHarvesterBeam.Target = target;
        _makoHarvester.AutoChargeHarvester();
        spriteFloat.Jitter(0.25f);
        yield return new WaitForSeconds(0.25f);
        makoHarvesterBeam.Target = null;
    }
}

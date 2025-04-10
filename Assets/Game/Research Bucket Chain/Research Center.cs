using UnityEngine;

public class ResearchCenter : MonoBehaviour
{

    public float gooLevel = 0f;
    public float gooToMakeKnowledge = 10f;

    public float maxKnowledgeHeight = 0f;

    public BucketChain bucketChain;
    public KnowledgeParticle knowledgeParticlePrefab;

    public KnoledgeTop knoledgeTop;

    public SpriteRenderer researchCenterBuilding;
    public SpriteRenderer researchCenterBuilderLit;
    public AnimationCurve researchCenterBuildingLightCurve;
    public float researchCenterBuildingLightTime = 1f;

    public KnowledgeGuage knowledgeGuage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        researchCenterBuilding.gameObject.SetActive(true);
        researchCenterBuilderLit.gameObject.SetActive(true);
        researchCenterBuildingLightTime = 1f;
        researchCenterBuilding.color = new Color(1, 1, 1, 1);
        researchCenterBuilderLit.color = new Color(1, 1, 1, 0);
       
    }

    // Update is called once per frame
    void Update()
    {
        researchCenterBuildingLightTime += Time.deltaTime;
        researchCenterBuildingLightTime = Mathf.Clamp01(researchCenterBuildingLightTime);
        researchCenterBuilding.color = new Color(1, 1, 1, researchCenterBuildingLightCurve.Evaluate(researchCenterBuildingLightTime));
        researchCenterBuilderLit.color = new Color(1, 1, 1, 1 - researchCenterBuildingLightCurve.Evaluate(researchCenterBuildingLightTime));
        knowledgeGuage.percentage = Mathf.Clamp01(gooLevel / gooToMakeKnowledge);
    }

    public void DidGetGoo()
    {
        gooLevel += 1f;
        if (gooLevel >= gooToMakeKnowledge)
        {
            gooLevel -= gooToMakeKnowledge;
            MakeKnowledge();
        }

        AudioManager.Instance.Play2D(
            "Factory/GotBucket", 
            loop: false,
            pitchMin: 0.9f,
            pitchMax: 1.1f,
            volumeMin: 0.05f, 
            volumeMax: 0.15f, 
            position: transform.position
        );
    }

    void MakeKnowledge()
    {
        KnowledgeParticle knowledgeParticle = Instantiate(knowledgeParticlePrefab, transform);
        knowledgeParticle.transform.position += Vector3.up * 1f;
        knowledgeParticle.speed = 1f;
        knowledgeParticle.maxY = maxKnowledgeHeight;
        knowledgeParticle.researchCenter = this;
        researchCenterBuildingLightTime = 0f;

        AudioManager.Instance.Play2D(
            "Factory/MakeKnowledge", 
            loop: false,
            pitchMin: 0.9f,
            pitchMax: 1.1f,
            volumeMin: 0.3f, 
            volumeMax: 0.5f, 
            position: transform.position
        );
    }

    public void KnoledgeParticleDidReachTop(KnowledgeParticle knowledgeParticle)
    {
        knoledgeTop.DidGetKnowledge();
        ResourceManager.Instance.IncrementResource(ResourceType.Science, 1);
    }

    public void MakeChainFaster() {
        bucketChain.chainSpeed *= 2;
    }

    public void IncreaseEfficiency() {
        gooToMakeKnowledge /= 2;
    }

    public void AddBucket() {
        bucketChain.bucketFreeCount += 1;
    }

    public void PlayAudio() {
        AudioManager.Instance.Play2D(
            "Ambiance/Factory", 
            loop: true,
            volumeMin: 0.05f, 
            volumeMax: 0.05f, 
            position: transform.position
        );
    }
}

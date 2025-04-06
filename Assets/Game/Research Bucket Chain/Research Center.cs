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
    }

    void MakeKnowledge()
    {
        KnowledgeParticle knowledgeParticle = Instantiate(knowledgeParticlePrefab, transform);
        knowledgeParticle.transform.position += Vector3.up * 1f;
        knowledgeParticle.speed = 1f;
        knowledgeParticle.maxY = maxKnowledgeHeight;
        knowledgeParticle.researchCenter = this;
        researchCenterBuildingLightTime = Mathf.Clamp01(researchCenterBuildingLightTime - 0.5f);
    }

    public void KnoledgeParticleDidReachTop(KnowledgeParticle knowledgeParticle)
    {
        knoledgeTop.DidGetKnowledge();
        ResourceManager.Instance.Science += 1;
    }
}

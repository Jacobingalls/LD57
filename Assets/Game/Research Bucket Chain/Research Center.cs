using UnityEngine;

public class ResearchCenter : MonoBehaviour
{

    public float gooLevel = 0f;
    public float gooToMakeKnowledge = 10f;

    public float maxKnowledgeHeight = 0f;

    public BucketChain bucketChain;
    public KnowledgeParticle knowledgeParticlePrefab;

    public KnoledgeTop knoledgeTop;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }

    public void KnoledgeParticleDidReachTop(KnowledgeParticle knowledgeParticle)
    {
        knoledgeTop.DidGetKnowledge();
    }
}

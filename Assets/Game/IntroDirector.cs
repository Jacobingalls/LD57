using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class IntroDirector : MonoBehaviour
{
    public List<GameObject> _sequences;
    public List<GameObject> _sequenceVisuals;

    int currentSequence = 0;
    int currentSequenceSubIndex = 0;

    void Start()
    {
        ActivateSequence();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<PubSubSender>().Publish("gameManager.showGame");
        }
    }

    public void ActivateSequence()
    {
        for (var i = 0; i < _sequences.Count; i++)
        {
            var sequence = _sequences[i];
            sequence.SetActive(i == currentSequence);
        }
        for (var i = 0; i < _sequenceVisuals.Count; i++)
        {
            var sequenceVisuals = _sequenceVisuals[i];
            sequenceVisuals.SetActive(i == currentSequence);
        }

        var curSeq = _sequences[currentSequence];
        var curSeqVisuals = _sequenceVisuals[currentSequence];

        StartCoroutine(NextSequenceElement(3.0f));
    }

    public IEnumerator NextSequenceElement(float waitFor)
    {
        AudioManager.Instance.Play2D("Upgrade/Purchase", pitchMin: 0.8f, pitchMax: 0.8f, volumeMin: 0.5f, volumeMax: 0.5f, position: transform.position);
        var curSeq = _sequences[currentSequence];
        for (var i = 0; i < curSeq.transform.childCount; i++)
        {
            var go = curSeq.transform.GetChild(i).gameObject;
            go.SetActive(i <= currentSequenceSubIndex);
        }

        yield return new WaitForSeconds(waitFor);
        currentSequenceSubIndex += 1;

        if (currentSequenceSubIndex == curSeq.transform.childCount)
        {
            StartCoroutine(NextSequence());
        }
        else
        {
            StartCoroutine(NextSequenceElement(3.0f));
        }
    }

    public IEnumerator NextSequence()
    {
        currentSequence += 1;
        currentSequenceSubIndex = 0;

        if (currentSequence >= _sequences.Count)
        {
            yield break;
        }

        for (var i = 0; i < _sequences.Count; i++)
        {
            var sequence = _sequences[i];
            sequence.SetActive(false);
        }
        for (var i = 0; i < _sequenceVisuals.Count; i++)
        {
            var sequenceVisuals = _sequenceVisuals[i];
            sequenceVisuals.SetActive(false);

        }

        if (currentSequence == 1) {
            AudioManager.Instance.Play2D(
                "Tutorial/Calamity",
                loop: false, 
                volumeMin: 1f, 
                volumeMax: 1f, 
                position: transform.position
            );
        }

        yield return new WaitForSeconds(10.0f);

        ActivateSequence();
    }
}

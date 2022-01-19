using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityDecoupledBehavior;
public class IdiomProunounciationGuide : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> speechList = new List<AudioClip>();
    [SerializeField]
    private SoundManager sManager;

    public void playSpeech(int index)
    {
        if (index >= speechList.Count) return;
        //cuz the original index start from 1 to limit
        sManager.PlayOneShot(speechList[index-1]);
    }
    // Start is called before the first frame update

}

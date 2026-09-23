using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSampleManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> effectSamples = new List<GameObject>();
    public GameObject effectPool;

    public void PlayEffectSample(int index)
    {
        if(index < 0 || index >= effectSamples.Count)
        {
            Debug.Log("EffectSampleManager: Invalid effect sample index.");
            return;
        }

       GameObject effect = Instantiate(effectSamples[index], effectPool.transform.position, Quaternion.identity);
       effect.transform.SetParent(effectPool.transform);
       effect.transform.localScale = Vector3.one;
    }
}

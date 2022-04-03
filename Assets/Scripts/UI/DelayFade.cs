using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayFade : MonoBehaviour
{
    public float delay = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Fade", delay);
    }

    public void Fade()
    {
        GetComponent<AnimatedPanel>().FadeOut();
    }
}

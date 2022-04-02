using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformCurves : MonoBehaviour
{
    public float duration = 1.0f;
    public AnimationCurve scaleCurve = null;
    public AnimationCurve y_positionCurve = null;

    bool running = false;
    private float timer = 0.0f;

    private Vector3 origScale;
    private Vector3 origLocalPosition;
    // Start is called before the first frame update
    void Start()
    {
        origScale = transform.localScale;
        origLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(running)
        {
            timer += Time.deltaTime;
            float scale = scaleCurve.Evaluate(timer / duration);
            transform.localScale = new Vector3(scale, scale, scale);
            Vector3 posDelta = new Vector3(0, y_positionCurve.Evaluate(timer / duration), 0);
            transform.localPosition = origLocalPosition + posDelta;
            if(timer > duration)
            {
                Restore();
            }
        }
        
    }
    public void Run()
    {
        running = true;
        timer = 0.0f;
    }
    public void Restore()
    {
        transform.localScale = origScale;
        transform.localPosition = origLocalPosition;
        running = false;
        timer = 0.0f;
    }
}

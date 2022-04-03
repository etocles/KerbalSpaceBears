using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ProgressIcon : MonoBehaviour
{
    public Image fillImage;
    private float timer;
    private bool runTimer = false;
    private float timeToRunTimer = 0.0f;
    public void Start()
    {
        
    }
    public void Update()
    {
        if(runTimer)
        {
            timer += Time.deltaTime;
            SetFillImage(timer / timeToRunTimer);
            if(timer > timeToRunTimer)
            {
                Destroy(gameObject);
            }
        }
    }
    public void StartTimer(float duration)
    {
        timer = 0.0f;
        runTimer = true;
        timeToRunTimer = duration;
    }
    public void SetFillImage(float value)
    {
        fillImage.fillAmount = value;
    }
}

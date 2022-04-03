using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource HoverSFX;
    public AudioSource bearSFX;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Hover_SFX()
    {
        HoverSFX.pitch = 1.0f;
        HoverSFX.Play();
    }
    public void Press_SFX()
    {
        HoverSFX.pitch = 0.8f;
        HoverSFX.Play();
    }
    public void Release_SFX()
    {
        HoverSFX.pitch = 1.2f;
        HoverSFX.Play();
    }
    public void Bear_SFX()
    {
        bearSFX.pitch = Random.Range(0.9f, 1.1f);
        bearSFX.Play();
    }
    public void Rocket_SFX_Short()
    {
        RocketScript.instance.SFX_Thrusters_Short.Play();
    }
}

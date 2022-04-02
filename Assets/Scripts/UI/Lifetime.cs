using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    public float Duration = 1.0f;

    [HideInInspector] public float Timer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, Duration);
    }

    private void Update()
    {
        Timer += Time.deltaTime;
    }
}

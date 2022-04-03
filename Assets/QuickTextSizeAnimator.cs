using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickTextSizeAnimator : MonoBehaviour
{

    private float min = 0.9f;
    private float max = 1.1f;

    private float maxRotate = 0.05f;
    private float rotate = 0.1f;

    private bool change;

    void Start(){
        change = false;
    }

    void Update(){
        if(Mathf.Abs(transform.rotation.z) < maxRotate){
            transform.Rotate(new Vector3(0, 0, rotate) * Time.deltaTime); 
        } else {
            rotate *= -1;
            transform.Rotate(new Vector3(0, 0, rotate) * Time.deltaTime); 
        }
    }
}

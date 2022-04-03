using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketMenu : MonoBehaviour
{
    public ParticleSystem Thrusters;
    public AudioSource SFX_Thrusters_Long;

    public IEnumerator Launch(){

        Thrusters.Play();
        SFX_Thrusters_Long.Play();

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos; endPos.y = 1;

        float t = 0.0f;
        while (t <= 3.0f)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t / 1.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = endPos;
    }
}

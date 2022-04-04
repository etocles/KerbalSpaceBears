using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalResult : MonoBehaviour
{
    float time;
    int bears;
    int planets;
    int fish;
    int oil;

    public TMP_Text time_text;
    public TMP_Text bear_text;
    public TMP_Text planet_text;
    public TMP_Text fish_text;
    public TMP_Text oil_text;

    public Transform bear_layout;
    public Transform planet_layout;
    public Transform fish_layout;
    public Transform oil_layout;

    public GameObject bear_icon;
    public GameObject planet_icon;
    public GameObject fish_icon;
    public GameObject oil_icon;


    void UpdateAll(){
        time = PlayerPrefs.GetFloat("_start_time", 0.0f);
        bears = PlayerPrefs.GetInt("_num_tamed_bears", 0);
        planets = PlayerPrefs.GetInt("_planets_traveled", 0);
        fish = PlayerPrefs.GetInt("_num_fish_obtained", 0);
        oil = PlayerPrefs.GetInt("_num_oil_obtained", 0);
    }

    void Start(){
        time = 94.0f;
        bears = 5;
        planets = 10;
        fish = 19;
        oil = 23;
        StartCoroutine("end");
    }

    public IEnumerator end(){
        yield return StartCoroutine("timer");
        yield return StartCoroutine(addTo(bear_icon, bear_layout, bears));
        yield return StartCoroutine(addTo(planet_icon, planet_layout, planets));
        yield return StartCoroutine(addTo(fish_icon, fish_layout, fish));
        yield return StartCoroutine(addTo(oil_icon, oil_layout, oil));
    }

    public IEnumerator addTo(GameObject icon, Transform location, int total){
        yield return new WaitForSeconds(1.0f);
        for(int x = 0; x < total; x++){
            //Debug.Log(x);
            GameObject i = Instantiate(icon, transform.position, Quaternion.identity);
            i.transform.parent = location;
            //i.transform.SetParent(location);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator timer(){
        float t = 0.0f;
        while(t < time){
            t += 1.0f;
            float minutes = Mathf.FloorToInt(t / 60); 
            float seconds = Mathf.FloorToInt(t % 60);
            time_text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return new WaitForSeconds(0.01f);
        }
    }
}

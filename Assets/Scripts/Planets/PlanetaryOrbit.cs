using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetaryOrbit : MonoBehaviour
{
    [SerializeField] float spinSpeed = 1.0f;
    [SerializeField] BezierCurve curve;

/*

 void Update(){
     //round lerp value down to int
     indexNum = Mathf.FloorToInt(moveSpeed);
     //increase lerp value relative to the distance between points to keep the speed consistent.
     moveSpeed += speed/Vector3.Distance(positions[indexNum], positions[indexNum+1]);
     //and lerp
     player.transform.position = Vector3.Lerp(positions[indexNum], positions[indexNum+1], moveSpeed-indexNum);
 }
*/
    // Update is called once per frame
    void Update(){
        // rotation of the planet
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
        // movement based on the curve
        //transform.position = Vector3.Lerp()
    }
}   

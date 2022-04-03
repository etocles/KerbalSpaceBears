using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIObject : MonoBehaviour
{
    // Attach this to objects that appear in the UI but must follow the position of a world object.
    public GameObject ObjectToFollow;
    private Vector3 offset = new Vector3(0, 15, 0);
    
    public void Initialize(GameObject target)
    {
        ObjectToFollow = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (ObjectToFollow != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(ObjectToFollow.transform.position) + offset;
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else Destroy(gameObject);
            
    }
}

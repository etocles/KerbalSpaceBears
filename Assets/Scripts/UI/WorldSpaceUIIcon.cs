using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceUIIcon : MonoBehaviour
{
    public Sprite sprite;
    // Start is called before the first frame update
    void Start()
    {
        if(GameplayCanvas.instance != null) GameplayCanvas.instance.CreateIcon(sprite, gameObject);
    }
}

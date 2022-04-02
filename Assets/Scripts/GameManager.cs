using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Hexsphere ActivePlanet;

    [HideInInspector] public Tile SelectedTile;



    private void OnValidate()
    {
        if (instance == null) instance = this;
    }
    private void Awake()
    {
        if(instance == null) instance = this;
    }

    
    public void SelectTile(Tile tile)
    {
        if(SelectedTile != null && tile != SelectedTile)
        {
            DeselectTile();
        }
        SelectedTile = tile;
        SelectedTile.SetHighlight(0.75f);
        SelectedTile.Selected = true;
    }
    public void DeselectTile()
    {
        SelectedTile.SetHighlight(0.0f);
        SelectedTile.Selected = false;
        SelectedTile = null;
    }

    public void KillBears(){
        MobileUnit[] components = GameObject.FindObjectsOfType<MobileUnit>();
        foreach(MobileUnit mu in components){
            Destroy(mu.getGameObject());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        KillBears();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

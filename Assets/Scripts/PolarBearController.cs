using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarBearController : MonoBehaviour
{
    private MobileUnit Unit;
    // Start is called before the first frame update
    void Start(){
        Unit = GetComponent<MobileUnit>();
        Tile.OnTileClickedAction += OnTileClicked;
    }

    public void OnTileClicked(Tile tile){
        if(!Unit.moving)
        {
            Stack<Tile> path;
            if(Hexsphere.planetInstances[0].navManager.findPath(Unit.currentTile, tile, out path))
            {
                Unit.moveOnPath(path);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarBearController : MonoBehaviour {

    public enum BearState{
        DEFAULT, FISH, OIL, LOST
    }

    private Tile shipTile;
    private MobileUnit Unit;
    private BearState state;
    // Start is called before the first frame update
    void Start(){
        state = DEFAULT;
        Unit = GetComponent<MobileUnit>();
        Tile.OnTileClickedAction += OnTileClicked;

    }

    public IEnumerator GetFish(Tile tile, Stack<Tile> fishPath){
        // tile (temp) = ship starting origin
        state = FISH;
        // yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        yield return StartCoroutine(SearchForFish(fishPath));
        //if(path == null) -> lost state (?)
        yield return new WaitForSeconds(5.0f);
        ReturnToShip();

        yield return null;
            
    }

    public void ReturnToShip(){
        if(!Unit.moving){
            Stack<Tile> path = new Stack<Tile>();
            if(Hexsphere.planetInstances[0].navManager.findPath(Unit.currentTile, shipTile, out path)){
                Unit.moveOnPath(path);
            }
        }
    }

    private Stack<Tile> SearchForOil(Stack<Tile> path){
        if(!Unit.moving){
            if(path != null || Hexsphere.planetInstances[0].navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Oil, Unit.currentTile, out path)){
                    Unit.currentTile.Occupied = false;
                    Unit.moveOnPath(path);
            }
        }
        return path;
    }

    private IEnumerator SearchForFish(Stack<Tile> path){
        if(!Unit.moving){
            if(Hexsphere.planetInstances[0].navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Fish, Unit.currentTile, out path)){
                yield return Unit.moveOnPathCoroutine(path);
            }
        }
    }

    public void OnTileClicked(Tile tile){
        //StartCoroutine(GetFish(tile, null));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarBearController : MonoBehaviour {

    public enum BearState {
        DEFAULT, FISH, OIL, LOST, SHIP
    }

    [SerializeField] float gatheringTime = 5.0f;

    private Tile shipTile;
    private MobileUnit Unit;
    private BearState state;
    // Start is called before the first frame update
    void Start(){
        state = BearState.DEFAULT;
        Unit = GetComponent<MobileUnit>();
        //Tile.OnTileClickedAction += OnTileClicked;
    }

    void ChangeState(BearState newState){
        if(state == newState)
            return;
        state = newState;
        switch(newState){
            case BearState.FISH:
                break;
            case BearState.OIL:
                break;
            case BearState.LOST:
                break;
            case BearState.SHIP:
                break;
            default:
                break;
        }
    }

    public IEnumerator GetFish(Tile tile, Stack<Tile> fishPath){
        // tile (temp) = ship starting origin
        ChangeState(BearState.FISH);
        yield return StartCoroutine(SearchForFish(fishPath));
        //if(path == null) -> lost state (?)
        yield return new WaitForSeconds(gatheringTime);
        ReturnToShip();

        yield return null;
    }

    public IEnumerator GetOil(Tile tile, Stack<Tile> oilPath){
        // tile (temp) = ship starting origin
        ChangeState(BearState.OIL);
        yield return StartCoroutine(SearchForOil(oilPath));
        //if(path == null) -> lost state (?)
        yield return new WaitForSeconds(gatheringTime);
        ReturnToShip();
    }

    public void ReturnToShip(){
        state = BearState.SHIP;
        if(!Unit.moving){
            Stack<Tile> path = new Stack<Tile>();
            if(Hexsphere.planetInstances[0].navManager.findPath(Unit.currentTile, shipTile, out path)){
                Unit.moveOnPath(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }

    private IEnumerator SearchForOil(Stack<Tile> path){
        if(!Unit.moving){
            if(Hexsphere.planetInstances[0].navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Oil, Unit.currentTile, out path)){
                Unit.currentTile.Occupied = false;
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }

    private IEnumerator SearchForFish(Stack<Tile> path){
        if(!Unit.moving){
            if(Hexsphere.planetInstances[0].navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Fish, Unit.currentTile, out path)){
                Unit.currentTile.Occupied = false;
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }

    public void OnTileClicked(Tile tile){
        //StartCoroutine(GetFish(tile, null));
    }
}

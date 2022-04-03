using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PolarBearController : Bear {

    public enum BearState {
        DEFAULT, FISH, OIL, LOST, SHIP
    }
    
    [SerializeField] private float oilGatheringTime;
    [SerializeField] private float fishGatheringTime;

    private Tile shipTile;
    private MobileUnit Unit;
    private BearState state;
    // Start is called before the first frame update
    void Start(){
        state = BearState.DEFAULT;
        Unit = GetComponent<MobileUnit>();
        //Tile.OnTileClickedAction += OnTileClicked;
        GameplayCanvas.instance.OnSearchForFish.AddListener(() => { if (gameObject.activeSelf) StartCoroutine(GetFish()); });
        GameplayCanvas.instance.OnSearchForOil.AddListener(() => { if (gameObject.activeSelf) StartCoroutine(GetOil()); });
    }

    public void Die()
    {
        // subtract self from the bears owned array in the Rocket script
        GameManager.instance.Rocket.GetComponent<RocketScript>().BearsOwned.Remove(gameObject);
        // after all ties are severed
        // commit Die
        Destroy(gameObject);
    }

    public void ChangeState(BearState newState){
        //Debug.Log("Changing to: " + newState.ToString());
        if(state == newState)
            return;
        state = newState;
        switch(newState){
            case BearState.FISH:
                break;
            case BearState.OIL:
                break;
            case BearState.LOST:
                Debug.Log("CANVAS: Bear is confused!");
                break;
            case BearState.SHIP:
                break;
            default:
                break;
        }
    }

    public IEnumerator GetFish(){
        // fires from context menu, so first have to check
        // if we're the right bear
        if (Unit.currentTile != GameManager.instance.SelectedTile) yield break;
        // tile (temp) = ship starting origin
        ChangeState(BearState.FISH);
        yield return StartCoroutine(SearchForFish(new Stack<Tile>()));
        //if(path == null) -> lost state (?)
        yield return new WaitForSeconds(fishGatheringTime);
        yield return StartCoroutine(ReturnToShip());
    }

    public IEnumerator GetOil(){
        // fires from context menu, so first have to check
        // if we're the right bear
        if (Unit.currentTile != GameManager.instance.SelectedTile) yield break;
        // tile (temp) = ship starting origin
        ChangeState(BearState.OIL);
        yield return StartCoroutine(SearchForOil(new Stack<Tile>()));
        //if(path == null) -> lost state (?)
        yield return new WaitForSeconds(oilGatheringTime);
        yield return StartCoroutine(ReturnToShip());
    }

    private void ConsumeResource(Tile tile)
    {
        // make sure we can get there
        tile.Occupied = false;

        // remove the placed object that's NOT a bear
        List<GameObject> temp = new List<GameObject>();
        foreach (GameObject obj in tile.PlacedObjects)
        {
            // if it doesn't have a mobility unit, it's not a bear, keep going
            if (obj.GetComponent<MobileUnit>() == null) temp.Add(obj);
            else Destroy(obj);
        }
        // set our new List to be without the Fish/Oil model on top
        tile.PlacedObjects = temp;

        // necessary upkeep for TilesByBiome
        tile.parentPlanet.TilesByBiome[tile.BiomeType].Remove(tile);
        tile.parentPlanet.TilesByBiome[Hexsphere.BiomeType.Ice].Add(tile);
        // change our BiomeType to Ice
        tile.BiomeType = Hexsphere.BiomeType.Ice;
    }

    public IEnumerator ReturnToShip(){
        if(!Unit.moving){
            Stack<Tile> path = new Stack<Tile>();
            RocketScript rocket = GameManager.instance.Rocket.GetComponent<RocketScript>();

            Tile dest = (rocket.GetUnOccupiedTile() == null) ? Unit.currentTile : rocket.GetUnOccupiedTile();

            // if we are boarding ship, destination is the shipTile
            if (state == BearState.SHIP) {
                dest = shipTile;
                dest.activeBear = ActiveBear.None; // just in case
            }
            // otherwise reserve that position
            else
            {
                dest.Occupied = true;
                dest.activeBear = ActiveBear.Tamed; // just in case
            }


            // try to find a path, if exists, traverse it
            if (GameManager.instance.ActivePlanet.navManager.findPath(Unit.currentTile, dest, out path))
            {
                // consume the resource
                ConsumeResource(Unit.currentTile);
                // we want others to be able to get to the ship
                if (state == BearState.SHIP) dest.Occupied = false;
                yield return Unit.moveOnPathCoroutine(path);
            } 
            // if there's no path, assume deserteds
            else 
            {
                ChangeState(BearState.LOST);
            }

        }

        switch (state)
        {
            // abrupt end to journey, report as lost 
            case BearState.LOST:
                break;
            // completed journey, add a fish
            case BearState.FISH:
                GameManager.instance.Rocket.GetComponent<RocketScript>().AddFish(1);
                break;
            // completed journey, add an oil
            case BearState.OIL:
                GameManager.instance.Rocket.GetComponent<RocketScript>().AddOil(1);
                break;
            // completed journey, board the ship
            case BearState.SHIP:
                GameManager.instance.Rocket.GetComponent<RocketScript>().BoardBear(gameObject);
                gameObject.SetActive(false);
                break;
        }
    }

    private IEnumerator SearchForOil(Stack<Tile> path){
        if(!Unit.moving){
            if(GameManager.instance.ActivePlanet.navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Oil, Unit.currentTile, out path)){
                Unit.currentTile.Occupied = false;
                Unit.currentTile.activeBear = ActiveBear.None;
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }

    private IEnumerator SearchForFish(Stack<Tile> path){
        if(!Unit.moving){
            if(GameManager.instance.ActivePlanet.navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Fish, Unit.currentTile, out path)){
                Unit.currentTile.Occupied = false;
                Unit.currentTile.activeBear = ActiveBear.None;
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }
    public void SetShipTile(Tile tile) => shipTile = tile;

    public void OnTileClicked(Tile tile){
        //StartCoroutine(GetFish(tile, null));
    }
}

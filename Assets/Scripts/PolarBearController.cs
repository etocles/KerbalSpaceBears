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
    public MobileUnit Unit;
    public BearState state;
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

    public GameObject currentIcon = null;
    public void ChangeState(BearState newState){
        //Debug.Log("Changing to: " + newState.ToString());
        
        if (state == newState)
            return;
        state = newState;
        currentIcon = GameplayCanvas.instance.CreateIcon(GameplayCanvas.instance.GetIconByBearState(state), gameObject);
        switch (newState){
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

    public void StartAutoHarvest(Tile tile)
    {
        StartCoroutine(AutoHarvest(tile));
    }
    public IEnumerator AutoHarvest(Tile tile)
    {
        // change state appropriately
        BearState temp2 = tile.BiomeType == Hexsphere.BiomeType.Fish ? BearState.FISH : BearState.OIL;
        if (tile.BiomeType == Hexsphere.BiomeType.Fish) ChangeState(BearState.FISH);
        if (tile.BiomeType == Hexsphere.BiomeType.Oil) ChangeState(BearState.OIL);
        bool gettingFish = state == BearState.FISH;
        Sprite icon = (gettingFish) ? GameplayCanvas.instance.FishIcon : GameplayCanvas.instance.OilIcon;
        string txt = (gettingFish) ? "+1 Fish" : "+1 Oil";
        float timeToWait = (gettingFish) ? fishGatheringTime : oilGatheringTime;

        GameObject spawnedProgressUI = GameplayCanvas.instance.CreateIcon(icon, gameObject, GameplayCanvas.instance.ProgressPrefab);
        spawnedProgressUI.GetComponent<ProgressIcon>().StartTimer(timeToWait);
        yield return new WaitForSeconds(timeToWait);
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, txt, gameObject.transform.position);
        ConsumeResource(Unit.currentTile);

        switch (temp2)
        {
            // completed journey, add a fish
            case BearState.FISH:
                GameManager.instance.Rocket.GetComponent<RocketScript>().AddFish(1);
                break;
            // completed journey, add an oil
            case BearState.OIL:
                GameManager.instance.Rocket.GetComponent<RocketScript>().AddOil(1);
                break;
        }
        tile.Occupied = true; // might need to be moved
    }

    public IEnumerator GetFish(){
        // fires from context menu, so first have to check
        // if we're the right bear
        if (Unit.currentTile != GameManager.instance.SelectedTile) yield break;
        // tile (temp) = ship starting origin
        ChangeState(BearState.FISH);
        yield return StartCoroutine(SearchForFish(new Stack<Tile>()));
        // If path to oil was not found, abandon execution
        if (state == BearState.LOST) yield break;
        GameObject spawnedProgressUI = GameplayCanvas.instance.CreateIcon(GameplayCanvas.instance.FishIcon, gameObject, GameplayCanvas.instance.ProgressPrefab);
        spawnedProgressUI.GetComponent<ProgressIcon>().StartTimer(fishGatheringTime);
        yield return new WaitForSeconds(fishGatheringTime);
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, "+1 Fish", gameObject.transform.position); // TODO: Maybe baxk to fish icon
        yield return StartCoroutine(ReturnToShip());
    }

    public IEnumerator GetOil(){
        // fires from context menu, so first have to check
        // if we're the right bear
        if (Unit.currentTile != GameManager.instance.SelectedTile) yield break;
        // tile (temp) = ship starting origin
        ChangeState(BearState.OIL);
        
        yield return StartCoroutine(SearchForOil(new Stack<Tile>()));
        // If path to oil was not found, abandon execution
        if (state == BearState.LOST) yield break;
        GameObject spawnedProgressUI = GameplayCanvas.instance.CreateIcon(GameplayCanvas.instance.OilIcon, gameObject, GameplayCanvas.instance.ProgressPrefab);
        spawnedProgressUI.GetComponent<ProgressIcon>().StartTimer(oilGatheringTime);
        yield return new WaitForSeconds(oilGatheringTime);
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, "+1 Oil", gameObject.transform.position);
        yield return StartCoroutine(ReturnToShip());
    }

    private void ConsumeResource(Tile tile)
    {

        // don't consume any resources if you're coming back from an ice tile
        if (tile.BiomeType == Hexsphere.BiomeType.Ice) return;


        // make sure we can get there
        tile.Occupied = false;

        // remove the placed object that's NOT a bear
        List<GameObject> temp = new List<GameObject>();
        foreach (GameObject obj in tile.PlacedObjects)
        {
            // if it doesn't have a mobility unit, it's not a bear, delete it
            if (obj.GetComponent<MobileUnit>() == null)
            {
                if (tile.BiomeType == Hexsphere.BiomeType.Oil)
                {
                    // remove the oil particle effect
                    Destroy(obj.transform.GetChild(0).gameObject);
                }
                else
                {
                    // remove the Fish model in its entirety
                    Destroy(obj);
                }
            }
            else
            {
                temp.Add(obj);
            }
        }
        // set our new List to be without the Fish/Oil model on top
        tile.PlacedObjects = temp;

        if (tile.BiomeType == Hexsphere.BiomeType.Oil)
        {
            // necessary upkeep for TilesByBiome
            GameManager.instance.ActivePlanet.TilesByBiome[tile.BiomeType].Remove(tile);
        }
        
        else if (tile.BiomeType == Hexsphere.BiomeType.Fish)
        {
            // necessary upkeep for TilesByBiome
            GameManager.instance.ActivePlanet.TilesByBiome[tile.BiomeType].Remove(tile);
            //GameManager.instance.ActivePlanet.TilesByBiome[Hexsphere.BiomeType.Ice].Add(tile);
            // change our BiomeType to Ice
            tile.BiomeType = Hexsphere.BiomeType.Ice;
            tile.SetGroupID(tile.parentPlanet.FindBiomeIDByType(Hexsphere.BiomeType.Ice));
        }
        if (tile.ExtrudedHeight <= 0.0000000001f) tile.ExtrudedHeight = 0.0001f;
    }

    public IEnumerator ReturnToShip(){
        if (Unit.moving) yield break;
        Stack<Tile> path = new Stack<Tile>();
        RocketScript rocket = GameManager.instance.Rocket.GetComponent<RocketScript>();

        Tile dest = rocket.GetUnOccupiedTile();

        bool tilesFull = false;
        // if we are boarding ship, destination is the shipTile
        if (state == BearState.SHIP) {
            dest = shipTile;
            dest.activeBear = ActiveBear.None; // just in case
        }
        // if there's no adjacent spot empty next to the ship, route to ship
        else if (dest == null)
        {
            dest = shipTile;
            dest.activeBear = ActiveBear.None; // just in case
            tilesFull = true;
        }
        // otherwise reserve that position (around the ship)
        else
        {
            dest.Occupied = true;
            dest.activeBear = ActiveBear.Tamed; // just in case
        }


        // try to find a path, if exists, traverse it
        if (GameManager.instance.ActivePlanet.navManager.findPath(
            Unit.currentTile, 
            dest,
            out path))
        {
            // consume the resource (if on ice, nothing happens)
            ConsumeResource(Unit.currentTile);
            // we want others to be able to get to the ship
            if (dest == shipTile) dest.Occupied = false;
            yield return Unit.moveOnPathCoroutine(path);
        } 
        // if there's no path, assume deserteds
        else 
        {
            ChangeState(BearState.LOST);
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
                // board taken care of in can continue
                break;
        }

        // if returning to ship but neighbors are full, board for free and delete self
        bool success = false; 
        if (tilesFull)
        {
            success = GameManager.instance.Rocket.GetComponent<RocketScript>().BoardBear(gameObject, true);
            gameObject.SetActive(!success);
            if (success) Destroy(currentIcon);
        }
    }

    private IEnumerator SearchForOil(Stack<Tile> path){
        if(Unit.currentTile.BiomeType == Hexsphere.BiomeType.Oil)
            yield break;
        if(!Unit.moving){
            if(GameManager.instance.ActivePlanet.navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Oil, Unit.currentTile, out path)){
                Unit.currentTile.Occupied = false;
                Unit.currentTile.activeBear = ActiveBear.None;
                Unit.currentTile.currentBear = null;
                GetComponent<Outline>().enabled = false;
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }

    private IEnumerator SearchForFish(Stack<Tile> path){
        if(Unit.currentTile.BiomeType == Hexsphere.BiomeType.Fish) yield break; 
        if(!Unit.moving){
            if(GameManager.instance.ActivePlanet.navManager.
            FindClosestIDTiles(Hexsphere.BiomeType.Fish, Unit.currentTile, out path)){
                Unit.currentTile.Occupied = false;
                Unit.currentTile.activeBear = ActiveBear.None;
                Unit.currentTile.currentBear = null;
                GetComponent<Outline>().enabled = false;
                yield return Unit.moveOnPathCoroutine(path);
            } else {
                ChangeState(BearState.LOST);
            }
        }
    }
    public void SetShipTile(Tile tile) => shipTile = tile;
    public Tile GetShipTile()=>shipTile;

    public void OnTileClicked(Tile tile){
        //StartCoroutine(GetFish(tile, null));
    }
}

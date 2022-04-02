using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Hexsphere ActivePlanet;
    public UnityEvent OnGameOver;
    public UnityEvent OnGameStart;
    public UnityEvent OnTileSelected;

    private List<GameObject> spawnedPolarBears = new List<GameObject>();
    public GameObject PolarBearPrefab;
    public GameObject RocketPrefab;
    //public GameObject Rocket = null;
    private bool GameStarted = false;

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
        OnTileSelected?.Invoke();
    }
    public void DeselectTile()
    {
        SelectedTile.SetHighlight(0.0f);
        SelectedTile.Selected = false;
        SelectedTile = null;
    }

    public void SpawnPolarBears(int count)
    {
        for(int p = spawnedPolarBears.Count - 1; p >= 0; p--)
        {
            DestroyImmediate(spawnedPolarBears[p]);
            spawnedPolarBears.RemoveAt(p);
        }
        HashSet<int> randomTiles = new HashSet<int>();
        for(int i = 0; i < count; i++)
        {
            int num = Random.Range(0, ActivePlanet.GetTilesByBiome(Hexsphere.BiomeType.Ice).Count);
            randomTiles.Add(num);
        }
        foreach(int spawnLoc in randomTiles)
        {
            PlacePolarBear(ActivePlanet.GetTilesByBiome(Hexsphere.BiomeType.Ice)[spawnLoc]);
        }
    }
    public GameObject PlacePolarBear(Tile location)
    {
        GameObject spawnedBear = Instantiate(PolarBearPrefab);
        spawnedBear.GetComponent<MobileUnit>().parentPlanet = ActivePlanet;
        spawnedBear.GetComponent<MobileUnit>().currentTile = location;
        location.placeObject(spawnedBear);
        spawnedPolarBears.Add(spawnedBear);
        return spawnedBear;
    }

    public void KillBears(){
        MobileUnit[] components = GameObject.FindObjectsOfType<MobileUnit>();
        foreach(MobileUnit mu in components){
            Destroy(mu.getGameObject());
        }
    }

    public void InitGame()
    {
        KillBears();
        SpawnPolarBears(10);
        // greet user by using GamePlay canvas to convey that we have to land on a planet
        /*
         *  TODO: Implement
         */
        // wait for tile to be selected
        OnTileSelected.AddListener(FirstLanding);
        OnGameStart.AddListener(StartGame);
    }

    public void FirstLanding()
    {
        // only fire once
        OnTileSelected.RemoveListener(FirstLanding);
        // Instantiate the ship
        GameObject Rocket = Instantiate(RocketPrefab, SelectedTile.transform);
        // Do landing sequence
        Rocket.GetComponent<RocketScript>().FirstLanding(SelectedTile);
    }

    private void StartGame() { GameStarted = true; }

    // Start is called before the first frame update
    void Start()
    {
        InitGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(ActivePlanet != null && GameStarted) ActivePlanet.Melt(Time.deltaTime);
    }
}

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
    public UnityEvent<Hexsphere> OnRocketLanded;
    public UnityEvent OnTileSelected;

    private List<GameObject> spawnedPolarBears = new List<GameObject>();
    public GameObject PolarBearPrefab;
    public GameObject SpaceBearPrefab;
    public GameObject RocketPrefab;
    public GameObject Rocket = null;
    private bool GameStarted = false;

    [HideInInspector] public Tile SelectedTile;

    // meltRate per second
    [SerializeField] float meltRate = 0.1f;



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
        GameplayCanvas.instance.DisplayContextMenu(SelectedTile.gameObject);
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

    public void InitGame()
    {
        // wait for tile to be selected
        OnTileSelected.AddListener(FirstLanding);
        OnGameStart.AddListener(StartGame);
    }

    public void FirstLanding()
    {
        // only fire once
        OnTileSelected.RemoveListener(FirstLanding);
        // Instantiate the ship
        Rocket = Instantiate(RocketPrefab, SelectedTile.transform);
        Rocket.GetComponent<RocketScript>().SpaceBearPrefab = SpaceBearPrefab;
        // Do landing sequence
        Rocket.GetComponent<RocketScript>().FirstLanding(SelectedTile);
    }

    private IEnumerator MeltingCoroutine(){
        yield return new WaitForSeconds(1.0f);
        ActivePlanet.Melt(meltRate);
        StartCoroutine("MeltingCoroutine");
    }

    private void StartGame() { GameStarted = true; }
    private void SetActivePlanet(Hexsphere planet) { 
        // assume set active planet is where we start with the planet
        ActivePlanet = planet;
        StartCoroutine("MeltingCoroutine");
    }

    // Start is called before the first frame update
    void Start()
    {
        InitGame();
        OnRocketLanded.AddListener(SetActivePlanet);
    }

    // Update is called once per frame
    void Update()
    {
        //if(ActivePlanet != null && GameStarted) ActivePlanet.Melt(Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Hexsphere ActivePlanet;
    public UnityEvent OnGameOver;
    public UnityEvent OnGameStart;
    public UnityEvent<Hexsphere> OnRocketLanded;
    public UnityEvent OnTileSelected;
    public UnityEvent OnContextConfirmFirstLand;

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
        if (SelectedTile != null && tile != SelectedTile)
        {
            DeselectTile();
        }
        SelectedTile = tile;
        SelectedTile.SetHighlight(0.75f);
        SelectedTile.Selected = true;
        if(SelectedTile.currentBear != null)
        {
            SelectedTile.currentBear.GetComponent<Outline>().enabled = true;
        }
        OnTileSelected?.Invoke();
        if(SelectedTile.parentPlanet == GameStateController.instance.ActiveHexsphere) GameplayCanvas.instance.DisplayContextMenu(SelectedTile.gameObject);
    }
    public void DeselectTile()
    {
        if(SelectedTile.currentBear != null) SelectedTile.currentBear.GetComponent<Outline>().enabled = false;
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
        OnRocketLanded.AddListener(SetActivePlanet);
        OnGameStart.AddListener(StartGame);
        OnGameOver.AddListener(EndGame);
        GameplayCanvas.instance.OnFirstLanding.AddListener(FirstLanding);
    }

    public static bool ValidTileForLanding(Tile tile)
    {
        if (instance.SelectedTile == null) return false;
        // not occupied, is ice, not tameable bear on it (or old bear)
        return !instance.SelectedTile.Occupied 
            && instance.SelectedTile.BiomeType == Hexsphere.BiomeType.Ice
            && instance.SelectedTile.activeBear == ActiveBear.None;
    }
    public void FirstLanding()
    {
        // make sure it is a valid tile to land on
        if (!ValidTileForLanding(SelectedTile)) return;
        // Instantiate the ship
        Rocket = Instantiate(RocketPrefab, SelectedTile.transform);
        Rocket.GetComponent<RocketScript>().SpaceBearPrefab = SpaceBearPrefab;
        // Do landing sequence
        Rocket.GetComponent<RocketScript>().FirstLanding(SelectedTile);
    }

    private IEnumerator MeltingCoroutine(){
        while (true)
        {
            ActivePlanet.Melt(meltRate);
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void StartGame() { GameStarted = true; }
    private void EndGame() {
        RocketScript.Stats temp = GameManager.instance.Rocket.GetComponent<RocketScript>().MyStats;

        PlayerPrefs.SetFloat("_start_time", Mathf.Abs(temp.start_time - Time.time));
        PlayerPrefs.SetInt("_num_tamed_bears", temp.num_tamed_bears);
        PlayerPrefs.SetInt("_planets_traveled", temp.planets_traveled.Count);
        PlayerPrefs.SetInt("_num_fish_obtained", temp.num_fish_obtained);
        PlayerPrefs.SetInt("_num_oil_obtained", temp.num_oil_obtained);
        SceneManager.LoadScene("GameOver"); 
    }
    private void SetActivePlanet(Hexsphere planet) {
        // assume set active planet is where we start with the planet
        //try
        //{
        //    StopCoroutine("MelingCoroutine");
        //}
        //catch (System.Exception e) { }
        ActivePlanet = planet;
        //StartCoroutine("MeltingCoroutine");
    }

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

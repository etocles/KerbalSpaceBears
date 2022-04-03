using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour {
    public static RocketScript instance;

    public GameObject fakeBear;

    public Tile CurrentTile;
    public Tile DestinationTile;
    public ParticleSystem Thrusters;

    public HashSet<GameObject> BearsOwned;
    public HashSet<GameObject> BearsBoarded;
    public int NumBears => BearsOwned.Count;
    public int NumOil = 0;
    public int NumFish = 0;
    public bool CanLaunch => (NumBears >= BearThreshold) && (NumOil >= OilThreshold);
    public bool Traveling = false;
    public bool Sank = false;



    [Tooltip("How many fish per bear")]
    public static int FishPerBear = 2;
    [Tooltip("Minimum bears for takeoff")]
    public static int BearThreshold = 2;
    [Tooltip("Minimum oil for takeoff")]
    public static int OilThreshold = 10;
    [Tooltip("Starting Amount of Bears")]
    public static int StartingBears = 3;
    [Tooltip("Starting Amount of Fish")]
    public static int StartingFish = 3;
    [SerializeField]
    public GameObject SpaceBearPrefab;


    private bool firstFishObtained = true;
    private bool firstOilObtained = true;
    private bool firstBearObtained = true;
    private bool firstPlanetTravel = true;
    private bool firstEnoughFuel = true;

    public AudioSource SFX_Thrusters_Long;
    public AudioSource SFX_Thrusters_Short;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        BearsOwned = new HashSet<GameObject>();
        for (int i = 0; i < StartingBears; i++)
        {
            GameObject bear = Instantiate(SpaceBearPrefab);
            bear.SetActive(false);
            BearsOwned.Add(bear);
        }
        BearsBoarded = new HashSet<GameObject>(BearsOwned);
        // TODO: Subscribe to canvas's events
        GameplayCanvas.instance.OnNavigateWithShip.AddListener(StartLaunch);
        UpdateSliders();
        
    }

    public void UpdateSliders()
    {
        GameplayCanvas.instance.SetResourceSliderValue(GameplayCanvas.Resource.Fish, Mathf.Clamp01((float)NumFish / (float)NumBears), NumFish.ToString());
        GameplayCanvas.instance.SetResourceSliderValue(GameplayCanvas.Resource.Oil, Mathf.Clamp01((float)NumOil / (float)OilThreshold), NumOil.ToString());
        GameplayCanvas.instance.SetResourceSliderValue(GameplayCanvas.Resource.Bear, 1, NumBears.ToString());
        GameplayCanvas.instance.SetResourceSliderValue(GameplayCanvas.Resource.Heat, Mathf.Clamp01((float)NumFish / (float)NumBears), NumFish.ToString());
    }
    // Update is called once per frame
    void Update()
    {
        if (!Traveling && !Sank)
        {
            TrySink();
        }
    }

    #region Resource Functions
    public void AddOil(int amt) {
        if (firstOilObtained)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstOilObtained);
            firstOilObtained = false;
        }
        if (firstEnoughFuel)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnEnoughFuelAccumulated);
            firstEnoughFuel = false;
        }
        NumOil += amt;
        Debug.Log("CANVAS: +1 Oil!");
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.RocketIcon, "+" + amt.ToString() + " Oil", gameObject.transform.position);
        UpdateSliders();
    }

    public void AddFish(int amt) {
        if (firstFishObtained)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstFishObtained);
            firstFishObtained = false;
        }
        NumFish += amt;
        Debug.Log("CANVAS: +1 Fish!");
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.RocketIcon, "+" + amt.ToString() + " Fish", gameObject.transform.position);
        UpdateSliders();
    }

    public bool PayForBear(GameObject bear)
    {
        if (NumFish >= FishPerBear)
        {
            //TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstBearObtained);
            NumFish -= FishPerBear;
            bear.GetComponent<UntamedBear>().PaidFor = true;

            Debug.Log("CANVAS: -"+FishPerBear+" Fish!");
            GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.RocketIcon, "-" + FishPerBear.ToString() + " Fish", gameObject.transform.position);
            UpdateSliders();
            return true;
        }
        else
        {
            GameplayCanvas.instance.PushMessage("You need at least " + FishPerBear + " fish to recruit a bear!", 3);
            Debug.Log("CANVAS: Need at least "+FishPerBear+" fish to recruit a bear!");
            return false;
        }
    }
    public void RecruitBear(GameObject bear) {
        if (!bear.GetComponent<UntamedBear>().PaidFor) return;
        GameObject pfb = (bear.GetComponent<UntamedBear>().GetBearType() == Bear.BearType.Brown) 
                            ? GameStateController.instance.BearPrefabs[0] // 0 is brown
                            : GameStateController.instance.BearPrefabs[1]; // 1 is polar
        GameObject temp = Instantiate(pfb);
        BearsOwned.Add(temp);
        BearsBoarded.Add(temp);
        Debug.Log("CANVAS: +1 Bear!");
        UpdateSliders();
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, "+1 Bear", gameObject.transform.position);
        // if there's still bears on board, that means the ship is full.
        // coroutine is still emptying themt out, so we don't have to
        // if there's 1 (the one we just added), do a manual refresh
        if (BearsBoarded.Count == 1) {
            temp = UnboardBear();
            GameStateController.instance.DepositBear(temp, GetUnOccupiedTile());
            
        }
        Destroy(bear);
    }
    public void BoardBear(GameObject bear)
    {
        BearsBoarded.Add(bear);
    }
    public void BoardUntamedBear(){
        GameObject fake_bear = Instantiate(fakeBear, Vector3.zero, Quaternion.identity);
        BearsBoarded.Add(fake_bear);
    }
    public GameObject UnboardBear()
    {
        List<GameObject> temp = new List<GameObject>(BearsBoarded);
        GameObject b = temp[0];
        // remove bear from ship
        BearsBoarded.Remove(b);
        return b;
    }
    public void LeaveBehind()
    {
        // update our list of bears to only be the ones that made it on the ship
        BearsOwned.IntersectWith(BearsBoarded);
    }

    #endregion  


    void TrySink()
    {
        Vector3 newPos = transform.localPosition;
        newPos.y = CurrentTile.ExtrudedHeight * 2;
        transform.localPosition = newPos;
        transform.localEulerAngles = new Vector3();

        if (CurrentTile.ExtrudedHeight <= 0.0f)
        {
            StartCoroutine(TipOver());
            Sank = true;
        }
    }
    // leave atmosphere by incrementing 
    void StartLaunch() {
        //if (!CanLaunch) return;
        DestinationTile = GameManager.instance.SelectedTile;
        // see if we can land there
        if (!GameManager.ValidTileForLanding(DestinationTile)) return ;
        if (firstPlanetTravel)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstPlanetTraveledTo);
            firstPlanetTravel = false;
        }
        StartCoroutine(Launch()); 
    }

    public void FirstLanding(Tile tile) => StartCoroutine(FirstLandingCutscene(tile));

    public Tile GetUnOccupiedTile()
    {
        foreach (Tile t in CurrentTile.neighborTiles)
        {
            if (!t.Occupied && t.BiomeType == Hexsphere.BiomeType.Ice)
                return t;
        }
        return null;
    }
    #region Coroutines
    IEnumerator TipOver()
    {
        Vector3 startRot = transform.localEulerAngles;
        Vector3 endRot = new Vector3(50f,0,0f);

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = transform.localPosition;
        endPos.y = -0.3f;
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime;
            transform.localEulerAngles = Vector3.Lerp(startRot, endRot, t / 1.0f);
            transform.localPosition = Vector3.Lerp(startPos, endPos, t / 1.0f);
            yield return new WaitForEndOfFrame();
        }

        GameManager.instance.OnGameOver?.Invoke();
    }
    IEnumerator Launch()
    {
        // leave behind any bears we didn't pick up
        LeaveBehind();

        Traveling = true;

        Thrusters.Play();
        SFX_Thrusters_Long.Play();

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos; endPos.y = 1;

        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t / 1.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = endPos;

        // potentially point to look at where we're headed
        Vector3 dest = DestinationTile.parentPlanet.transform.TransformPoint(DestinationTile.transform.localPosition + new Vector3(0, 1.0f, 0));
        transform.LookAt(dest, transform.up);

        // parent to the scene
        transform.SetParent(null);

        StartCoroutine(Travel());
    }
    IEnumerator Land()
    {
        transform.SetParent(DestinationTile.transform);

        // rotate to properly land
        Vector3 startRot = transform.localEulerAngles;
        Vector3 endRot = new Vector3();
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime;
            transform.localEulerAngles = Vector3.Lerp(startRot, endRot, t / 0.3f);
            yield return new WaitForEndOfFrame();
        }
        transform.localEulerAngles = endRot;

        // land
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = new Vector3(0, CurrentTile.ExtrudedHeight * 2, 0);

        t = 0.0f;
        while (t <= 2.0f)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t / 2.0f);
            yield return new WaitForEndOfFrame();
        }

        Thrusters.Stop();

        Traveling = false;
        CurrentTile = DestinationTile;
        DestinationTile = null;
        GameManager.instance.OnRocketLanded?.Invoke(CurrentTile.parentPlanet);
        GameStateController.instance.UnboardBears();
    }
    IEnumerator Travel()
    {

        Vector3 startPos = transform.position;
        // TODO: Find appropriate position to fly to
        Vector3 endPos = DestinationTile.parentPlanet.transform.TransformPoint(DestinationTile.transform.localPosition + new Vector3(0, 2.0f, 0));
        float duration = 3.0f;
        float t = 0.0f;
        while (t < duration)
        {
            // stop traveling if the rocket is already there
            if(transform.position == endPos)
                break;
            //
            endPos = DestinationTile.parentPlanet.transform.TransformPoint(DestinationTile.transform.localPosition + new Vector3(0, 2.0f, 0));
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            // rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, endPos - startPos), t);

            yield return new WaitForEndOfFrame();
        }

        t = 0.0f;
        duration = 2.0f;
        transform.SetParent(DestinationTile.transform);
        startPos = transform.localPosition;
        endPos = new Vector3(0, 1.0f, 0);
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t / 2.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = endPos;

        StartCoroutine(Land());
    }
    IEnumerator FirstLandingCutscene(Tile tile)
    {
        CurrentTile = tile;
        Traveling = true;

        Thrusters.Play();

        Vector3 startPos = new Vector3(0, 1.0f, 0);
        Vector3 endPos = new Vector3(0, tile.ExtrudedHeight * 2, 0);

        float t = 0.0f;
        while (t <= 2.0f)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t / 2.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = endPos;

        Thrusters.Stop();

        Traveling = false;
        GameManager.instance.OnRocketLanded?.Invoke(CurrentTile.parentPlanet);
        TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnRocketLanded);
        GameManager.instance.OnGameStart?.Invoke();
        GameStateController.instance.UnboardBears();
    }
    #endregion
}

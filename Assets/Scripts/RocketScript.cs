using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class RocketScript : MonoBehaviour {
    public static RocketScript instance;

    public struct Stats
    {
        public float start_time; //survival_time;
        public int num_tamed_bears;
        public HashSet<Hexsphere> planets_traveled;
        public int num_fish_obtained;
        public int num_oil_obtained;
    }
    public Stats MyStats;

    public GameObject fakeBear;

    public Tile CurrentTile;
    public Tile DestinationTile;
    public ParticleSystem Thrusters;

    public HashSet<GameObject> BearsOwned;
    public HashSet<GameObject> BearsBoarded;
    public int NumBears => BearsOwned.Count;
    public int NumOil = 0;
    public int NumFish = 0;
    public bool CanLaunch => (BearsBoarded.Count >= BearThreshold) && (NumOil >= (OilThreshold));
    public bool Traveling = false;
    public bool Sank = false;



    [Tooltip("How many fish per bear")]
    public static int FishPerBear = 2;
    [Tooltip("Minimum bears for takeoff")]
    public static int BearThreshold = 1;
    [Tooltip("Minimum oil for takeoff")]
    public static int OilThreshold = 5;
    [Tooltip("How many fish to board the ship")]
    public static int AdmissionPrice = 1;
    [Tooltip("Starting Amount of Bears")]
    public static int StartingBears = 3;
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
        MyStats = new Stats();
        MyStats.planets_traveled = new HashSet<Hexsphere>();
        MyStats.start_time = Time.time;
        MyStats.num_tamed_bears = StartingBears;
    }

    // Start is called before the first frame update
    void Start()
    {
        BearsOwned = new HashSet<GameObject>();
        for (int i = 0; i < StartingBears; i++)
        {
            GameObject bear = Instantiate(GameStateController.instance.BearPrefabs[Random.Range(0,2)]);
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
        Interlocked.Add(ref NumOil, amt);
        Interlocked.Add(ref MyStats.num_oil_obtained, 1);
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.RocketIcon, "+" + amt.ToString() + " Oil", gameObject.transform.position);
        UpdateSliders();
    }

    public void AddFish(int amt) {
        if (firstFishObtained)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstFishObtained);
            firstFishObtained = false;
        }
        Interlocked.Add(ref NumFish, amt);
        Interlocked.Add(ref MyStats.num_fish_obtained, 1);
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.RocketIcon, "+" + amt.ToString() + " Fish", gameObject.transform.position);
        UpdateSliders();
    }

    public bool PayForBear(GameObject bear)
    {
        if (NumFish >= FishPerBear)
        {
            //TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstBearObtained);
            Interlocked.Add(ref NumFish, -FishPerBear);
            bear.GetComponent<UntamedBear>().PaidFor = true;
            GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.RocketIcon, "-" + FishPerBear.ToString() + " Fish", gameObject.transform.position);
            UpdateSliders();
            return true;
        }
        else
        {
            GameplayCanvas.instance.PushMessage("You need at least " + FishPerBear + " fish to recruit a bear!", 3);
            return false;
        }
    }
    public void RecruitBear(GameObject bear) {
        if (!bear.GetComponent<UntamedBear>().PaidFor) return;
        GameObject pfb = (bear.GetComponent<UntamedBear>().GetBearType() == Bear.BearType.Brown) 
                            ? GameStateController.instance.BearPrefabs[0] // 0 is brown
                            : GameStateController.instance.BearPrefabs[1]; // 1 is polar
        GameObject temp = Instantiate(pfb);
        temp.SetActive(false);
        BearsOwned.Add(temp);
        BearsBoarded.Add(temp);
        Interlocked.Add(ref MyStats.num_tamed_bears, 1);
        UpdateSliders();
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, "+1 Bear", gameObject.transform.position);
        Destroy(bear);
    }
    public bool BoardBear(GameObject bear, bool free = false)
    {
        // free boarding for bears that can't get to the ship because of occluded neighbors
        if (free)
        {
            GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, "+1 Bear Boarded", gameObject.transform.position);
            BearsBoarded.Add(bear);
            UpdateSliders();
            return true;
        }
        else if (NumFish >= AdmissionPrice)
        {
            GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.FishIcon, "-" + AdmissionPrice.ToString() + " Fish", gameObject.transform.position);
            GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.BearIcon, "+1 Bear Boarded", gameObject.transform.position);

            Interlocked.Add(ref NumFish, -AdmissionPrice);
            BearsBoarded.Add(bear);
            UpdateSliders();
            return true;
        }
        else
        {
            GameplayCanvas.instance.PushMessage("Out of fish! Ship is at capacity!", 0.5f);
            return false;
        }
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
        GameObject[] arr = new GameObject[BearsOwned.Count];
        BearsOwned.CopyTo(arr);
        HashSet<GameObject> BearsOwnedCopy = new HashSet<GameObject>(arr);
        BearsOwned.IntersectWith(BearsBoarded);
        BearsOwnedCopy.ExceptWith(BearsBoarded);
        foreach (GameObject obj in BearsOwnedCopy)
        {
            Destroy(obj);
        }
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
        // if we're already moving, bruh
        if (Traveling) return;
        if (!CanLaunch)
        {
            string res = "";
            if (BearsBoarded.Count < BearThreshold)
            {
                 res += "at least "+BearThreshold+" Bears";
            }
            if (NumOil < OilThreshold)
            {
                if (!res.Equals("")) res += " and ";
                res += OilThreshold + " Oil!";
            }
            GameplayCanvas.instance.PushMessage("Not enough fuel to launch! Need " + res, 1.0f);
            return;
        }
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
            if (!t.Occupied && t.BiomeType != Hexsphere.BiomeType.Water)
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
        Interlocked.Add(ref NumOil, -(OilThreshold));
        GameplayCanvas.instance.SpawnPopup(GameplayCanvas.instance.OilIcon, "- " + (OilThreshold + BearsBoarded.Count), gameObject.transform.position);
        UpdateSliders();

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
        UpdateSliders();

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
        MyStats.planets_traveled.Add(DestinationTile.parentPlanet);
        GameManager.instance.OnRocketLanded?.Invoke(DestinationTile.parentPlanet);
        GameStateController.instance.UnboardBears();
        DestinationTile = null;
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
        if (Traveling) yield break;
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

        MyStats.planets_traveled.Add(tile.parentPlanet);
    }
    #endregion
}

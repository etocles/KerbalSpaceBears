using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour {
    public GameObject fakeBear;

    public Tile CurrentTile;
    public Tile DestinationTile;
    public ParticleSystem Thrusters;

    public HashSet<GameObject> BearsOwned;
    public HashSet<GameObject> BearsBoarded;
    public int NumBears => BearsOwned.Count;
    public float NumOil = 0;
    public int NumFish = 0;
    public bool CanLaunch => (NumBears >= BearThreshold) && (NumOil >= OilThreshold);
    public bool Traveling = false;
    public bool Sank = false;



    [Tooltip("How many fish per bear")]
    public static int FishPerBear = 2;
    [Tooltip("Minimum bears for takeoff")]
    public static int BearThreshold = 2;
    [Tooltip("Minimum oil for takeoff")]
    public static float OilThreshold = 0.6f;
    [Tooltip("Starting Amount of Bears")]
    public static int StartingBears = 3;
    [Tooltip("Starting Amount of Fish")]
    public static int StartingFish = 3;
    [SerializeField]
    public GameObject SpaceBearPrefab;


    private bool firstFishObtained = true;
    private bool firstOilObtained = true;

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
    public void AddOil(float amt) {
        if (firstOilObtained)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstOilObtained);
            firstFishObtained = false;
        }
        NumOil += amt; // TODO: Report to canvas
    }

    public void AddFish(int amt) {
        if (firstFishObtained)
        {
            TutorialManager.instance.InitiateTutorialEvent(TutorialEvent.OnFirstFishObtained);
            firstFishObtained = false;
        }
        NumFish += amt; // TODO: Report to canvas
    }

    public bool PayForBear(GameObject bear)
    {
        if (NumFish >= FishPerBear)
        {
            NumFish -= FishPerBear;
            bear.GetComponent<UntamedBear>().PaidFor = true;
            return true;
        }
        else
        {
            return false;
        }
    }
    public void RecruitBear(GameObject bear) {
        if (!bear.GetComponent<UntamedBear>().PaidFor) return;
        GameObject pfb = (bear.GetComponent<UntamedBear>().GetBearType() == Bear.BearType.Brown) 
                            ? GameStateController.instance.BearPrefabs[0] 
                            : GameStateController.instance.BearPrefabs[1];
        GameObject temp = Instantiate(pfb);
        BearsOwned.Add(temp);
        BearsBoarded.Add(temp);
        // TODO: Report to canvas

        // if there's still bears on board, that means the ship is full.
        // coroutine is still emptying them out, so we don't have to
        // if there's 0, do a manual refresh
        if (BearsBoarded.Count == 0) {
            bear = UnboardBear();
            GameStateController.instance.DepositBear(bear, GetUnOccupiedTile());
        }
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
        DestinationTile = GameManager.instance.SelectedTile; 
        StartCoroutine(Launch()); 
    }

    public void FirstLanding(Tile tile) => StartCoroutine(FirstLandingCutscene(tile));

    public Tile GetUnOccupiedTile()
    {
        foreach (Tile t in CurrentTile.neighborTiles)
        {
            if (!t.Occupied && t.GroupID != CurrentTile.parentPlanet.FindBiomeIDByType(Hexsphere.BiomeType.Water))
                return t;
        }
        return null;
    }
    #region Coroutines
    IEnumerator TipOver()
    {
        Vector3 startRot = transform.localEulerAngles;
        Vector3 endRot = transform.localEulerAngles;
        endRot.y = 100.0f;

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
        while (t <= 0.3f)
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
        Vector3 endPos = DestinationTile.parentPlanet.transform.TransformPoint(DestinationTile.transform.localPosition + new Vector3(0, 1.0f, 0));
        float t = 0.0f;
        while (t <= 3.0f)
        {
            endPos = DestinationTile.parentPlanet.transform.TransformPoint(DestinationTile.transform.localPosition + new Vector3(0, 1.0f, 0));
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t / 3.0f);
            yield return new WaitForEndOfFrame();
        }

        t = 0.0f;
        transform.SetParent(DestinationTile.transform);
        startPos = transform.localPosition;
        endPos = new Vector3(0, 1.0f, 0);
        while (t <= 2.0f)
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

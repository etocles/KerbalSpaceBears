using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour

{
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
        // TODO: Subscribe to canvas's  context menu to begin launch
        //StartLaunch();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Traveling)
        {
            TrySink();
        }
    }

    #region Resource Functions
    public void AddOil(float amt) {
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

    public void RecruitBear(GameObject bear) {
        if (NumFish >= FishPerBear)
        {
            NumFish -= FishPerBear;
            BearsOwned.Add(bear);
            // TODO: Report to canvas
        }
    }
    public void BoardBear(GameObject bear)
    {
        BearsBoarded.Add(bear);
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
        BearsOwned.IntersectWith(BearsBoarded);
        //bears owned gets changed but bears boarded doesn't
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
    void StartLaunch() => StartCoroutine(Launch());
    public void FirstLanding(Tile tile) => StartCoroutine(FirstLandingCutscene(tile));

    #region Coroutines
    IEnumerator TipOver()
    {
        Vector3 startRot = transform.localEulerAngles;
        Vector3 endRot = transform.localEulerAngles;
        endRot.y = 60.0f;

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

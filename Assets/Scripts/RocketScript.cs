using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour

{
    public Tile CurrentTile;
    public Tile DestinationTile;
    public ParticleSystem Thrusters;

    public int NumBears = 0;
    public float NumOil = 0;
    public int BearThreshold = 2;
    public float OilThreshold = 0.6f;
    public bool CanLaunch => (NumBears >= BearThreshold) && (NumOil >= OilThreshold);
    public bool Traveling = false;
    public bool Sank = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // for testing, do movement sequence
        if (Input.GetKeyDown(KeyCode.Space) 
            && !Traveling 
            && DestinationTile.parentPlanet != CurrentTile.parentPlanet)
        {
            StartLaunch();
        }
        if (!Traveling)
        {
            TrySink();
        }
    }

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
        GameManager.instance.OnGameStart?.Invoke();
    }

}

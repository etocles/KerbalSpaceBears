using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    [System.Serializable]
    public class PlanetDefinition
    {
        public float orbitalRadius; // determines heat;
        public int polarBearCount = 5;
        [Range(1, 5)]
        public int iceWeight = 1;
        [Range(1, 5)]
        public int waterWeight = 1;
        [Range(1, 5)]
        public int oilWeight = 1;
        [Range(1, 5)]
        public int fishWeight = 1;
        [Range(0.5f, 2.5f)]
        public float planetScale;
    }
    public List<GameObject> BearPrefabs;
    public List<GameObject> UntamedBearPrefabs;

    public List<PlanetDefinition> planetDefinitions = new List<PlanetDefinition>();
    [HideInInspector] public List<Hexsphere> planets = new List<Hexsphere>();
    public GameObject HexspherePrefab;
    [HideInInspector] public Hexsphere ActiveHexsphere;
    public static GameStateController instance;

    private List<GameObject> spawnedPolarBears = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateSolarSystem();
        SetTarget(planets[0]);
        GameplayCanvas.instance.OnRecallAllBears.AddListener(BoardBears);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = MouseHit();
            if (hit.collider != null && hit.collider.gameObject.layer == 6) // Planet layer
            {
                SetTarget(hit.collider.GetComponentInParent<Hexsphere>());
            }
        }
    }

    public void GenerateSolarSystem()
    {
        foreach (PlanetDefinition definition in planetDefinitions)
        {
            GameObject planetPivot = new GameObject();
            planetPivot.name = "Generated Planet";
            planetPivot.transform.SetParent(transform);
            planetPivot.transform.localPosition = new Vector3(0, 0, 0);

            GameObject spawnedPlanet = Instantiate(HexspherePrefab);
            Hexsphere sphereCtrl = spawnedPlanet.GetComponent<Hexsphere>();
            planets.Add(sphereCtrl);
            sphereCtrl.GroupBiomes[1].weight = definition.iceWeight;
            sphereCtrl.GroupBiomes[2].weight = definition.waterWeight;
            sphereCtrl.GroupBiomes[3].weight = definition.fishWeight;
            sphereCtrl.GroupBiomes[4].weight = definition.oilWeight;
            sphereCtrl.planetScale = definition.planetScale;
            sphereCtrl.BuildPlanet();
            sphereCtrl.GenerateRandom();
            spawnedPlanet.transform.SetParent(planetPivot.transform);
            spawnedPlanet.transform.localPosition = new Vector3(definition.orbitalRadius, 0, 0);
            planetPivot.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            Rotate orbitRotCtrl = planetPivot.AddComponent<Rotate>();
            orbitRotCtrl.speed -= orbitRotCtrl.speed * (definition.orbitalRadius / 200);
            if (Random.value > 0.5f) orbitRotCtrl.speed *= -1f;
            Rotate planetRotCtrl = spawnedPlanet.AddComponent<Rotate>();
            planetRotCtrl.speed = 2;
            SpawnBears(sphereCtrl, definition.polarBearCount);

        }
    }

    public RaycastHit MouseHit()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 250);
        return hit;
    }

    public void RemoveTarget()
    {
        ActiveHexsphere.transform.Find("Atmosphere").GetComponent<SphereCollider>().enabled = true;
    }
    public void SetTarget(Hexsphere sphere)
    {
        if (ActiveHexsphere != null && sphere != ActiveHexsphere)
        {
            RemoveTarget();
        }
        ActiveHexsphere = sphere;
        CameraBoom.instance.SwitchPlanets(sphere);
        ActiveHexsphere.transform.Find("Atmosphere").GetComponent<SphereCollider>().enabled = false;
    }

    public void SpawnBears(Hexsphere planet, int count)
    {
        HashSet<int> randomTiles = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            int num = Random.Range(0, planet.GetTilesByBiome(Hexsphere.BiomeType.Ice).Count);
            randomTiles.Add(num);
        }
        foreach (int spawnLoc in randomTiles)
        {
            PlaceUntamedBear(planet.GetTilesByBiome(Hexsphere.BiomeType.Ice)[spawnLoc]);
        }
    }

    public GameObject PlaceUntamedBear(Tile location) {

        GameObject spawnedBear = Instantiate(UntamedBearPrefabs[(int)Random.Range(0.0f, UntamedBearPrefabs.Count)]);
        spawnedBear.GetComponent<MobileUnit>().parentPlanet = location.parentPlanet;
        spawnedBear.GetComponent<MobileUnit>().currentTile = location;
        location.placeObject(spawnedBear);
        location.activeBear = ActiveBear.Untamed;
        location.Occupied = true;
        spawnedPolarBears.Add(spawnedBear);
        return spawnedBear;
    }

    public GameObject PlaceBear(Tile location)
    {

        GameObject spawnedBear = Instantiate(BearPrefabs[(int)Random.Range(0.0f, BearPrefabs.Count)]);
        spawnedBear.GetComponent<MobileUnit>().parentPlanet = location.parentPlanet;
        spawnedBear.GetComponent<MobileUnit>().currentTile = location;
        location.placeObject(spawnedBear);
        location.activeBear = ActiveBear.Tamed;
        location.Occupied = true;
        spawnedPolarBears.Add(spawnedBear);
        return spawnedBear;
    }

    public void DepositBear(GameObject bear, Tile location)
    {
        bear.SetActive(true);
        bear.GetComponent<MobileUnit>().parentPlanet = location.parentPlanet;
        bear.GetComponent<MobileUnit>().currentTile = location;
        RocketScript rocket = GameManager.instance.Rocket.GetComponent<RocketScript>();
        bear.GetComponent<PolarBearController>().SetShipTile(rocket.CurrentTile);
        location.placeObject(bear);
        location.Occupied = true;
        location.activeBear = ActiveBear.Tamed;
        if (location.BiomeType == Hexsphere.BiomeType.Fish || location.BiomeType == Hexsphere.BiomeType.Oil)
        {
            bear.GetComponent<PolarBearController>().StartAutoHarvest(location);
        }
    }

    public void BoardBears()
    {
        RocketScript rocket = GameManager.instance.Rocket.GetComponent<RocketScript>();
        // request return and let PolarBear controller take care of it
        foreach (GameObject bear in rocket.BearsOwned)
        {
            bear.GetComponent<PolarBearController>().ChangeState(PolarBearController.BearState.SHIP);
            bear.GetComponent<PolarBearController>().SetShipTile(rocket.CurrentTile);
            StartCoroutine(bear.GetComponent<PolarBearController>().ReturnToShip());
        }
    }
    public void UnboardBears()
    {
        StartCoroutine(UnBoarding());
    }

    IEnumerator UnBoarding()
    {
        RocketScript rocket = GameManager.instance.Rocket.GetComponent<RocketScript>();
        while (rocket.BearsBoarded.Count > 0)
        {
            Tile tryTile = rocket.GetUnOccupiedTile();
            if (tryTile == null)
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            GameObject bear = rocket.UnboardBear();
            DepositBear(bear, tryTile);
        }
    }

}

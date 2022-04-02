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
        public int oilCount = 5;
        [Range(1,5)]
        public int waterWeight = 1;
        [Range(0.5f, 2.5f)]
        public float planetScale;
    }
    public List<PlanetDefinition> planetDefinitions = new List<PlanetDefinition>();
    [HideInInspector] public List<Hexsphere> planets = new List<Hexsphere>();
    public GameObject HexspherePrefab;
    [HideInInspector] public Hexsphere ActiveHexsphere;
    // Start is called before the first frame update
    void Start()
    {
        GenerateSolarSystem();
        SetTarget(planets[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = MouseHit();
            if(hit.collider != null && hit.collider.gameObject.layer == 6) // Planet layer
            {
                SetTarget(hit.collider.GetComponentInParent<Hexsphere>());
            }
        }
    }

    public void GenerateSolarSystem()
    {
        foreach(PlanetDefinition definition in planetDefinitions)
        {
            GameObject planetPivot = new GameObject();
            planetPivot.name = "Generated Planet";
            planetPivot.transform.SetParent(transform);
            planetPivot.transform.localPosition = new Vector3(0, 0, 0);

            GameObject spawnedPlanet = Instantiate(HexspherePrefab);
            Hexsphere sphereCtrl = spawnedPlanet.GetComponent<Hexsphere>();
            planets.Add(sphereCtrl);
            sphereCtrl.GroupBiomes[2].weight = definition.waterWeight;
            sphereCtrl.planetScale = definition.planetScale;
            sphereCtrl.BuildPlanet();
            sphereCtrl.GenerateRandom();
            spawnedPlanet.transform.SetParent(planetPivot.transform);
            spawnedPlanet.transform.localPosition = new Vector3(definition.orbitalRadius, 0, 0);
            planetPivot.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            Rotate rotCtrl = planetPivot.AddComponent<Rotate>();
            rotCtrl.speed += rotCtrl.speed * (definition.orbitalRadius / 200);
            if (Random.value > 0.5f) rotCtrl.speed *= -1f;
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
        if(ActiveHexsphere != null && sphere != ActiveHexsphere)
        {
            RemoveTarget();
        }
        ActiveHexsphere = sphere;
        CameraBoom.instance.SwitchPlanets(sphere);
        ActiveHexsphere.transform.Find("Atmosphere").GetComponent<SphereCollider>().enabled = false;
    }
}

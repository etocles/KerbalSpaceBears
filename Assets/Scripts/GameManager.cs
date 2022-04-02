using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Hexsphere ActivePlanet;
    public GameObject PolarBearPrefab;
    [HideInInspector] public Tile SelectedTile;

    private List<GameObject> spawnedPolarBears = new List<GameObject>();

    private void OnValidate()
    {
        if (instance == null) instance = this;
    }
    private void Awake()
    {
        if(instance == null) instance = this;
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
            int num = Random.Range(0, ActivePlanet.IceTiles.Count);
            randomTiles.Add(num);
        }
        foreach(int spawnLoc in randomTiles)
        {
            PlacePolarBear(ActivePlanet.IceTiles[spawnLoc]);
        }
    }
    public GameObject PlacePolarBear(Tile location)
    {
        GameObject spawnedBear = Instantiate(PolarBearPrefab);
        location.placeObject(spawnedBear);
        spawnedPolarBears.Add(spawnedBear);
        return spawnedBear;
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
    }
    public void DeselectTile()
    {
        SelectedTile.SetHighlight(0.0f);
        SelectedTile.Selected = false;
        SelectedTile = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

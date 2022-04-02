using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Xml.Serialization.Advanced;
using UnityEngine.EventSystems;
public enum TileDisplayOptions
{
    None,
    GroupID,
    NavWeight,
    Navigable
}

public enum Sides
{
	ONE, TWO, THREE, FOUR, FIVE, SIX, NONE
}


[Serializable]
public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{

	public bool Occupied;
	public bool Selected;
	public static float planetScale;
	private static int ID = 0;
    public static Action<Tile> OnTileClickedAction;
	[Header("Their Settings")]
	[Tooltip("The instance of the hexsphere which constructed this tile")]
	public Hexsphere parentPlanet;
	
	public List<Tile> neighborTiles = new List<Tile>();
	
	//Tile Attributes
	[Tooltip("Whether or not navigation will consider this tile as a valid to move over")]
	public bool navigable = true;
	[Tooltip("The cost of moving across this tile in terms of pathfinding weight.  Pathfinding will prioritize the lowest cost path.")]
	[Range(1, 100)]
	public int pathCost = 1;

	private MaterialPropertyBlock propBlock;
	// The center of this tile when initially generated.  Does not account for extrusion.
	public Vector3 center
    {
		get{ return tileRenderer.bounds.center; }
	}

    // The current center of the tiles face accounting for extrusion.
    public Vector3 FaceCenter
    {
        get
        {
            float heightMult = IsInverted ? -1f : 1f;
            return transform.position + transform.up * ExtrudedHeight * heightMult * parentPlanet.planetScale;
        }
    }

	//The position of this tile as reported by the renderer in world space.  More strict than the above center.
	public Vector3 centerRenderer
    {
		get{ return tileRenderer.bounds.center; }
	}

    [HideInInspector]
	public int GroupID;
	public Hexsphere.BiomeType BiomeType;

    [HideInInspector]
	public int id;

	[HideInInspector]
	public Renderer tileRenderer;

    [HideInInspector]
    public float ExtrudedHeight;

    public bool isHexagon;

    [HideInInspector]
    public bool IsInverted;

    public List<GameObject> PlacedObjects = new List<GameObject>();

    [HideInInspector]
    public TileDisplayOptions InfoDisplayOption;

	public Dictionary<Tile, Sides> indexAssociation;
	//[HideInInspector]
	public List<Tile> AKeys;
	//[HideInInspector]
	public List<Sides> AValues;

	//Used to specify which tile is currently selected so that any tile can query the selected tile or assign themselves as selected.
	private static Tile selectedTile;
	//The center of the tile in worldspace as assigned by the hexsphere during generation.  Not affected by the scale of the planet.
    [SerializeField, HideInInspector]
	private bool hasBeenExtruded;

    [SerializeField, HideInInspector]
    private Material TileMaterial;

    private Color HilightColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [SerializeField, HideInInspector]
    private Vector3[] Vertices;
    [SerializeField, HideInInspector]
    private Vector2[] UVs;
    [SerializeField, HideInInspector]
    private int[] Triangles;

	
    
	private void Awake()
    {
		tileRenderer = GetComponent<Renderer> ();
		propBlock = new MaterialPropertyBlock();

	}
    private void Start()
    {
		indexAssociation = new Dictionary<Tile, Sides>();
        for(int i = 0; i < AKeys.Count; ++i)
        {
			indexAssociation.Add(AKeys[i], AValues[i]);
        }
    }

    public void Initialize()
    {
		indexAssociation = new Dictionary<Tile, Sides>();
		tileRenderer = GetComponent<Renderer> ();
		id = ID;
		ID++;
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		if(Selected == false) SetHighlight(0.33f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(Selected == false) SetHighlight(0.0f);

	}
	public void OnPointerDown(PointerEventData eventData)
	{
		if(eventData.button == PointerEventData.InputButton.Left)
        {
			GameManager.instance.SelectTile(this);
			OnTileClickedAction?.Invoke(this);
		}
		
	}
	public void SetHighlight(float intensity)
    {
		tileRenderer.GetPropertyBlock(propBlock);
		tileRenderer.material.EnableKeyword("_EMISSION");
		propBlock.SetColor("_EmissionColor", new Color(intensity, intensity, intensity));
		tileRenderer.SetPropertyBlock(propBlock);
	}
	
	

	/// <summary>
	/// Just a simple demo function that allows you to click on two tiles and draw the shortest path between them.
	/// </summary>
	public void pathfindingDrawDemo()
    {
		if (selectedTile == null) {
			selectedTile = this;
		}
		else if(selectedTile != this){
			Stack<Tile> path = new Stack<Tile>();
			if(parentPlanet.navManager.findPath(selectedTile, this, out path)){
				parentPlanet.navManager.drawPath(path);
				selectedTile = null;
			}
		}
	}

	public void placeObject(GameObject obj)
    {
		obj.transform.position = FaceCenter;
		obj.transform.up = transform.up;
        obj.transform.SetParent(transform);
        PlacedObjects.Add(obj);
	}

	public void DeleteLastPlacedObject()
    {
        if(PlacedObjects.Count > 0)
        {
            DestroyImmediate(PlacedObjects[PlacedObjects.Count - 1]);
            PlacedObjects.RemoveAt(PlacedObjects.Count - 1);
        }
    }

    public void DeletePlacedObjects()
    {
        for(int i = 0; i < PlacedObjects.Count; i++)
        {
            if(PlacedObjects[i] != null)
            {
                DestroyImmediate(PlacedObjects[i]);
            }
        }

        PlacedObjects.Clear();
    }

	public HashSet<Tile> GetTilesWithinRadius(int radius)
    {
		if(radius == 0)
        {
			HashSet<Tile> tilesInRange = new HashSet<Tile>();
			return tilesInRange;
        }
		else
        {
			HashSet<Tile> tilesInRange = new HashSet<Tile>();

			List<Tile> tilesToCheck = new List<Tile>();
			tilesToCheck.Add(this);
			bool checkForMoreNeighbors = true;
			int i = 0;
			while (checkForMoreNeighbors)
			{
				List<Tile> neighborTiles = new List<Tile>();
				foreach (Tile tile in tilesToCheck)
				{
					foreach (Tile neighbor in tile.neighborTiles)
					{
						if (neighborTiles.Contains(neighbor) == false)
						{
							neighborTiles.Add(neighbor);
							tilesInRange.Add(neighbor);
						}
					}
				}
				i++;
				if (i >= radius)
				{
					checkForMoreNeighbors = false;
				}
				else
				{
					tilesToCheck = neighborTiles;
				}
			}
			tilesInRange.Add(this);
			return tilesInRange;
		}
    }

	public HashSet<Tile> GetNavigableTilesWithinRadius(int movement)
	{
		if (movement == 0)
		{
			HashSet<Tile> tilesInRange = new HashSet<Tile>();
			return tilesInRange;
		}
		else
		{
			HashSet<Tile> tilesInRange = new HashSet<Tile>();

			List<Tile> tilesToCheck = new List<Tile>();
			tilesToCheck.Add(this);
			bool checkForMoreNeighbors = true;
			int i = 0;
			while (checkForMoreNeighbors)
			{
				List<Tile> neighborTiles = new List<Tile>();
				foreach (Tile tile in tilesToCheck)
				{
					foreach (Tile neighbor in tile.neighborTiles)
					{
						Hexsphere.Biome tileBiome = parentPlanet.FindBiomeByID(neighbor.GroupID);
						if (neighborTiles.Contains(neighbor) == false && tileBiome.navigable == true && (movement - i) >= tileBiome.movementCost)
						{
							neighborTiles.Add(neighbor);
							tilesInRange.Add(neighbor);
						}
					}
				}
				i++;
				if (i >= movement)
				{
					checkForMoreNeighbors = false;
				}
				else
				{
					tilesToCheck = new List<Tile>(neighborTiles);
				}
			}
			tilesInRange.Add(this);
			return tilesInRange;
		}
	}
	public void SetExtrusionHeight(float height)
    {
        float delta = height - ExtrudedHeight;
        Extrude(delta);
    }

	public void Extrude(float heightDelta)
    {
        ExtrudedHeight += heightDelta;
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		Vector3[] verts = mesh.vertices;
		//Check if this tile has already been extruded
		if(hasBeenExtruded)
        {
			int sides = isHexagon ? 6 : 5;
			//Apply extrusion heights
			for(int i = 0; i < sides; i++)
            {
				Vector3 worldV = (transform.TransformPoint (verts [i]) - parentPlanet.transform.position);
				worldV += heightDelta * worldV.normalized * parentPlanet.planetScale;
				verts [i] = transform.InverseTransformPoint (worldV + parentPlanet.transform.position);
			}
			for (int i = sides + 2; i < sides + sides * 4; i += 4)
            {
				Vector3 worldV = (transform.TransformPoint (verts [i]) - parentPlanet.transform.position);
				worldV += heightDelta * worldV.normalized * parentPlanet.planetScale;
				verts [i] = transform.InverseTransformPoint (worldV + parentPlanet.transform.position);

				worldV = (transform.TransformPoint (verts [i + 1]) - parentPlanet.transform.position);
				worldV += heightDelta * worldV.normalized * parentPlanet.planetScale;
				verts [i + 1] = transform.InverseTransformPoint (worldV + parentPlanet.transform.position);
			}

			mesh.vertices = verts;

            // If this has a mesh collider, update the mesh
            MeshCollider mCollider = GetComponent<MeshCollider>();
            if (mCollider != null)
            {
                GetComponent<MeshCollider>().sharedMesh = mesh;
            }
			
			GetComponent<MeshFilter> ().sharedMesh = mesh;
			return;
		}

		//Sort vertices clockwise
		Array.Sort(verts, new ClockwiseComparer (transform.InverseTransformPoint (center)));
		List<int> tris = new List<int> (mesh.triangles);
        //List<Vector3> normals = new List<Vector3> (mesh.normals);

		//Duplicate the existing vertices
		List<Vector3> faceVerts = new List<Vector3>(verts);
		//Translate duplicated verts along local up
		for(int i = 0; i < faceVerts.Count; i++)
        {
			Vector3 worldV = (transform.TransformPoint (faceVerts [i]) - parentPlanet.transform.position);
			worldV += heightDelta * worldV.normalized * parentPlanet.planetScale;
			faceVerts [i] = transform.InverseTransformPoint (worldV + parentPlanet.transform.position);
		}
		//Set triangles for extruded face
		tris [0] = 0;
		tris [1] = 1;
		tris [2] = 2;

		tris [3] = 0;
		tris [4] = 2;
		tris [5] = 3;

		tris [6] = 0;
		tris [7] = 3;
		tris [8] = 4;

		//Only set the last triangle if this is a hexagon
		if (verts.Length == 6)
        {
			tris [9] = 0;
			tris [10] = 4;
			tris [11] = 5;
		}
		int t = 0;
		//Create side triangles
		for(int i = 0; i < verts.Length - 1; i++, t += 4)
        {
			faceVerts.Add (verts [i]);
			faceVerts.Add (verts [i + 1]);

			faceVerts.Add (faceVerts [i]);
			faceVerts.Add (faceVerts [i + 1]);

			tris.Add (t + verts.Length);
			tris.Add (t + verts.Length + 1);
			tris.Add (t + verts.Length + 2);

			tris.Add (t + verts.Length + 1);
			tris.Add (t + verts.Length + 3);
			tris.Add (t + verts.Length + 2);
		}
		//Manually create last two triangles
		faceVerts.Add(verts[verts.Length - 1]);
		faceVerts.Add(verts[0]);

		faceVerts.Add(faceVerts[verts.Length - 1]);
		faceVerts.Add(faceVerts[0]);

		tris.Add (faceVerts.Count - 4);
		tris.Add (faceVerts.Count - 3);
		tris.Add (faceVerts.Count - 2);

		tris.Add (faceVerts.Count - 3);
		tris.Add (faceVerts.Count - 1);
		tris.Add (faceVerts.Count - 2);


		mesh.vertices = faceVerts.ToArray ();
		mesh.triangles = tris.ToArray ();
		mesh.RecalculateNormals ();
		//Reassign UVs
		mesh.uv = isHexagon ? generateHexUvs() : generatePentUvs();

		//Assign meshes to Mesh Collider and Mesh Filter
        if(GetComponent<MeshCollider>() != null)
        {
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
		
		GetComponent<MeshFilter> ().sharedMesh = mesh;
		hasBeenExtruded = true;
        //Assign Tile Material
        //Color color = tileRenderer.sharedMaterial.color;
		//tileRenderer.sharedMaterial = isHexagon ? parentPlanet.hexMat_extruded : parentPlanet.pentMat_extruded;
        //SetColor(color);
	}

	public Vector2[] generateHexUvs()
    {
		Vector2[] uvs = new Vector2[30];
		uvs [0] = new Vector2 (0.293f, 0.798f);
		uvs [1] = new Vector2 (0.397f, 0.977f);
		uvs [2] = new Vector2 (0.604f, 0.977f);
		uvs [3] = new Vector2 (0.707f, 0.798f);
		uvs [4] = new Vector2 (0.604f, 0.619f);
		uvs [5] = new Vector2 (0.397f, 0.619f);

		float h = 6f;
		float y = 0.6f;
		for (int i = 6; i < 28; i += 4)
        {
			uvs [i] = new Vector2 (h / 6f, 0f);
			uvs [i + 1] = new Vector2 ((h - 1) / 6f, 0f);

			uvs [i + 2] = new Vector2 (h / 6f, y);
			uvs [i + 3] = new Vector2 ((h - 1)/ 6f, y);
			h--;
		}
		return uvs;
	}

	public Vector2[] generatePentUvs()
    {
		Vector2[] uvs = new Vector2[25];
		uvs [0] = new Vector2 (0.389f, 0.97f);
		uvs [1] = new Vector2 (0.611f, 0.97f);
		uvs [2] = new Vector2 (0.68f, 0.758f);
		uvs [3] = new Vector2 (0.5f, 0.627f);
		uvs [4] = new Vector2 (0.32f, 0.758f);

		float h = 5f;
		float y = 0.6f;
		for (int i = 5; i < 22; i += 4)
        {
			uvs [i] = new Vector2 (h / 5f, 0f);
			uvs [i + 1] = new Vector2 ((h - 1) / 5f, 0f);

			uvs [i + 2] = new Vector2 (h / 5f, y);
			uvs [i + 3] = new Vector2 ((h - 1)/ 5f, y);
			h--;
		}
		return uvs;
	}

	public void SetGroupID(int groupId)
    {
		GroupID = groupId;
		Hexsphere.Biome thisBiome = parentPlanet.FindBiomeByID(groupId);
		BiomeType = thisBiome.type;
		// set color
        if (GroupID < parentPlanet.GroupBiomes.Length)
        {
            tileRenderer.sharedMaterial = thisBiome.material;
            TileMaterial = tileRenderer.sharedMaterial;
        }
		
		navigable = thisBiome.navigable;
		
		
	}

    public void SetColor(Color col)
    {
        Material tempMaterial = new Material(GetComponent<Renderer>().sharedMaterial);
        tempMaterial.color = col;
        tileRenderer.sharedMaterial = tempMaterial;
    }

    public void SetMaterial(Material mat)
    {
        TileMaterial = mat;
        tileRenderer.sharedMaterial = mat;
    }

    public void SetHighlight(bool hilighted)
    {
        if(hilighted)
        {
            
            Material tempMaterial = new Material(TileMaterial);
            tempMaterial.color = HilightColor + TileMaterial.color;
            tileRenderer.sharedMaterial = tempMaterial;
        }
        else
        {
            tileRenderer.sharedMaterial = TileMaterial;
        }
    }

    #region TileDirectionFunctions

	public void PrintDirectionData()
    {
		if (indexAssociation == null) Debug.LogError("ERROR: Tile Association Dictionary not initialized.");
        else
        {
			foreach (KeyValuePair<Tile, Sides> kvp in indexAssociation)
			{
				Debug.Log($"[TILE {this.id}] {kvp.Value} Has ID => {kvp.Key.id}");
			}
		}

    }

    /// <summary>
    /// Returns the index among this Tile's neighbors of the first Tile
	/// found shared between this and some other tile. -1 on failure.
    /// </summary> 
    public int GetSharedNeighborIndex(Tile other, HashSet<Tile> validNeigbors)
	{
		foreach(Tile neighbor in other.neighborTiles)
		{
			if (!validNeigbors.Contains(neighbor)) continue;

			int index = this.neighborTiles.FindIndex(a => a.Equals(neighbor));

			if (index != -1) return index;
		}

		Debug.LogWarning($"[Tile {this.id}] INDEX ASSOCIATION ERROR. PLEASE FIX IF GENERATING NEW GLOBE!");
        return -1;
	}

	public Tile GetTileFromSide(Sides side)
    {
		foreach(Tile n in indexAssociation.Keys)
        {
			if (indexAssociation[n] == side)
				return n;
        }

		Debug.LogWarning("GetTileFromSide: Given side is not associated. Returning null");
		return null;
    }

	public Sides NextSide(Sides side)
    {
		if (side == Sides.SIX) return Sides.ONE;
		else if (side == Sides.NONE) return Sides.NONE;
		else return side + 1;
    }

	public Sides PrevSide(Sides side)
	{
		if (side == Sides.ONE) return Sides.SIX;
		else if (side == Sides.NONE) return Sides.NONE;
		else return side - 1;
	}
	/// <summary>
	/// Given a side, return the Tile opposite of that side. If null is returned, there's a big problem
	/// </summary>
	public Tile GetOppositeTile(Sides side)
    {
		Sides opposite = side;
		for (int i = 0; i < 3; ++i) opposite = NextSide(opposite);

		foreach (KeyValuePair<Tile, Sides> kvp in indexAssociation)
        {
			if (kvp.Value == opposite) return kvp.Key;
        }
		Debug.LogWarning($"Side opposite of {side} not found. Returning NULL");
		return null;
    }

	/// <summary>
	/// Given a neighbor tile, return the opposite facing neighbor. If null is returned, there's a big problem
	/// </summary>
	public Tile GetOppositeTile(Tile neighbor)
    {
		if (!neighborTiles.Contains(neighbor))
        {
			Debug.LogWarning($"Given Tile not a neighbor. Returning NULL");
			return null;
        }
		return GetOppositeTile(indexAssociation[neighbor]);
    }
	/// <summary>
	/// Given a side, returns an array of the two adjasent side values [side-1, side+1]
	/// </summary>
	public Sides[] GetAdjasentSides(Sides side)
    {
		Sides[] result = {PrevSide(side), NextSide(side)};
		/*
		if(side == Sides.NONE) { result[0] = Sides.NONE; result[1] = Sides.NONE; }
		else if(side == Sides.ONE) { result[0] = Sides.SIX; result[1] = side + 1; }
		else if(side == Sides.SIX) { result[0] = side - 1; result[1] = Sides.ONE; }
        else { result[0] = side - 1; result[1] = side + 1; }
		*/


		return result;
    }
	/// <summary>
	/// Get the Side enum associated with a tile's neighbor
	/// </summary>
	public Sides GetNeighborSide(Tile neighbor)
    {
		int index = neighborTiles.FindIndex(a => a.Equals(neighbor));
		if (index == -1) return Sides.NONE;
		else return indexAssociation[neighbor];
    }

    #endregion

    /// <summary>
    /// Gets all tiles reachable from this tile that share the same group id.
    /// </summary>
    /// <returns>A list of connected tiles with the same group id.</returns>
    public List<Tile> GetConnectedGroup()
    {
        List<Tile> connectedRegion = new List<Tile>();
        Stack<Tile> s = new Stack<Tile>();
        s.Push(this);

        while (s.Count > 0)
        {
            Tile t = s.Pop();

            if (!connectedRegion.Contains(t))
            {
                connectedRegion.Add(t);

                foreach (Tile v in t.neighborTiles)
                {
                    if (v.GroupID == this.GroupID)
                    {
                        s.Push(v);
                    }
                }
            }
        }
        return connectedRegion;
    }

    public int getID()
    {
		return id;
	}

    public void SaveMeshData()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Vertices = mf.sharedMesh.vertices;
        Triangles = mf.sharedMesh.triangles;
        UVs = mf.sharedMesh.uv;
    }

    public void RestoreMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if(mf.sharedMesh == null)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = Vertices;
            mesh.triangles = Triangles;
            mesh.uv = UVs;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            mf.sharedMesh = mesh;
        }
    }

}

public class ClockwiseComparer : IComparer
{
	private Vector3 mOrigin;

	public ClockwiseComparer(Vector3 origin)
    {
		mOrigin = origin;
	}

	public int Compare(object first, object second)
    {
		Vector3 v1 = (Vector3)first;
		Vector3 v2 = (Vector3)second;

		return IsClockwise (v2, v1, mOrigin);
	}

	public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
    {
		if (first == second)
        {
			return 0;
		}

		Vector3 firstOffset = first - origin;
		Vector3 secondOffset = second - origin;

		float angle1 = Mathf.Atan2 (firstOffset.x, firstOffset.z);
		float angle2 = Mathf.Atan2 (secondOffset.x, secondOffset.z);

		if (angle1 < angle2)
        {
			return 1;
		}

		if (angle1 > angle2)
        {
			return -1;
		}

		return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
	}

	
}

public class ClockwiseComparer2D : IComparer{
	private Vector2 mOrigin;

	public ClockwiseComparer2D(Vector2 origin){
		mOrigin = origin;
	}

	public int Compare(object first, object second){
		Vector2 v1 = (Vector2)first;
		Vector2 v2 = (Vector2)second;

		return IsClockwise (v2, v1, mOrigin);
	}

	public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin){
		if (first == second) {
			return 0;
		}

		Vector2 firstOffset = first - origin;
		Vector2 secondOffset = second - origin;

		float angle1 = Mathf.Atan2 (firstOffset.x, firstOffset.y);
		float angle2 = Mathf.Atan2 (secondOffset.x, secondOffset.y);

		if (angle1 < angle2) {
			return 1;
		}

		if (angle1 > angle2) {
			return -1;
		}

		return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
	}

}

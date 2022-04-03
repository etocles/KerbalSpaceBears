using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour {

	public List<Tile> worldTiles;
	private Hexsphere planet;
	public LineRenderer pathRenderer;

	public void setWorldTiles(List<Tile> tiles){
		worldTiles = tiles;
	}

	void Start(){
		planet = transform.parent.GetComponent<Hexsphere>();
	}

	// Find the closest fish tile
    public bool FindClosestIDTiles(Hexsphere.BiomeType biomeType, Tile start, out Stack<Tile> pathStack){
		pathStack = new Stack<Tile>();
		// if start is already the tile do not return a path
		if(start.GroupID == planet.FindBiomeIDByType(biomeType))
			return false;

		// get tiles of all biomeids from tiletype
		List<Tile> NewIceTiles = new List<Tile>();
		//Check if the connected region which start is in also contains end
		foreach(Tile potentialTile in planet.GetTilesByBiome(biomeType)){
			if (start.navigable && potentialTile.navigable && !potentialTile.Occupied)
				NewIceTiles.Add(potentialTile);
		}

		Tile end;
		if(!(end = ClosestTile(NewIceTiles, start)))
			return false;
			
		end.Occupied = true;
		// clicking on an oil or fish pool shouldn't do anything
		end.activeBear = ActiveBear.None;

		//Find the shortest path between two tiles using Dijkstra's algorithm
		List<Tile> unvisited = new List<Tile>();
		Dictionary<Tile, int> distanceMap = new Dictionary<Tile, int> ();
		Dictionary<Tile, Tile> previousNode = new Dictionary<Tile, Tile> ();
		int altPathLength;

		foreach (Tile tile in worldTiles) {
			if(tile.navigable){
				distanceMap.Add(tile, int.MaxValue);
				previousNode.Add(tile, null);
				unvisited.Add(tile);
			}
		}
		distanceMap [start] = 0;
		bool found = false;
		//MAIN LOOP
		while (unvisited.Count > 0 && !found) {
			//Get tile with min distance from source
			int d = int.MaxValue;
			Tile closest = null;
			foreach(Tile u in unvisited){
				if(distanceMap[u] < d){
					d = distanceMap[u];
					closest = u;
				}
			}
			//Mark this tile as visited by removing it from the unvisited list.
			unvisited.Remove(closest);
			//If no tile was found, then there is no possible path
			if(closest == null){
				return false;
			}
			foreach(Tile v in closest.neighborTiles){
				if(v.navigable && unvisited.Contains(v)){
					//altPathLength = distanceMap[closest] + 1;
					altPathLength = distanceMap[closest] + v.pathCost;
					if(altPathLength < distanceMap[v]){
						distanceMap[v] = altPathLength;
						previousNode[v] = closest;
					}
					if(v == end){
						//Target tile found
						found = true;
						break;
					}
				}
			}
		}
		//Build a stack of vectors of the shortest path
		Tile pathV = end;
		while(previousNode[pathV] != null){
			pathStack.Push(pathV);
			pathV = previousNode[pathV];
		}
		pathStack.Push(pathV);
		return true;
    }

	public bool findPath(Tile start, Tile end, out Stack<Tile> pathStack){
		pathStack = new Stack<Tile> ();
		if (!start.navigable || !end.navigable) {
			return false;
		}
		//Check if the connected region which start is in also contains end
		if (!DFS (start).Contains (end)) {
			return false;
		}

		end.Occupied = true;
		// clicking on the places around the ship
		// TODO: determine if this is needed to be updated
		//end.activeBear = ActiveBear.None; 

		//Find the shortest path between two tiles using Dijkstra's algorithm
		List<Tile> unvisited = new List<Tile> ();
		Dictionary<Tile, int> distanceMap = new Dictionary<Tile, int> ();
		Dictionary<Tile, Tile> previousNode = new Dictionary<Tile, Tile> ();
		int altPathLength;

		foreach (Tile tile in worldTiles) {
			if(tile.navigable){
				distanceMap.Add(tile, int.MaxValue);
				previousNode.Add(tile, null);
				unvisited.Add(tile);
			}
		}
		distanceMap [start] = 0;
		bool found = false;
		//MAIN LOOP
		while (unvisited.Count > 0 && !found) {
			//Get tile with min distance from source
			int d = int.MaxValue;
			Tile closest = null;
			foreach(Tile u in unvisited){
				if(distanceMap[u] < d){
					d = distanceMap[u];
					closest = u;
				}
			}
			//Mark this tile as visited by removing it from the unvisited list.
			unvisited.Remove(closest);
			//If no tile was found, then there is no possible path
			if(closest == null){
				return false;
			}
			foreach(Tile v in closest.neighborTiles){
				if(v.navigable && unvisited.Contains(v)){
					//altPathLength = distanceMap[closest] + 1;
					altPathLength = distanceMap[closest] + v.pathCost;
					if(altPathLength < distanceMap[v]){
						distanceMap[v] = altPathLength;
						previousNode[v] = closest;
					}
					if(v == end){
						//Target tile found
						found = true;
						break;
					}
				}
			}
		}
		//Build a stack of vectors of the shortest path
		Tile pathV = end;
		while(previousNode[pathV] != null){
			pathStack.Push(pathV);
			pathV = previousNode[pathV];
		}
		pathStack.Push (pathV);
		return true;
	}

	public void drawPath(Stack<Tile> pathStack){
		if (pathStack == null) {
			return;
		}
		pathRenderer.enabled = true;
		int i = 0;
		pathRenderer.positionCount = pathStack.Count;
		while (pathStack.Count > 0) {
			pathRenderer.SetPosition(i, pathStack.Pop().center);
			i++;
		}
	}

	// BFS, returns the closest tile from a list of tiles
	// 100 > tiles -> failure
	public Tile ClosestTile(List<Tile> tiles, Tile start) {
		HashSet<Tile> visited = new HashSet<Tile>();
		Queue<Tile> queue = new Queue<Tile>();
		queue.Enqueue(start);
		visited.Add(start);
		int count = 100;

		while(queue.Count > 0 && count > 0){
			Tile t = queue.Dequeue();
			foreach(Tile v in t.neighborTiles){
				if(visited.Contains(v))
					continue;
				
				foreach(Tile d in tiles){
					if(v == d)
						return d;
				}
				
				visited.Add(t);
				queue.Enqueue(v);
			}
			count--;
		}
		return null;
	}

	public List<Tile> DFS(Tile start){
		List<Tile> connectedRegion = new List<Tile> ();
		Stack<Tile> s = new Stack<Tile> ();
		s.Push (start);
		while (s.Count > 0) {
			Tile t = s.Pop();
			if(!connectedRegion.Contains(t)){
				connectedRegion.Add (t);
				foreach(Tile v in t.neighborTiles){
					if(v.navigable){
						s.Push(v);
					}
				}
			}
		}
		return connectedRegion;
	}
}
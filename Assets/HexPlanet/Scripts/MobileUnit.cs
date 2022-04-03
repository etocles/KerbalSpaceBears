using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobileUnit : MonoBehaviour {

	//The currently selected unit.
	public static MobileUnit selectedUnit;
	[Tooltip("The instance of the HexSphere which this unit resides on")]
	public Hexsphere parentPlanet;
	[Tooltip("How quickly this unit moves between tiles")]
	public float moveSpeed;
	[Tooltip("The reference to the tile on which this unit currently resides")]
	public Tile currentTile;
	[Tooltip("Reference to the GameObject Polar Bear")]
	public GameObject polarBear;

	public bool moving;

	public void Start()
	{
		StartCoroutine("CheckIfDrowned");
	}
	IEnumerator CheckIfDrowned()
	{
        while (true)
        {
			if (currentTile.ExtrudedHeight <= 0 && currentTile.BiomeType == Hexsphere.BiomeType.Water)
			{
				// check type (Space vs Non-Space)
				bool isSpace = (polarBear.GetComponent<PolarBearController>() != null);
				// if non-space, just delete it, no one loved it
				if (!isSpace) Destroy(polarBear);
				// if space, call PolarBearController.Die
				else polarBear.GetComponent<PolarBearController>().Die();
			}
			yield return new WaitForSeconds(5.0f);
		}
	}

    public void moveOnPath(Stack<Tile> path)
    {
		StartCoroutine ("move", path);
	}

	public IEnumerator moveOnPathCoroutine(Stack<Tile> path){
		yield return StartCoroutine("move", path);
	}

	public GameObject getGameObject(){ return this.gameObject; }

	private bool CanContinue(Tile tile)
    {
		// if next tile is ship, board if tamed
		PolarBearController temp = polarBear.GetComponent<PolarBearController>();
		if (tile == GameManager.instance.Rocket.GetComponent<RocketScript>().CurrentTile)
        {
			bool isTamed = temp != null;
			// if Tamed and returning to ship, then try to pay admission
			if (isTamed && temp.state == PolarBearController.BearState.SHIP)
            {
				return GameManager.instance.Rocket.GetComponent<RocketScript>().BoardBear(polarBear);
            }
			// if UnTamed, do nothing
			// if Tamed and not returning to ship, chill tf out
			return true;
		}
		// check if environment is traversible
		if (!tile.navigable || tile.BiomeType == Hexsphere.BiomeType.Water){
			return false;
        }
		// if boarding ship but received stop message, quit moving
		if (temp.state == PolarBearController.BearState.SHIP 
			&& !GameStateController.GoingToShip) return false;
		return true;
    }

	public IEnumerator move(Stack<Tile> path)
    {
		moving = true;
		//Pop the first tile from the stack as it is the one we are currently on
		if(path.Count > 0)
			currentTile = path.Pop();
        Vector3 lastPos = transform.position;
		//Pop off the tiles in the path and move to each one
		while (path.Count > 0)
        {
			Tile next = path.Pop();

			if (!CanContinue(next))
            {
				polarBear.GetComponent<PolarBearController>().ChangeState(PolarBearController.BearState.LOST);
				break;
            }

			//Vector3 currentPos = transform.position - parentPlanet.transform.position;
			Vector3 currentPos = transform.position;
			float t = 0f;
			//Spherically Interpolate current position to the next position

			while(t < 1f)
            {
				t += Time.deltaTime * moveSpeed;
				Vector3 vSlerp = Vector3.Lerp(currentTile.FaceCenter, next.FaceCenter, t) + ((currentTile.transform.up + next.transform.up) / 2).normalized * 0.0025f;
				transform.position = vSlerp;
                Vector3 lookDir = transform.position - lastPos;
				//Correct rotation to keep transform forward aligned with movement direction and transform up aligned with tile normal
				//transform.rotation = Quaternion.LookRotation(lookDir, transform.position - parentPlanet.transform.position);
				transform.LookAt(next.transform);
				transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
                lastPos = transform.position;
				this.transform.parent = currentTile.transform;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			//Assign the unit's current tile when it has finished interpolating to it.
			currentTile.currentBear = null;
			currentTile = next;
		}
		moving = false;
		currentTile.currentBear = GetComponent<Bear>();
	}
}

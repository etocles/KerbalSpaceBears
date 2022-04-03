﻿using UnityEngine;
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
			if (currentTile.ExtrudedHeight <= 0)
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
			//Vector3 currentPos = transform.position - parentPlanet.transform.position;
			Vector3 currentPos = transform.position;
			float t = 0f;
			//Spherically Interpolate current position to the next position

			while(t < 1f)
            {
				t += Time.deltaTime * moveSpeed;
				Vector3 vSlerp = Vector3.Slerp(currentPos, next.FaceCenter, t);
				transform.position = vSlerp;
                Vector3 lookDir = transform.position - lastPos;
                //Correct rotation to keep transform forward aligned with movement direction and transform up aligned with tile normal
                transform.rotation = Quaternion.LookRotation(lookDir, transform.position - parentPlanet.transform.position);
                lastPos = transform.position;
				this.transform.parent = currentTile.transform;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			//Assign the unit's current tile when it has finished interpolating to it.
			currentTile = next;
		}
		moving = false;
	}
}

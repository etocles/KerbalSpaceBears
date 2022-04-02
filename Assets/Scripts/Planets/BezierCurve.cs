using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
public class BezierCurve : MonoBehaviour 
{
	public GameObject start, middle, end;
	public Color color = Color.white;
	public float width = 0.2f;
	public int numberOfPoints = 20;

    //private Vector3[] positions = new Vector3[numberOfPoints];
	LineRenderer lineRenderer;

	void Start () 
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.useWorldSpace = true;
		lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
	}

    //Vector3[] getPos(){ return positions; }
	
	void Update () {
		if( lineRenderer == null || start == null || middle == null || end == null){
			return; // no points specified
		}

		// update line renderer
		lineRenderer.startColor = color;
		lineRenderer.endColor = color;
   		lineRenderer.startWidth = width;
		lineRenderer.endWidth = width;

  		if (numberOfPoints > 0){
    		lineRenderer.positionCount = numberOfPoints;
		}

		// set points of quadratic Bezier curve
		Vector3 p0 = start.transform.position;
		Vector3 p1 = middle.transform.position;
		Vector3 p2 = end.transform.position;
		float t;
		for(int i = 0; i < numberOfPoints; i++)
		{
            /*
			t = i / (numberOfPoints - 1.0f);
			positions[i] = (1.0f - t) * (1.0f - t) * p0 
			+ 2.0f * (1.0f - t) * t * p1 + t * t * p2;
			lineRenderer.SetPosition(i, positions[i]);*/
		}
	}
}

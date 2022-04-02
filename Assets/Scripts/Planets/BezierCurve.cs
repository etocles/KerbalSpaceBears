using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class BezierCurve : MonoBehaviour {
    public Transform[] points;
    public LineRenderer lineRenderer;
    
    private int curveCount = 0;    
    private int layerOrder = 0;
    private int total = 75;
    
        
    void Start(){
        if (!lineRenderer){ lineRenderer = GetComponent<LineRenderer>(); }
        lineRenderer.sortingLayerID = layerOrder;
        curveCount = (int) points.Length / 3;
    }

    void Update(){
        DrawCurve();
    }
    
    void DrawCurve(){
        for (int j = 0; j < curveCount; j++){
            for (int i = 1; i <= total; i++){
                float t = i / (float) total;
                int nodeIndex = j * 3;
                Vector3 location = CalculateCubicBezierPoint(t, points[nodeIndex].position, points[nodeIndex + 1].position, points[nodeIndex + 2].position, points[nodeIndex + 3].position);
                lineRenderer.positionCount = (j * total) + i;
                lineRenderer.SetPosition((j * total) + (i - 1), location);
            }
        }
    }
        
    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3){
        float u = 1 - t;
        
        Vector3 p = u * u * u * p0; 
        p += 3 * u * u * t * p1; 
        p += 3 * u * t * t * p2; 
        p += t * t * t * p3; 
        
        return p;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float cameraRotationSpeed = 60.0f;
    public Transform pivotPoint;
    public Transform camTransform;

    [SerializeField] float cameraZoomFloor;
    [SerializeField] float cameraZoomCeiling;
    [SerializeField] Vector3 cameraZoomSpeed;

    // Private var
    private Vector3 saveLocation;

    // Update is called once per frame
    void Update()
    {
        MouseInputs();
        Scroll();
    }

    void MouseInputs(){
        // Mouse Rotation
        if(Input.GetMouseButtonDown(0)){
            saveLocation = Input.mousePosition;
        }
        if(Input.GetMouseButton(0)){
            Vector3 loc = saveLocation - Input.mousePosition;
            pivotPoint.transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), loc.y*cameraRotationSpeed/35.0f, Space.World);
            pivotPoint.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), loc.x*cameraRotationSpeed/35.0f, Space.World);
            saveLocation = Input.mousePosition;
        }
    }

    void Scroll(){
        Vector3 targetPos = pivotPoint.position;

        Vector3 zoom = camTransform.localPosition;
        if(Input.mouseScrollDelta.y != 0)
            zoom += (Input.mouseScrollDelta.y * cameraZoomSpeed);

        if(Input.mouseScrollDelta.y > 0 && cameraZoomSpeed.y > cameraZoomFloor)
            return;
        else if(Input.mouseScrollDelta.y < 0 && cameraZoomSpeed.y < cameraZoomCeiling)
            return;

        camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, zoom, Time.deltaTime);
    }
}

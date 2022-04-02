using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float cameraRotationSpeed = 60.0f;
    public Transform pivotPoint;
    public Transform camTransform;

    [SerializeField] float cameraZoomFloor;
    [SerializeField] float cameraYAngleCeiling;
    [SerializeField] float cameraZoomCeiling;
    [SerializeField] float cameraZoomSpeed;

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
            // rotate side to side
            camTransform.RotateAround(pivotPoint.position, Vector3.up, loc.x*cameraRotationSpeed/35.0f);
            Vector3 pos = camTransform.position;
            // rotate up and down with Clamping
            if (Mathf.Abs(loc.y) >= 0.1f)
            {
                pos.y += loc.y * cameraRotationSpeed / 35.0f;
                pos.y = Mathf.Clamp(pos.y, -cameraYAngleCeiling, cameraYAngleCeiling);
                camTransform.position = pos.normalized * (camTransform.position - pivotPoint.position).magnitude;
                camTransform.LookAt(pivotPoint, Vector3.up);
            }

            saveLocation = Input.mousePosition;
        }
    }

    void Scroll(){
        Vector3 targetPos = pivotPoint.position;

        float zoom = 1.0f;
        Vector3 dir = camTransform.position - pivotPoint.position;
        dir = -dir;

        if (Mathf.Abs(Input.mouseScrollDelta.y) >= 0.001f)
            zoom = (Input.mouseScrollDelta.y * cameraZoomSpeed);
        else
            zoom = 0.0f;

        Vector3 finalPos = camTransform.localPosition + dir.normalized * zoom;
        dir = finalPos - pivotPoint.position;
        // if we would be going further than the allowed zoom-out, don't
        if(Input.mouseScrollDelta.y > 0 && dir.magnitude > cameraZoomCeiling)
            return;
        // if we would be going closer than the allowed zoom-in, don't
        else if (Input.mouseScrollDelta.y < 0 && dir.magnitude < cameraZoomFloor)
            return;

        camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, finalPos, Time.deltaTime);
    }
}

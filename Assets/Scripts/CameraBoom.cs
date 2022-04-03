using UnityEngine;
using System.Collections;

/*
 * FEATURES:
 * 
 * --Revolve[Right Click]-- Primary function of revolving the camera around a HexSphere by mimicing a camera boom
 * --Zoom[Scroll Wheel]-- Zooms the camera in and out along constraints defined by the HexSphere size and LOD
 *		Faster when far from the globe and slower when closer.
 * --Peek[Ctrl+Right Click]-- Pan and tilt the camera in place.
 *		Automatically returns to neutral position when key released
 * --Position Reset[F Key]-- Return the camera to it's default zoom and position 
 *		as determined at the beginning of a turn. Default location should be the
 *		pentagon of the current player's base.
 *
 * 
 * STILL NEEDED:
 * --Integration with turn switching and start points [DONE]
 * --Determine starting rotation of camera boom given a tile on the HexSphere [DONE]
 * --Adaption of zoom to HexShere size and LOD [DONE]
 * 
 * KNOWN ISSUES:
 * Script fails if planet not yet generated. Should not be an issue if the planet is
 * generated in a loading scene beforehand
 * 
 * Hibiki's Notes:
 * - CamPoint is a GameObject that defines the position of the angled view, as the child of CameraBoom. Main Camera is the child of CamPivot.
 * - Currently switches between two angles and positions of the CamPivot
 * - Adjusted zSpeedModifier to fix the offset caused by the angled view implementation
 * 
 * - Fixing/recalculating zoom value due to the default zoom now held by CamPivot instead of main camera.
 */
/// <summary>
/// Controls the player camera and its movement
/// </summary>
public class CameraBoom : MonoBehaviour 
{
	public static CameraBoom instance;
	[Header("Sensitivity")]
    public float mouseSensitivityX = 5.0f;
	public float mouseSensitivityY = 5.0f;
	[Tooltip("How quickly mouse sensitivity grows to max value"), Range(0, 1)]
	public float sensitivityGrowthRate = 0.01f;
	public float zoomSensitivity = 0.1f;

	[Header("References")]
	public Hexsphere hexsphere;

	private bool pauseMovement = false; //Freezes input during automatic movement
	private float rotY, rotX = 0.0f;
	private float sensitivityTimeMod = 0.0f;
	private float camRotY = 0.0f;
	private float camRotX = 0.0f;
	private readonly float peekTransitionDuration = 0.2f;
	private bool camPeekActive = false;
	private Camera playerCamera;
	private GameObject dummy;
    // cam pivot
    [Header("Angled View Options")]
    public int camAngle = 25;
    public float angleShift = 1.2f;
    private GameObject CamPivot;
    bool isMainView = true;
    bool retrievedDistance = false; // temporary var
    float dummyDist = 0f;
	float timeToSwitchPlanets = 2f;
	bool switchingPlanets = false;
	private float switchingTimer = 0.0f;
	private float baseZoomSensitivity;

	private float maxZoom;
	private float minZoom;
	private Vector3 oldPlanetLoc;
	//Reset each turn to a position above a player's pentagon OR hexagon of their last action
	//Something along those lines.
	private float defaultZoom, pivotZoom; 
	private Vector3 defaultRot;
	private readonly float transitionDuration = 0.5f;

	/// <summary>Sets the default camera location for the turn and moves to that point</summary>
	/// <param name="startTile">The tile on which to focus. This should be a pentagon
	/// but a hexagon will funciton fine.</param>
	public void SetStartLocation(Tile startTile)
	{
        dummy.transform.LookAt(2 * dummy.transform.position - startTile.transform.position);
        defaultRot = new Vector3(dummy.transform.localEulerAngles.x, dummy.transform.localEulerAngles.y, 0);
		MoveToLocation(defaultRot, defaultZoom);
        
	}

	private void MoveToLocation(Vector3 localEuler, float zoom)
	{
		pauseMovement = true;
		rotY = localEuler.x;
		rotX = localEuler.y;
		StartCoroutine(MoveToLoc(localEuler, zoom));
	}

	//Lerps the camera to a desired location
	private IEnumerator MoveToLoc(Vector3 localEuler, float zoom)
	{
		float alpha = 0.0f;
		Vector3 startZoom =  playerCamera.transform.localPosition;
		Vector3 endZoom = new Vector3(startZoom.x, startZoom.y, zoom);

		Quaternion startRot = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
		Quaternion endRot = Quaternion.Euler(localEuler.x, localEuler.y, localEuler.z);

		while (alpha < 1.0f)
		{
			alpha += Time.deltaTime * (Time.timeScale / (transitionDuration * 0.9f));
			playerCamera.transform.localPosition = Vector3.Lerp(startZoom, endZoom, alpha);
			transform.localRotation = Quaternion.Lerp(startRot, endRot, alpha);
			yield return null;
		} 
        
		//One-tenth of the transition is allocated for a cooldown
		alpha = 0;
		while (alpha < 1.0f)
		{
			alpha += Time.deltaTime * (Time.timeScale / (transitionDuration * 0.1f));
			yield return null;
		}

        if (!retrievedDistance) { // Get distance between dummy and camera at start
            dummyDist = Vector3.Distance(dummy.transform.position,playerCamera.transform.position);
            Debug.Log(dummyDist);
            retrievedDistance = true;
        }
		pauseMovement = false;

	}


	//Resets the camera to it's default location after a peek
	private IEnumerator ResetCam()
	{
		float alpha = 0.0f;
		Quaternion startRot = Quaternion.Euler(playerCamera.transform.localEulerAngles.x, playerCamera.transform.localEulerAngles.y, playerCamera.transform.localEulerAngles.z);
		Quaternion endRot = Quaternion.Euler(0, 0, playerCamera.transform.localEulerAngles.z);

		while (alpha < 1.0f)
		{
			alpha += Time.deltaTime * (Time.timeScale / (peekTransitionDuration * 0.9f));
			playerCamera.transform.localRotation = Quaternion.Lerp(startRot, endRot, alpha);
			yield return null;
		}

		//One-tenth of the transition is allocated for a cooldown
		alpha = 0;
		while (alpha < 1.0f)
		{
			alpha += Time.deltaTime * (Time.timeScale / (transitionDuration * 0.1f));
			yield return null;
		}

		pauseMovement = false;
	}

    // Lerps camera between top and angled view
    private IEnumerator SwitchView()
    {
        pauseMovement = true;
        float alpha = 0.0f;
        Vector3 startZoom, endZoom;
        Quaternion startRot, endRot;
        
        if (isMainView) {
            startZoom = new Vector3(0,0,pivotZoom);
            endZoom = new Vector3(0f, -angleShift, pivotZoom); // hardcoded position but will be calculated 
            startRot = Quaternion.Euler(0f,0f,0f);
            endRot = Quaternion.Euler(-camAngle, 0f, 0f); // hardcoded angle
        } else {
            endZoom = new Vector3(0f, 0f, pivotZoom);
            startZoom = new Vector3(0f, -angleShift, pivotZoom); // hardcoded position but will be calculated
            endRot = Quaternion.Euler(0f, 0f, 0f);
            startRot = Quaternion.Euler(-camAngle, 0f, 0f); // hardcoded angle
        }


        while (alpha < 1.0f) {
            alpha += Time.deltaTime * (Time.timeScale / (transitionDuration * 0.9f));
            CamPivot.transform.localPosition = Vector3.Lerp(startZoom, endZoom, alpha);
            CamPivot.transform.localRotation = Quaternion.Lerp(startRot, endRot, alpha);
            yield return null;
        }

        //One-tenth of the transition is allocated for a cooldown
        alpha = 0;
        while (alpha < 1.0f) {
            alpha += Time.deltaTime * (Time.timeScale / (transitionDuration * 0.1f));
            yield return null;
        }
        
        isMainView = isMainView ? false : true;
        pauseMovement = false;
    }
	public void SwitchPlanets(Hexsphere sphere)
    {
		if (hexsphere == null) oldPlanetLoc = GameStateController.instance.planets[0].transform.position;
		else oldPlanetLoc = hexsphere.transform.position;
		switchingTimer = 0.0f;
		switchingPlanets = true;
		hexsphere = sphere;
		transform.SetParent(hexsphere.transform);
	}
	public void OnPlanetSwitchCompleted()
    {
		
		transform.localPosition = new Vector3(0, 0, 0);
		switchingTimer = 0.0f;
		zoomSensitivity *= hexsphere.planetScale;
		float rad = hexsphere.planetScale;
		minZoom = -(rad * 2);
		maxZoom = 0.3f * rad;
		pivotZoom = (minZoom + maxZoom) * 0.6f; //Will maybe tweak this
		///*
		// set cam pivot and modify minZoom and maxZoom to nullify the offset

		minZoom += -(pivotZoom);
		maxZoom += -(pivotZoom);
		defaultZoom = (minZoom + maxZoom) * .5f;


		StartCoroutine(ResetCam());
		//MoveToLocation(defaultRot, defaultZoom);
	}

	private void Awake()
	{
		baseZoomSensitivity = zoomSensitivity;
		//Set globals
		dummy = new GameObject("Dummy");
		dummy.transform.SetParent(this.transform.parent);
		dummy.transform.position = new Vector3(0, 0, 0);
		defaultRot = new Vector3(0, 0, 0);

		playerCamera = GetComponentInChildren<Camera>();
		CamPivot = transform.GetChild(0).gameObject;
		instance = this;

		float rad = 1;
		minZoom = -(rad * 5);
		maxZoom = -0.35f - rad;
		pivotZoom = (minZoom + maxZoom) * 0.5f; //Will maybe tweak this
		///*
		// set cam pivot and modify minZoom and maxZoom to nullify the offset

		CamPivot.transform.position = new Vector3(0, 0, pivotZoom);
		minZoom += -(pivotZoom);
		maxZoom += -(pivotZoom);
		defaultZoom = (minZoom + maxZoom) * .5f;
		playerCamera.transform.localPosition = new Vector3(0, 0, defaultZoom);
		//*/

	}

    private void Start()
    {
		
	}

    private void Update()
	{
		float scrollDelta = Input.mouseScrollDelta.y;

		//Reset camera rotation on peek release
		if (camPeekActive && (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetMouseButtonUp(1)))
		{
			camPeekActive = false;
			pauseMovement = true;
			camRotX = 0.0f; camRotY = 0.0f;
			StartCoroutine(ResetCam());
		}
		/*
		//Pan and tilt camera in place. Clamp becomes more constrained when very close
		else if(Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftControl) && !pauseMovement) {
			camPeekActive = true;
			float clampTolerance = 0.4f;
			float clampVal = 25 * Mathf.Min(
				((playerCamera.transform.localPosition.z / minZoom) + (1 - maxZoom / minZoom)) * clampTolerance, 1);

			camRotX += Input.GetAxis("Mouse X") * mouseSensitivityX / 2;
			camRotY += Input.GetAxis("Mouse Y") * mouseSensitivityY / 2;

			camRotX = Mathf.Clamp(camRotX, -clampVal, clampVal);
			camRotY = Mathf.Clamp(camRotY, -clampVal, clampVal);
			playerCamera.transform.localEulerAngles = new Vector3(-camRotY, camRotX, 0);


		}
		*/
		// press O to rotate a bit
		else if (Input.GetKeyDown(KeyCode.O))
		{
			playerCamera.transform.Rotate(1, 1, 0, Space.World);
		}

		// rotation around HexSphere 
		//Speed modified based of of current zoom and average sensitivity
		else if (Input.GetMouseButton(1) && !pauseMovement && switchingPlanets == false)
		{
			sensitivityTimeMod += Mathf.Pow(sensitivityGrowthRate, 1.55f);
			sensitivityTimeMod = Mathf.Clamp01(sensitivityTimeMod);
			//Modify rotation speed 
			Vector3 localPos = playerCamera.transform.localPosition;
			float absZoom = Mathf.Abs(minZoom - maxZoom);
			float zoomDiff = Mathf.Abs(absZoom - localPos.z);

			float zoomSpeedModifier = Mathf.Abs(zoomDiff / absZoom * ((mouseSensitivityX + mouseSensitivityY) / 2));

			rotX += Input.GetAxis("Mouse X") * mouseSensitivityX * zoomSpeedModifier * sensitivityTimeMod;
			rotY -= Input.GetAxis("Mouse Y") * mouseSensitivityY * zoomSpeedModifier * sensitivityTimeMod;
			rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
			transform.localEulerAngles = new Vector3(rotY, rotX, 0.0f);
			if (GameplayCanvas.instance.ContextMenuVisible) GameplayCanvas.instance.HideContextMenu();
		}


		//Zoom with scroll wheel
		//Slower when close to the globe and faster when far
		else if (Mathf.Abs(scrollDelta) >= 0.1 && !pauseMovement)
		{
			Vector3 localPos = playerCamera.transform.localPosition;
			float absZoom = Mathf.Abs(minZoom - maxZoom);
			float zoomDiff = Mathf.Abs(absZoom - localPos.z);

			float zoomSpeedModifier = Mathf.Abs(zoomDiff / absZoom * zoomSensitivity);
			//Debug.Log(string.Format("speed: {0}, localposz: {1}, minZoom: {2}, sensitive:{3}", zoomSpeedModifier, localPos.z, minZoom, zoomSensitivity));
			float zoom = localPos.z + scrollDelta * zoomSpeedModifier;
			zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

			playerCamera.transform.localPosition = new Vector3(localPos.x, localPos.y, zoom);

		}

		//Return to neutral (default) position as determined at the beginning of a turn
		else if (Input.GetKeyDown(KeyCode.F) && !pauseMovement)
		{
			MoveToLocation(defaultRot, defaultZoom);
		}

		// Press S to switch view between angled cam and main cam 
		else if (Input.GetKeyDown(KeyCode.S) && !pauseMovement)
		{
			StartCoroutine("SwitchView");
		}

		else
		{
			sensitivityTimeMod = 0.0f;
		}

		if(switchingPlanets)
        {
			switchingTimer += Time.deltaTime;
			transform.position = Vector3.Lerp(oldPlanetLoc, hexsphere.transform.position, switchingTimer / timeToSwitchPlanets);
			if (switchingTimer > timeToSwitchPlanets || Vector3.Distance(transform.position, hexsphere.transform.position) < 0.05f)
			{
				switchingPlanets = false;
				OnPlanetSwitchCompleted();
			}
		}

	}

}

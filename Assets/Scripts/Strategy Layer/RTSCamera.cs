using UnityEngine;
using System.Collections;

public class RTSCamera : MonoBehaviour {

	public Camera cameraToControl;

	private float realScrollSpeed;
	public float scrollSpeed = 50f;
	public float edgeSize = 5f;

	public bool canZoom = true;
	public float zoomSpeed = 3f;
	public float defaultZoom = 30f;
	public float minZoom = 3f;
	public float maxZoom = 40f;

	public float cameraToSize;
	float cameraToSizeOnLastFrame;
	float cameraFromSize;
	float zoomStartTime;
	float zoomDuration = 0.5f;
	bool isZooming;

	public float worldBoundLeft = -30f;
	public float worldBoundRight = 30f;
	public float worldBoundTop = 15f;
	public float worldBoundBottom = -15f;

	private Vector3 PosOnClick;
	float xPosOnClick;
	float yPosOnClick;

	private float CamStartSize;
	private float timeScaleMultiplier = 1;

	public GameObject[] backgroundBillboards;
	float[] billboardSizes;

	public static bool cameraIsPanning = false;

	float realZoomSpeed = 0;
	float newCamSize;

	[HideInInspector] public bool canMove = true;

	bool autoMoving = false;
	float autoMoveStartTime;
	public float autoMoveLength = 0.5f;
	Vector3 autoMoveStartPos;
	Vector3 autoMoveDestination;


	void Awake()
	{
		if (cameraToControl == null)
			cameraToControl = Camera.main;

		CamStartSize = cameraToControl.orthographicSize;
		defaultZoom = CamStartSize;
		cameraToSize = CamStartSize;

		billboardSizes = new float[backgroundBillboards.Length]; 
		for(int i = 0; i < backgroundBillboards.Length; i++)
		{
			billboardSizes[i] = backgroundBillboards[i].transform.localScale.x;
		}
	}

	void Update()
	{
		if (!canMove)
			return;

		if (autoMoving) 
		{
			transform.position = Vector3.Lerp (autoMoveStartPos, autoMoveDestination, (Time.unscaledTime - autoMoveStartTime) / autoMoveLength);
			if (transform.position == autoMoveDestination)
				autoMoving = false;
		}

		if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
			autoMoving = false;


		//FOR HOLD RIGHT-CLICK SCROLLING
		if (Input.GetMouseButtonDown (1)) {
			PosOnClick = transform.position;
			xPosOnClick = Input.mousePosition.x;
			yPosOnClick = Input.mousePosition.y;

			autoMoving = false;
		}

		if (Input.GetMouseButton (1)) {
			float xPos = Input.mousePosition.x - xPosOnClick;
			float yPos = Input.mousePosition.y - yPosOnClick;

			transform.Translate (xPos * realScrollSpeed / 100, yPos * realScrollSpeed / 100, 0, Space.World);

			if (Vector3.Distance (transform.position, PosOnClick) >= 0.1f) {
				cameraIsPanning = true;
			}
		}
		if (Input.GetMouseButtonUp (1)) {
			Invoke ("SwitchCameraPanningBool", 0.1f);
		}

		//FOR LEFT-CLICK DRAGGING
		if (Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (2)) {
			PosOnClick = transform.position; //of the camera object, not the mouse
			xPosOnClick = Input.mousePosition.x; //of the mouse when clicked
			yPosOnClick = Input.mousePosition.y;

			autoMoving = false;
		}

		if (Input.GetMouseButton (0) || Input.GetMouseButton (2)) {
			float xPos = (Input.mousePosition.x - xPosOnClick) * cameraToControl.orthographicSize * 2 / Screen.height;
			float yPos = (Input.mousePosition.y - yPosOnClick) * cameraToControl.orthographicSize * 2 / Screen.height;

			transform.position = new Vector3 (PosOnClick.x - xPos, PosOnClick.y - yPos, -50);

			if (Vector3.Distance (transform.position, PosOnClick) >= 0.1f) {
				cameraIsPanning = true;
			}
		}
		if (Input.GetMouseButtonUp (0) || Input.GetMouseButtonUp (2)) {
			Invoke ("SwitchCameraPanningBool", 0.1f);
		}


		//FOR ZOOM

		if (canZoom) {
			realZoomSpeed = zoomSpeed * (cameraToControl.orthographicSize / maxZoom);

			//OLD WAY 
			/*
			if(canZoom && cameraToControl.orthographicSize <= maxZoom && cameraToControl.orthographicSize >= minZoom)
			{
				cameraToControl.orthographicSize = cameraToControl.orthographicSize -(Input.GetAxis("Mouse ScrollWheel") * realZoomSpeed);
				
				newCamSize = cameraToControl.orthographicSize;
				ScaleBackground(newCamSize/CamStartSize);
			}

			if(cameraToControl.orthographicSize > maxZoom)
			{
				cameraToControl.orthographicSize = maxZoom;
				newCamSize = cameraToControl.orthographicSize;
				ScaleBackground(newCamSize/CamStartSize);
			}
			if(cameraToControl.orthographicSize < minZoom)
			{
				cameraToControl.orthographicSize = minZoom;
				newCamSize = cameraToControl.orthographicSize;
				ScaleBackground(newCamSize/CamStartSize);
			}*/

			//NEW WAY

			if (canZoom)
			{
				cameraToSize -= Input.GetAxis ("Accelerate") / 25 * realZoomSpeed;
				cameraToSize += Input.GetAxis ("Reverse") / 25 * realZoomSpeed;
				cameraToSize -= Input.GetAxis ("Gamepad Right Vertical") / 25 * realZoomSpeed;	
			}

			if (Input.GetMouseButtonDown (2) || Input.GetKeyDown (KeyCode.JoystickButton9)) {
				cameraToSize = defaultZoom;
			}

			if (canZoom && cameraToControl.orthographicSize <= maxZoom && cameraToControl.orthographicSize >= minZoom) {
				cameraToSizeOnLastFrame = cameraToSize;
				cameraToSize -= (Input.GetAxis ("Mouse ScrollWheel") * realZoomSpeed);

				if (!isZooming && cameraToControl.orthographicSize != cameraToSize) {
					isZooming = true;
					zoomStartTime = Time.unscaledTime;
					cameraFromSize = cameraToControl.orthographicSize;
				} else if (isZooming && cameraToControl.orthographicSize == cameraToSize) {
					isZooming = false;
				} else if (isZooming && cameraToControl.orthographicSize != cameraToSize && cameraToSize == cameraToSizeOnLastFrame) {
					cameraToControl.orthographicSize = Mathf.Lerp (cameraFromSize, cameraToSize, (Time.unscaledTime - zoomStartTime) / zoomDuration);
				} else if (isZooming && cameraToControl.orthographicSize != cameraToSize && cameraToSize != cameraToSizeOnLastFrame) {
					zoomStartTime = Time.unscaledTime;
					cameraFromSize = cameraToControl.orthographicSize;
					cameraToControl.orthographicSize = Mathf.Lerp (cameraFromSize, cameraToSize, (Time.unscaledTime - zoomStartTime) / zoomDuration);
				}

				ScaleBackground (cameraToControl.orthographicSize / CamStartSize);
			}

			if (cameraToControl.orthographicSize > maxZoom) {
				cameraToControl.orthographicSize = maxZoom;
				newCamSize = cameraToControl.orthographicSize;
				cameraToSize = newCamSize;
				cameraFromSize = newCamSize;
				ScaleBackground (newCamSize / CamStartSize);
			}
			if (cameraToControl.orthographicSize < minZoom) {
				cameraToControl.orthographicSize = minZoom;
				newCamSize = cameraToControl.orthographicSize;
				cameraToSize = newCamSize;
				cameraFromSize = newCamSize;
				ScaleBackground (newCamSize / CamStartSize);
			}		
		}

		realScrollSpeed = scrollSpeed / 100 * cameraToControl.orthographicSize / CamStartSize;

		//FOR MOUSE INTO CORNER PANNING
		if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
		{
			if (Time.timeScale == 0 || Time.timeScale == 1)
				timeScaleMultiplier = 1;
			else if (Time.timeScale == 10)
				timeScaleMultiplier = 1;
			else if (Time.timeScale == 100)
				timeScaleMultiplier = 0.1f;

			Rect recdown = new Rect (0, 0, Screen.width, edgeSize);
			Rect recup = new Rect (0, Screen.height - edgeSize, Screen.width, edgeSize);
			Rect recleft = new Rect (0, 0, edgeSize, Screen.height);
			Rect recright = new Rect (Screen.width - edgeSize, 0, edgeSize, Screen.height);


			if (recdown.Contains (Input.mousePosition) || Input.GetKey (KeyCode.S))
				PanCamera ("down", 1);

			if (recup.Contains (Input.mousePosition) || Input.GetKey (KeyCode.W))
				PanCamera ("up", 1);

			if (recleft.Contains (Input.mousePosition) || Input.GetKey (KeyCode.A))
				PanCamera ("left", 1);

			if (recright.Contains (Input.mousePosition) || Input.GetKey (KeyCode.D))
				PanCamera ("right", 1);
		}

		//for Gamepad panning
		if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
		{
			PanCamera (Input.GetAxis ("Gamepad Left Horizontal"), Input.GetAxis ("Gamepad Left Vertical"));
		}

	}//END OF UPDATE


	void PanCamera(string direction, float speed)
	{
		if(Input.GetButton("Afterburners"))
		{
			speed *= 3;		}

		switch(direction)
		{
		default: Debug.LogError("Wrong Camera Direction");
			break;
		case "up": 
			transform.Translate(0, realScrollSpeed / timeScaleMultiplier * speed, 0, Space.World);
			break;
		case "down":
			transform.Translate(0, -realScrollSpeed / timeScaleMultiplier * speed, 0, Space.World);
			break;
		case "left":
			transform.Translate(-realScrollSpeed / timeScaleMultiplier * speed, 0, 0, Space.World);
			break;
		case "right":
			transform.Translate(realScrollSpeed / timeScaleMultiplier * speed, 0, 0, Space.World);
			break;
		}
	}
	void PanCamera(float horizontal, float vertical)
	{
		if(Input.GetButton("Afterburners"))
		{
			horizontal *= 4;
			vertical *= 4;
		}
		transform.Translate (realScrollSpeed / timeScaleMultiplier * horizontal, realScrollSpeed / timeScaleMultiplier * vertical, 0);
	}


	void LateUpdate()
	{
		Vector3 pos = transform.position;
		pos.x = Mathf.Clamp (pos.x, worldBoundLeft, worldBoundRight);
		pos.y = Mathf.Clamp (pos.y, worldBoundBottom, worldBoundTop);

		transform.position = pos;
	}

	void SwitchCameraPanningBool()
	{
		cameraIsPanning = false;
	}


	void ScaleBackground(float adjustScaleAmount)
	{
		for(int i=0; i < backgroundBillboards.Length; i++)
		{
			float z = backgroundBillboards[i].transform.localScale.z;
			backgroundBillboards[i].transform.localScale = new Vector3 (billboardSizes[i] * adjustScaleAmount, billboardSizes[i] * adjustScaleAmount , z);
		}
		/*float z = background.transform.localScale.z;

		background.transform.localScale = new Vector3 (1 * adjustScaleAmount, 1 * adjustScaleAmount , z);*/
	}

	public void ResetToDefaultSizes()
	{
		cameraToControl.orthographicSize = CamStartSize;
		cameraToSize = CamStartSize;
	}


	public void SetAutoMoveTarget(Vector3 newTargetPos)
	{
		autoMoving = true;
		autoMoveStartTime = Time.unscaledTime;
		autoMoveStartPos = transform.position;
		autoMoveDestination = new Vector3 (newTargetPos.x, newTargetPos.y, -50);
	}


}//MONO

using UnityEngine;
using System.Collections;

public class CameraControllerFighter : MonoBehaviour {

	public bool isPlayersRadar = true;

	public float dampTime = 0.2f;
	public float playerLeadDistance = 4.0f;
	public float velocityAffectsCameraAmount = .5f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;
	float startingSize;

	bool isRadarCam = false;
	//Vector3 offset = new Vector3(0,0,-50);
	Quaternion startingRotation;


	void Awake()
	{
		if (gameObject.name == "Camera (Radar)")
		{
			isRadarCam = true;
			if(isPlayersRadar)
				target = GameObject.FindGameObjectWithTag ("PlayerFighter").transform;
			startingSize = GetComponent<Camera>().orthographicSize;
			AdjustRadarCamSize();
			startingRotation = transform.rotation;
		}

		if(target == null && GameObject.FindGameObjectWithTag ("PlayerFighter") != null)
		{
			GameObject player = GameObject.FindGameObjectWithTag("PlayerFighter");
			GameObject camTarget = new GameObject();
			camTarget.transform.parent = player.transform;
			camTarget.transform.position = player.transform.position;
			camTarget.name = "Cam Target";
			target = camTarget.transform;
		}
	}
	
	void FixedUpdate () 
	{
		if (target && !isRadarCam)
		{
			Vector3 targetVel = (Vector3)target.transform.parent.GetComponent<Rigidbody2D>().velocity * velocityAffectsCameraAmount;
			Vector3 lead = target.transform.parent.transform.up * playerLeadDistance;
			target.position = target.parent.position + lead + targetVel;

			Vector3 point = Camera.main.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
		}
	}

	void LateUpdate()
	{
		if (target && isRadarCam)
		{
			//transform.position = target.transform.position + offset; // Will Child this to player. Put this in FixedUpdate if not childed
			transform.rotation = startingRotation;
		}
	}
	
	void AdjustRadarCamSize()
	{
		float sizeAdjustRatio = 1;
		
		if(GetComponent<Camera>().pixelWidth < GetComponent<Camera>().pixelHeight)
		{
			sizeAdjustRatio = (GetComponent<Camera>().pixelHeight - GetComponent<Camera>().pixelWidth)/GetComponent<Camera>().pixelHeight;
			GetComponent<Camera>().orthographicSize += (startingSize * sizeAdjustRatio);
			if(sizeAdjustRatio > 0)
				Debug.Log ("Adjusting Camera Size on " + this.name+ " by " + sizeAdjustRatio);
		}
		
	}

	
}
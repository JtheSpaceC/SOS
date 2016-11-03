using UnityEngine;
using System.Collections;

public class CameraControllerFighter : MonoBehaviour {

	public enum CameraBehaviour {Normal, Radar, AsteroidsBox, SelfPlayScene};
	public CameraBehaviour cameraBehaviour;
	public bool canUseAsteroidsBox = false;

	public float dampTime = 0.2f;
	public float playerLeadDistance = 4.0f;
	public float velocityAffectsCameraAmount = .5f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;
	float startingSize;

	Quaternion startingRotation;

	bool takeDPadInput = true;


	void Awake()
	{
		if (cameraBehaviour == CameraBehaviour.Radar)
		{
			GameObject player = GameObject.FindGameObjectWithTag ("PlayerFighter");
			if(player) target = player.transform;
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
		if ((cameraBehaviour == CameraBehaviour.Normal || cameraBehaviour == CameraBehaviour.SelfPlayScene) && target)
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
		if (cameraBehaviour == CameraBehaviour.Radar && target)
		{
			//transform.position = target.transform.position + offset; // Will Child this to player. Put this in FixedUpdate if not childed
			transform.rotation = startingRotation;
		}
		//TOGGLE ASTEROIDS STYLE COMBAT
		else if(canUseAsteroidsBox && (cameraBehaviour == CameraBehaviour.Normal || cameraBehaviour == CameraBehaviour.AsteroidsBox))
		{
			if(!ClickToPlay.instance.paused && !RadialRadioMenu.instance.radialMenuShown && 
				(Input.GetKeyDown(KeyCode.E) || (takeDPadInput && Input.GetAxis("Dpad Horizontal") < -0.5f)))
			{
				StartCoroutine("DPadInputWait");
				Tools.instance.combatAsteroidsStyleScript.itemsInZone.Clear();
				Tools.instance.combatAsteroidsStyleScript.enabled = !Tools.instance.combatAsteroidsStyleScript.enabled;
			}
		}
	}

	IEnumerator DPadInputWait()
	{
		takeDPadInput = false;
		yield return new WaitForSecondsRealtime(0.4f);
		takeDPadInput = true;
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

	public IEnumerator OrthoCameraZoomToSize(float newSize, float delayTime, float zoomTime)
	{
		yield return new WaitForSeconds(delayTime);

		float startTime = Time.time;
		float startSize = Camera.main.orthographicSize;

		while(Camera.main.orthographicSize != newSize)
		{
			Camera.main.orthographicSize = Mathf.Lerp(startSize, newSize, (Time.time - startTime)/zoomTime);
			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator PerspectiveCamZoom(float newDistance, float delayTime, float zoomTime)
	{
		float startTime = Time.time;
		float startDistance = Camera.main.transform.position.z;


		while(Camera.main.transform.position.z != newDistance)
		{
			Vector3 position = Camera.main.transform.position;
			position.z = Mathf.Lerp(startDistance, newDistance, (Time.time - startTime)/zoomTime);
			Camera.main.transform.position = position;
			yield return new WaitForEndOfFrame();
		}
	}

	
}
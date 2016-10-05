using UnityEngine;
using System.Collections;

public class FailedCapShipScaler : MonoBehaviour {

	public Transform targetObject;
	Camera mainCam;
	float mainCamOrthoSize;

	float distance;
	Vector2 dir;

	float startHeight;
	float maxScale;

	float newScale;

	void Awake()
	{
		mainCam = Camera.main;
		mainCamOrthoSize = mainCam.orthographicSize;
		startHeight = Screen.height;
		maxScale = mainCamOrthoSize / startHeight;
	}

	void LateUpdate()
	{
		distance = Vector2.Distance(targetObject.position, mainCam.transform.position);	
		dir = (targetObject.position - mainCam.transform.position);
		if(dir.magnitude > 1)
			dir = dir.normalized;

		if(distance > (mainCamOrthoSize-1) && !CameraTactical.instance.mapIsShown)
		{			
			transform.position = (Vector2)mainCam.transform.position + (dir * (mainCamOrthoSize-1));

			newScale = Mathf.Clamp(
				(Mathf.Sqrt(mainCamOrthoSize-1) / Mathf.Sqrt( distance))  
				* maxScale, 
				maxScale/15, maxScale);

			transform.localScale = new Vector3 (newScale, newScale, 1);
		}
		else
		{
			transform.position = targetObject.position;
			transform.localScale = new Vector3(maxScale, maxScale, 1);
		}
	}
}

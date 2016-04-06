using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarSignatures : MonoBehaviour {

	float radarRadius;
	Transform radarCamera;
	Vector3 startScale;
	float newScaleValue;

	bool alreadyResizedForTactical = false;


	void Awake () 
	{
		if(GameObject.Find("Camera (Radar)") == null)
		{
			this.enabled = false;
			return;
		}

		radarCamera = GameObject.Find("Camera (Radar)").transform;
		radarRadius = radarCamera.GetComponent<Camera>().orthographicSize;
		radarRadius -= radarRadius * 0.05f;
		startScale = transform.localScale;
	}


	void LateUpdate () 
	{
		if(Vector2.Distance(radarCamera.position, transform.parent.position) > radarRadius && !CameraTactical.instance.mapIsShown)
		{			
			transform.position = (Vector2)radarCamera.position + 
				(((Vector2)transform.position - (Vector2)radarCamera.position).normalized * radarRadius);

			newScaleValue = Mathf.Clamp(radarRadius/Vector2.Distance(radarCamera.position, transform.parent.position), 0.4f, 1);
			transform.localScale = new Vector3 (newScaleValue, newScaleValue, 1);

			alreadyResizedForTactical = false;
		}
		else
		{
			transform.localPosition = Vector2.zero;

			if(!CameraTactical.instance.mapIsShown)
			{
				transform.localScale = startScale;
			}
			else if(CameraTactical.instance.mapIsShown && !alreadyResizedForTactical)
			{
				transform.localScale = startScale;
				alreadyResizedForTactical = true;
			}
		}

	}

}//Mono
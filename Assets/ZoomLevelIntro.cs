using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoomLevelIntro : MonoBehaviour {

	public Vector3 farPoint;
	public float zoomDuration = 15;
	Vector3 cameraStartPoint;
	RTSCamera rtsCam;
	GameObject speedParticles;

	public List<GameObject> objectsToToggleAtStart;
	public List<GameObject> objectsToToggleAfterApproach;


	void Start () 
	{
		//turn off UI
		foreach(GameObject go in objectsToToggleAtStart)
		{
			go.SetActive(!go.activeInHierarchy);
		}

		PlayerAILogic.instance.TogglePlayerControl(false, false, false, false);

		speedParticles = GameObject.Find("Particles for Speed");
		speedParticles.SetActive(false);
		cameraStartPoint = Camera.main.transform.position;
		Camera.main.transform.position = farPoint;
		rtsCam = Camera.main.GetComponent<RTSCamera>();
		rtsCam.enabled = true;
		rtsCam.SetAutoMoveTarget(cameraStartPoint, zoomDuration);

		Invoke("FinishedZoom", zoomDuration);
	}

	void FinishedZoom()
	{
		speedParticles.SetActive(true);
		rtsCam.enabled = false;

		foreach(GameObject obj in objectsToToggleAfterApproach)
		{
			obj.SetActive(!obj.activeSelf);
		}

		PlayerAILogic.instance.TogglePlayerControl(true, true, true, true);

	}
	

}

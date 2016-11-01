using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoAndTutorialLevel : MonoBehaviour {

	public bool playIntro = true;
	public Vector3 farPoint;
	public float zoomDuration = 15;
	Vector3 cameraStartPoint;
	RTSCamera rtsCam;
	GameObject speedParticles;

	public List<GameObject> objectsToToggleAtStart;
	public List<GameObject> objectsToToggleAfterApproach;
	public CircleCollider2D asteroidFieldCollider;

	[Header("Tutorial Stuff")]
	public GameObject tutorialWindow;
	public Image tutorialImage;
	public float framesPerSecond = 20;
	public Sprite[] dodgeTutorialFrames;

	[Header("Demo Stuff")]
	public Canvas demoCanvas;


	void Start () 
	{
		if(playIntro)
			DoZoomIntro();
	}

	void DoZoomIntro()
	{
		//turn off UI
		foreach(GameObject go in objectsToToggleAtStart)
		{
			go.SetActive(!go.activeInHierarchy);
		}

		PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false);

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

		PlayerAILogic.instance.TogglePlayerControl(true, true, true, true, true, true, true);

	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.T))
		{
			if(!tutorialWindow.activeInHierarchy)
				OpenTutorialWindowPopup();
			else if(tutorialWindow.activeInHierarchy)
				CloseTutorialWindow();
		}

		if(Director.instance.timer > 1 && Director.instance.timer < 1.1f)
		{
			Director.instance.flowchart.SendFungusMessage("wd");
		}
	}


	public void TurnOnAsteroids()
	{
		asteroidFieldCollider.enabled = true;
	}


	public void OpenTutorialWindowPopup()
	{
		Tools.instance.StopCoroutine("FadeScreen");
		Tools.instance.MoveCanvasToFront(Tools.instance.blackoutCanvas);
		Tools.instance.MoveCanvasToFront(demoCanvas);
		Tools.instance.blackoutPanel.color = Color.Lerp (Color.black, Color.clear, 0.1f);
		AudioMasterScript.instance.masterMixer.SetFloat("Master vol", -15f);

		Tools.instance.AlterTimeScale(0);
		PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false);

		//TODO: Change which tutorial
		tutorialImage.GetComponent<SpriteAnimator>().frames = dodgeTutorialFrames;
		tutorialImage.GetComponent<SpriteAnimator>().framesPerSecond = framesPerSecond;

		tutorialWindow.SetActive(true);
	}

	public void CloseTutorialWindow()
	{
		Tools.instance.MoveCanvasToRear (Tools.instance.blackoutCanvas);
		Tools.instance.MoveCanvasToRear (demoCanvas);
		Tools.instance.blackoutPanel.color = Color.clear;
		AudioMasterScript.instance.masterMixer.SetFloat("Master vol", 0);

		bool[] bools = PlayerAILogic.instance.previousPlayerControlBools;
		PlayerAILogic.instance.TogglePlayerControl (bools[0], bools[1], bools[2], bools[3], bools[4], bools[5], bools[6]);
		Tools.instance.AlterTimeScale(1);

		tutorialWindow.SetActive(false);
	}
	

}

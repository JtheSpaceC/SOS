using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoAndTutorialLevel : MonoBehaviour {

	public bool playIntro = true;
	bool currentlyPlayingIntro = false;
	public Vector3 farPoint;
	public float introDuration = 15;
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
	public Slider skipIntroSlider;
	public Text skipIntroText;


	void Start () 
	{
		if(playIntro)
			DoZoomIntro();
	}

	void DoZoomIntro()
	{
		currentlyPlayingIntro = true;
		//turn off UI
		foreach(GameObject go in objectsToToggleAtStart)
		{
			go.SetActive(!go.activeInHierarchy);
		}

		PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false);

		Director.instance.timer = -introDuration;
		speedParticles = GameObject.Find("Particles for Speed");
		speedParticles.SetActive(false);
		cameraStartPoint = Camera.main.transform.position;
		Camera.main.transform.position = farPoint;
		rtsCam = Camera.main.GetComponent<RTSCamera>();
		rtsCam.enabled = true;
		rtsCam.SetAutoMoveTarget(cameraStartPoint, introDuration);

		Invoke("FinishedZoom", introDuration);
	}

	void FinishedZoom()
	{
		CancelInvoke("FinishedZoom"); //only needs cancelling if the intro was skipped, but can call it anyway

		Director.instance.timer = 0;
		speedParticles.SetActive(true);
		rtsCam.enabled = false;
		Camera.main.transform.position = cameraStartPoint;

		foreach(GameObject obj in objectsToToggleAfterApproach)
		{
			obj.SetActive(!obj.activeSelf);
		}

		PlayerAILogic.instance.TogglePlayerControl(true, true, true, true, true, true, true);

	}

	void Update()
	{
		//FOR SKIPPING INTRO
		if(currentlyPlayingIntro)
		{
			if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.JoystickButton0)
				|| Input.GetKey(KeyCode.JoystickButton1) || Input.GetKey(KeyCode.JoystickButton7))
			{
				skipIntroSlider.value += Time.deltaTime;
				skipIntroText.enabled = true;

				if(skipIntroSlider.value >= 1)
				{
					currentlyPlayingIntro = false;
					skipIntroSlider.transform.parent.gameObject.SetActive(false);
					CancelInvoke("FinishedZoom");

					Tools.instance.CommenceFade(0, .5f, Color.clear, Color.black, true);
					Invoke("FinishedZoom", .6f);
					Tools.instance.CommenceFade(.75f, .5f, Color.black, Color.clear, false);
				}
			}
			else
			{
				skipIntroSlider.value = 0;
				skipIntroText.enabled = false;
			}
		}

		//REMOVE: FOR SHOWING TUTORIAL
		if(Input.GetKeyDown(KeyCode.T))
		{
			if(!tutorialWindow.activeInHierarchy)
				OpenTutorialWindowPopup();
			else if(tutorialWindow.activeInHierarchy)
				CloseTutorialWindow();
		}

		//FIRST FUNGUS MESSAGE
		if(Director.instance.timer > 10 && Director.instance.timer < 10.1f)
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

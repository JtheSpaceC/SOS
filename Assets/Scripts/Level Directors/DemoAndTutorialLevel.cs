using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoAndTutorialLevel : MonoBehaviour {

	public bool playIntro = true;
	public bool skipToFirstEnemy = false;
	public Vector3 farPoint;
	public float introDuration = 15;
	Vector3 cameraStartPoint;
	RTSCamera rtsCam;
	GameObject speedParticles;

	GameObject player;

	public List<GameObject> objectsToToggleAtStart;
	public List<GameObject> objectsToToggleAfterApproach;
	public CircleCollider2D asteroidFieldCollider;
	public GameObject wreck;
	Transform bridgeView;
	public Slider bridgeViewSlider;

	[Header("Tutorial Stuff")]
	public GameObject tutorialWindow;
	public Image tutorialImage;
	public float framesPerSecond = 20;
	public Sprite[] dodgeTutorialFrames;

	[Header("Demo Stuff")]
	public GameObject firstEnemy;
	AIFighter firstEnemyAI;
	public Canvas demoCanvas;
	public Slider skipIntroSlider;
	public Text skipIntroText;
	public UnityEvent wreckReachedEvents;
	public float bridgeCheckoutDistance = 2;
	public float bridgeCheckoutTime = 4;
	Waypoint toWreckWaypoint;
	Waypoint bridgeCheckoutWaypoint;

	[Header("Progression")]
	bool firstMessagePlayed = false;

	[Header("Hints Stuff")]
	WeaponsPrimaryFighter playerWeapons;

	public Camera tacMapCamera;
	bool playerKnowsHowToMove = false;
	bool playerKnowsHowToShoot = false;
	bool playerKnowsHowToDodge = false;
	bool playerKnowsHowToAfterburn = false;
	bool playerKnowsMenu = false;
	bool playerKnowsOrders = false;
	bool playerKnowsMap = false;
	bool postedEndMessage = false;

	bool currentlyPlayingIntro = false;
	bool timeToCheckOutBridge = false;
	bool checkedOutBridge = false;
	bool firstEnemySpawned = false;
	bool firstEnemyDefeated = false; //for retreating or death
	bool calledEnemyRetreating = false;

	void OnEnable()
	{
		//not intended use, but it'll suffice to let us known an enemy spawned
		_battleEventManager.wingmanFirstClash += SetEnemyHasSpawnedBool;
		_battleEventManager.playerGotKill += FirstEnemyKilled;
	}
	void OnDisable()
	{
		_battleEventManager.wingmanFirstClash -= SetEnemyHasSpawnedBool;
		_battleEventManager.playerGotKill -= FirstEnemyKilled;
	}

	void Start () 
	{
		player = GameObject.FindGameObjectWithTag("PlayerFighter");
		playerWeapons = player.GetComponentInChildren<WeaponsPrimaryFighter>();
		bridgeView = bridgeViewSlider.transform.parent.parent;

		if(skipToFirstEnemy)
		{
			SkipToFirstEnemy();
		}
		else if(playIntro)
		{
			DoZoomIntro();
		}
		else //only really for when I skip in editor
		{
			DoObjectToggles();
		}
	}

	void DoObjectToggles()
	{
		foreach(GameObject go in objectsToToggleAtStart)
		{
			go.SetActive(!go.activeInHierarchy);
		}
		foreach(GameObject obj in objectsToToggleAfterApproach)
		{
			obj.SetActive(!obj.activeSelf);
		}
	}

	void SkipToFirstEnemy()
	{
		DoObjectToggles();
		SpawnWreck();
		Destroy(toWreckWaypoint.gameObject);
		wreck.transform.position = player.transform.position;
		firstMessagePlayed = true;
		timeToCheckOutBridge = true;
		checkedOutBridge = true;
		SpawnFirstEnemy();
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
		//FOR HINTS
		DoHints();

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
		if(!firstMessagePlayed && playerKnowsHowToMove)
		{
			if((Director.instance.timer > 10 && Vector2.Distance(player.transform.position, Vector3.zero) > 50)
				|| Input.GetKeyDown(KeyCode.Y) || Director.instance.timer > 60)
			{
				firstMessagePlayed = true;
				Director.instance.flowchart.SendFungusMessage("wd");
			}
		}
		//CHECK OUT THE BRIDGE
		if(timeToCheckOutBridge && !checkedOutBridge)
		{
			if(Vector2.Distance(player.transform.position, bridgeView.position) <= bridgeCheckoutDistance)
			{
				bridgeViewSlider.value += Time.deltaTime/bridgeCheckoutTime;
				if(bridgeViewSlider.value == 1)
				{
					BridgeCheckoutComplete();
				}
			}
			else
			{
				bridgeViewSlider.value = 0;
			}
		}

		//MONITOR FIRST ENEMY
		if(firstEnemySpawned && !firstEnemyDefeated)
		{
			if(Input.GetKey(KeyCode.L))
			{
				firstEnemyAI.healthScript.health = 1;
			}
			if(!firstEnemy.activeInHierarchy)
			{
				FirstEnemyRetreatedSuccessfully();
			}
			else if(!calledEnemyRetreating && firstEnemyAI.inRetreatState)
			{
				FirstEnemyRetreating();
			}
		}


	}//end of UPDATE()


	void TurnOnAsteroids()
	{
		asteroidFieldCollider.enabled = true;
	}

	#region Demo Functions In Story Sequence

	void SpawnWreck()
	{
		float playerDistance = Vector2.Distance(player.transform.position, Vector2.zero);

		if(playerDistance < 100 || playerDistance > 350)
			wreck.transform.position = (player.transform.position - Vector3.zero).normalized * 250; //250
		else
			wreck.transform.position = (Vector3.zero - player.transform.position).normalized * 250; //250

		wreck.SetActive(true);
		toWreckWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, new Vector2[]{wreck.transform.position}, 5);
		toWreckWaypoint.OnReachedEvents = wreckReachedEvents;
		toWreckWaypoint.destroyWhenReached = true;
	}

	public void ReachedWreck()
	{
		wreck.name = "Wrecked Transport";
		Director.instance.flowchart.SendFungusMessage("wr");
	}

	void CheckoutBridge()
	{
		timeToCheckOutBridge = true;
		bridgeCheckoutWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, bridgeView);
	}

	void BridgeCheckoutComplete()
	{
		checkedOutBridge = true;
		wreck.name = "\"Endeavour\" Wreck";
		bridgeView.gameObject.SetActive(false);
		Destroy(bridgeCheckoutWaypoint.gameObject);
		Director.instance.flowchart.SendFungusMessage("bc");
	}

	void SpawnFirstEnemy()
	{
		Director.instance.flowchart.SendFungusMessage("sf");
		firstEnemy.transform.position = player.transform.position + (Vector3)(Random.insideUnitCircle.normalized * 10);
		firstEnemy.SetActive(true);
	}
	void SetEnemyHasSpawnedBool()
	{
		firstEnemySpawned = true;
		firstEnemyAI = FindObjectOfType<AIFighter>();
		firstEnemy = firstEnemyAI.gameObject;
		firstEnemyAI.cowardice = 30;
	}
	void FirstEnemyKilled()
	{
		firstEnemyDefeated = true;
		Director.instance.flowchart.SendFungusMessage("eKilled");
	}
	void FirstEnemyRetreating()
	{
		calledEnemyRetreating = true;
		Director.instance.flowchart.SendFungusMessage("eRetreating");
	}
	void FirstEnemyRetreatedSuccessfully()
	{
		firstEnemyDefeated = true;
		Director.instance.flowchart.SendFungusMessage("eRetreatSuccess");
	}

	void RegroupWithWingmen()
	{
		print("REGROUP WITH WINGMEN");
	}

	#endregion

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
	
	void DoHints ()
	{
		if(!Tools.instance.useHintsThisSession)
		{
			return;
		}

		//these set the hints to KNOWN
		if (!playerKnowsHowToMove && Mathf.Approximately(Time.timeScale, 1) && Input.GetAxis ("Accelerate") != 0)
			playerKnowsHowToMove = true;
		if (!playerKnowsHowToShoot && playerWeapons.enabled && playerWeapons.allowedToFire &&  Input.GetButtonDown ("FirePrimary"))
			playerKnowsHowToShoot = true;
		if (!playerKnowsHowToDodge && Input.GetButton ("Dodge"))
			playerKnowsHowToDodge = true;
		if (!playerKnowsHowToAfterburn && Input.GetButton ("Afterburners"))
			playerKnowsHowToAfterburn = true;
		if (!playerKnowsOrders && RadialRadioMenu.instance.radialMenuShown)
			playerKnowsOrders = true;
		if (!playerKnowsMap && tacMapCamera.enabled)
			playerKnowsMap = true;
		if (!playerKnowsMenu && ClickToPlay.instance.escMenuIsShown)
			playerKnowsMenu = true;

		//this checks and activates the next relevant hint
//		if (!missionIsOver && !objectivesComplete && !playerKnowsHowToMove && timer > 9) 
//		{
//			if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
//				Subtitles.instance.PostHint (new string[] {
//					"Press UP ARROW to ACCELERATE"
//				});
//			else
//				if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
//					Subtitles.instance.PostHint (new string[] {
//						"Press RIGHT TRIGGER to ACCELERATE"
//					});
//			Subtitles.instance.CoolDownHintNoise ();
//			Subtitles.instance.CoolDownHintHighlight ();
//		}
//		else if (!missionIsOver && !objectivesComplete  && playerKnowsHowToMove && !playerKnowsHowToShoot && timer > 14) 
//		{
//			if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
//				Subtitles.instance.PostHint (new string[] {
//					"Press SPACEBAR to SHOOT"
//				});
//			else
//				if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
//					Subtitles.instance.PostHint (new string[] {
//						"Press A to SHOOT"
//					});
//			Subtitles.instance.CoolDownHintNoise ();
//			Subtitles.instance.CoolDownHintHighlight ();
//		}
//		else if (!missionIsOver && !objectivesComplete  && playerKnowsHowToMove && playerKnowsHowToShoot && !playerKnowsHowToDodge && timer > 21) 
//		{
//			if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
//				Subtitles.instance.PostHint (new string[] {
//					"Press LEFT CTRL to DODGE"
//				});
//			else
//				if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
//					Subtitles.instance.PostHint (new string[] {
//						"Press B to DODGE"
//					});
//			Subtitles.instance.CoolDownHintNoise ();
//			Subtitles.instance.CoolDownHintHighlight ();
//		}
		/*else if (!missionComplete && !playerKnowsMenu && timer > 30 && Subtitles.instance.hintsPanel.color == Color.clear) {
						if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
							Subtitles.instance.PostHint (new string[] {
								"View controls and more on the START MENU. Press ESC"
							});
						else
							if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
								Subtitles.instance.PostHint (new string[] {
									"View controls and more on the START MENU. Press START"
								});
						Subtitles.instance.CoolDownHintNoise ();
						Subtitles.instance.CoolDownHintHighlight ();
					}*/
		/*else if (!missionComplete && playerKnowsHowToMove && playerKnowsHowToShoot && playerKnowsHowToDodge && !playerKnowsHowToAfterburn && timer > 27)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Hold LEFT SHIFT for AFTERBURNERS (uses Nitro)"});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Hold X for AFTERBURNERS (uses Nitro)"});
			Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}
		else if(!missionComplete && !playerKnowsOrders && timer > 60 && Subtitles.instance.hintsPanel.color == Color.clear)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Experiment with the RADIO to give orders to wingmen or call for EXTRACTION.",
					"Use NUMBER KEYS 1,2,3,4 for RADIO commands."});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Experiment with the RADIO to give orders to wingmen or call for EXTRACTION.",
					"Use the D-PAD for RADIO commands."});	
			Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}
		else if(!missionComplete && !playerKnowsMap && timer > 90)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Press TAB or M to view the TACTICAL MAP"});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Press BACK to view the TACTICAL MAP"});				
			Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}*/
//		else if (!missionIsOver && !playerKnowsDocking && 
//			timeWhenFirstSawFighterTransportPickup > 0 && timer > timeWhenFirstSawFighterTransportPickup + 7) 
//		{
//			Subtitles.instance.PostHint (new string[]
//				{"To DOCK, fly over the Transport and then respond to the new RADIO command." + " This is timed. Try again if you miss."});
//			Subtitles.instance.CoolDownHintNoise ();
//			Subtitles.instance.CoolDownHintHighlight ();
//		}
//
//		//to start the enemies spawning once you know how to move
//		if (!spawnerScript.enabled && !objectivesComplete && playerKnowsHowToMove && playerKnowsHowToShoot && playerKnowsHowToDodge) {
//			spawnerScript.enabled = true;
//		}

	}//end of DOHINTS()
}

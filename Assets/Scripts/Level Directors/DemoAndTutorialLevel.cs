using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoAndTutorialLevel : MonoBehaviour {

	public enum PlayFrom {Intro, AfterIntro, ReachWreck, FirstEnemy, ReachWingmen, ReachConvoy, EscortComplete};
	public PlayFrom playFrom;

	public Vector3 farPoint;
	public float introDuration = 15;
	Vector3 cameraStartPoint;
	RTSCamera rtsCam;

	GameObject player;
	int playerStartingHealth;

	public List<GameObject> objectsToToggleAtStart;
	public List<GameObject> objectsToToggleAfterApproach;
	public CircleCollider2D asteroidFieldCollider;
	public GameObject wreck;
	Transform bridgeView;
	public Slider bridgeViewSlider;

	[Header("Tutorial Stuff")]
	public GameObject tutorialWindow;
	public Text tutorialHeaderText;
	public Image tutorialImage;
	public Text tutorialBodyText;
	public Sprite[] dodgeTutorialFrames;
	public Sprite[] ordersCoverMeFrames;
	public Sprite[] afterburnersFuelFrames;
	public Sprite[] spaceBrakeFrames;

	bool coverMeTutorialActive = false;
	bool afterburnerTutorialActive = false;
	bool spaceBrakeTutorialActive = false;
	float afterburnInputDelay = 1;
	bool dodgeTutorialActive = false;
	bool doneDodgeTutorial = false;

	[Header("Demo Stuff")]
	public Text objectiveText;
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
	Waypoint toWingmenWaypoint;
	Waypoint toConvoyWaypoint;
	Waypoint convoyEscortWaypoint;
	public UnityEvent wingmenReachedEvents;
	public GameObject arrow2;
	public GameObject arrow3;
	public GameObject rightPanel;
	public GameObject convoy;
	public GameObject missionCompleteScreen;
	public GameObject deathScreen;
	public Button mainRestartButton;
	public Button.ButtonClickedEvent restartEventsIfPlayerDead;

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

	bool currentlyPlayingIntro = false;
	bool timeToCheckOutBridge = false;
	bool checkedOutBridge = false;
	bool firstEnemySpawned = false;
	bool firstEnemyDefeated = false; //for retreating or death
	bool calledEnemyRetreating = false;
	bool headingForConvoy = false;
	bool reachedConvoy = false;
	float convoyReachedRegistrationDistance = 20;
	bool haveOrderedCoverMeAtLeastOnce = false;
	bool lastOfEmMessagePlayed = false;
	bool escortComplete = false;


	void OnEnable()
	{
		//not intended use, but it'll suffice to let us known an enemy spawned
		_battleEventManager.wingmanFirstClash += SetEnemyHasSpawnedBool;
		_battleEventManager.playerGotKill += FirstEnemyKilled;
		_battleEventManager.ordersCoverMe += OrderedWingmenIntoPosition;
		_battleEventManager.playerShotDown += CallPlayerDied;
	}
	void OnDisable()
	{
		_battleEventManager.wingmanFirstClash -= SetEnemyHasSpawnedBool;
		_battleEventManager.playerGotKill -= FirstEnemyKilled;
		_battleEventManager.ordersCoverMe -= OrderedWingmenIntoPosition;
		_battleEventManager.playerShotDown -= CallPlayerDied;
	}

	void CheckPlayerPrefs()
	{
		Debug.Log("Checking for Checkpoint");
		int checkpoint = PlayerPrefs.GetInt("checkpoint", 0);

		if(checkpoint == 0)
			playFrom = PlayFrom.Intro;
		else if(checkpoint == 1)
			playFrom = PlayFrom.ReachWreck;
		else if(checkpoint == 2)
			playFrom = PlayFrom.FirstEnemy;
		else if(checkpoint == 3)
			playFrom = PlayFrom.ReachWingmen;
		else if(checkpoint == 4)
			playFrom = PlayFrom.ReachConvoy;		
	}
	[ContextMenu("ClearCheckpoints")]
	public void ClearCheckpoints()
	{
		PlayerPrefs.DeleteKey("checkpoint");
		Debug.Log("Deleting Checkpoint." + PlayerPrefs.GetInt("checkpoint"));
	}

	void Start () 
	{
		player = GameObject.FindGameObjectWithTag("PlayerFighter");
		playerStartingHealth = (int)PlayerAILogic.instance.healthScript.maxHealth;
		// prevents Transport spawning at 0,0 if player didn't move
		player.transform.position += (Vector3)Random.insideUnitCircle.normalized * 0.001f; 
		playerWeapons = player.GetComponentInChildren<WeaponsPrimaryFighter>();
		bridgeView = bridgeViewSlider.transform.parent.parent;


		#if !UNITY_EDITOR
		CheckPlayerPrefs();
		#endif

		if(playFrom == PlayFrom.Intro)
		{
			DoZoomIntro();
		}
		else if(playFrom == PlayFrom.AfterIntro) //only really for when I skip in editor
		{
			SkipToAfterIntro();
		}
		else if(playFrom == PlayFrom.ReachWreck)
		{
			SkipToReachWreck();
			wreckReachedEvents.Invoke();
		}
		else if(playFrom == PlayFrom.FirstEnemy)
		{
			SkipToFirstEnemy();
			SpawnFirstEnemy();
		}
		else if(playFrom == PlayFrom.ReachWingmen)
		{
			SkipToReachWingmen();
		}
		else if(playFrom == PlayFrom.ReachConvoy)
		{
			SkipToReachConvoy();
		}
		else if(playFrom == PlayFrom.EscortComplete)
		{
			SkipToEscortComplete();
		}

		//disable afterburner for the tutorial, unless we're past it
		if(Tools.instance.useHintsThisSession)
		{
			if(playFrom == PlayFrom.AfterIntro || playFrom == PlayFrom.Intro)
				PlayerAILogic.instance.engineScript.hasAfterburner = false;
		}
	}
	#region Checkpoints/Skipping
	void DoZoomIntro()
	{
		currentlyPlayingIntro = true;
		//turn off UI
		foreach(GameObject go in objectsToToggleAtStart)
		{
			go.SetActive(!go.activeSelf);
		}

		StartCoroutine (PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false));

		Director.instance.timer = -introDuration;
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

		currentlyPlayingIntro = false;
		Director.instance.timer = 0;
		rtsCam.enabled = false;
		Camera.main.transform.position = cameraStartPoint;

		foreach(GameObject obj in objectsToToggleAfterApproach)
		{
			obj.SetActive(!obj.activeSelf);
		}

		StartCoroutine( PlayerAILogic.instance.TogglePlayerControl(true, true, true, true, true, true, true));
	}
	void SkipToAfterIntro()
	{
		foreach(GameObject go in objectsToToggleAtStart)
		{
			go.SetActive(!go.activeSelf);
		}
		foreach(GameObject obj in objectsToToggleAfterApproach)
		{
			obj.SetActive(!obj.activeSelf);
		}
	}
	void SkipToReachWreck()
	{
		SkipToAfterIntro();
		SpawnWreck();
		wreck.transform.position = player.transform.position;
		Destroy(toWreckWaypoint.gameObject);
		firstMessagePlayed = true;
	}
	void SkipToFirstEnemy()
	{
		SkipToReachWreck();
		timeToCheckOutBridge = true;
		checkedOutBridge = true;
		objectiveText.text = "OBJECTIVES:\n" +
			"* Hold Position";
	}
	void SkipToReachWingmen()
	{
		SkipToFirstEnemy();
		firstEnemyDefeated = true;
		Destroy(wreck);
		firstEnemySpawned = true;
		TurnOnAsteroids();
		toWingmenWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, new Vector2[]{player.transform.position}, 20);
		toWingmenWaypoint.destroyWhenReached = true;
		toWingmenWaypoint.OnReachedEvents = wingmenReachedEvents;
		objectiveText.text = "OBJECTIVES:\n" +
			"* Rejoin your squadmates";
	}
	void SkipToReachConvoy()
	{
		SkipToReachWingmen();
		haveOrderedCoverMeAtLeastOnce = true;
		Invoke("ForceWingmenToCover", 0.001f);

		HeadForConvoy();
		Destroy(toConvoyWaypoint.gameObject);
		convoy.transform.position = player.transform.position;
	}
	void ForceWingmenToCover()
	{
		PlayerAILogic.instance.squadLeaderScript.CoverMe(arrow2.GetComponent<AIFighter>());
		PlayerAILogic.instance.squadLeaderScript.CoverMe(arrow3.GetComponent<AIFighter>());
	}
	void SkipToEscortComplete()
	{
		SkipToReachConvoy();
		SetEscortWaypoint();
		TransportsClearedAsteroidField();
	}
	#endregion

	void Update()
	{
		//FOR HINTS
		DoHints();

		#region Game Progress Updates
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

		//FIRST FUNGUS MESSAGE
		if(!firstMessagePlayed)
		{
			if((Director.instance.timer > 8 && Vector2.Distance(player.transform.position, Vector3.zero) > 40)
				|| Director.instance.timer > 30)
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
			if(!firstEnemy)
			{
				FirstEnemyRetreatedSuccessfully();
			}
			else if(!calledEnemyRetreating && firstEnemyAI.inRetreatState)
			{
				FirstEnemyRetreating();
			}
		}

		//SEE IF WE REACHED THE CONVOY YET
		if(headingForConvoy && !reachedConvoy)
		{
			if(Vector2.Distance(player.transform.position, convoy.transform.position) <= convoyReachedRegistrationDistance)
			{
				ReachedConvoy();
			}
		}

		//SEE IF ALL ENEMIES ARE DEAD
		if(lastOfEmMessagePlayed && !escortComplete)
		{
			if(FindObjectsOfType<SpawnerGroup>().Length == 0) //if no more fighters are about to come in
			{
				//see if there's any active fighters
				if(PlayerAILogic.instance.enemyCommander.myFighters.Count == 0)
				{
					//if not, we're done
					TransportsClearedAsteroidField();
				}
			}
		}
		#endregion

		#region TutorialUpdates

		if(coverMeTutorialActive)
		{
			if(Input.GetKeyDown(KeyCode.Q) || 
				((Input.GetAxis("Dpad Vertical")) > 0.5f) && RadialRadioMenu.instance.takeDPadInput)
			{
				CancelInvoke("CoverMeTutorial");
				coverMeTutorialActive = false;
				CloseTutorialWindow();
				//fixes a bug where couldn't Escape after this because two off toggles came in a row
				ClickToPlay.instance.escCanGiveQuitMenu = true; 
				RadialRadioMenu.instance.ActivateRadialMenu();
			}
		}

		if(afterburnerTutorialActive)
		{
			afterburnInputDelay -= Time.unscaledDeltaTime; //don't want to instantly end the tutorial if afterburning

			if(afterburnInputDelay <= 0 && Input.GetButtonDown("Afterburners"))
			{
				afterburnerTutorialActive = false;
				CloseTutorialWindow();
			}
		}

		if(spaceBrakeTutorialActive)
		{
			if((Input.GetAxis("Accelerate") > 0 && Input.GetAxis("Reverse") >0) || 
				(Input.GetButton("Accelerate") && Input.GetButton("Reverse")))
			{
				spaceBrakeTutorialActive = false;
				CloseTutorialWindow();
			}
		}

		//if hit twice, do dodge tutorial
		if(Tools.instance.useHintsThisSession && !doneDodgeTutorial && 
			(int)PlayerAILogic.instance.healthScript.health <= playerStartingHealth -2)
		{
			doneDodgeTutorial = true;
			Invoke("DodgeTutorial", 1.5f);
		}

		if(dodgeTutorialActive)
		{
			if(Input.GetButtonDown("Dodge"))
			{
				dodgeTutorialActive = false;
				CloseTutorialWindow();
				PlayerAILogic.instance.dodgeScript.DodgePressed();
			}	
		}

		#endregion
	

	}//end of UPDATE()




	#region Demo Functions In Story Sequence

	void TurnOnAsteroids()
	{
		asteroidFieldCollider.enabled = true;
	}

	void SpawnWreck()
	{
		float playerDistance = Vector2.Distance(player.transform.position, Vector2.zero);

		if(playerDistance < 100 || playerDistance > 350)
			wreck.transform.position = (player.transform.position - Vector3.zero).normalized * 250;
		else
			wreck.transform.position = (Vector3.zero - player.transform.position).normalized * 250; 

		wreck.SetActive(true);
		toWreckWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, new Vector2[]{wreck.transform.position}, 5);
		toWreckWaypoint.OnReachedEvents = wreckReachedEvents;
		toWreckWaypoint.destroyWhenReached = true;
	}

	public void ReachedWreck()
	{
		PlayerPrefs.SetInt("checkpoint", 1);
		print("SET NEW INT 1");

		wreck.name = "Wrecked Transport";
		Director.instance.flowchart.SendFungusMessage("wr");
	}

	void CheckoutBridge()
	{
		timeToCheckOutBridge = true;
		bridgeCheckoutWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, bridgeView);
		objectiveText.text = "OBJECTIVES:\n" +
			"* Identify the Wreck";
	}

	void BridgeCheckoutComplete()
	{
		checkedOutBridge = true;
		wreck.name = "\"Endeavour\" Wreck";
		bridgeView.gameObject.SetActive(false);
		Destroy(bridgeCheckoutWaypoint.gameObject);
		Director.instance.flowchart.SendFungusMessage("bc");
		objectiveText.text = "OBJECTIVES:\n" +
			"* Hold Position";
	}

	void SpawnFirstEnemy()
	{
		PlayerPrefs.SetInt("checkpoint", 2);
		print("SET NEW INT 2");

		Director.instance.flowchart.SendFungusMessage("sf");
		firstEnemy.transform.position = player.transform.position + (Vector3)(Random.insideUnitCircle.normalized * 10);
		firstEnemy.SetActive(true);
	}
	void SetEnemyHasSpawnedBool()
	{
		if(firstEnemySpawned)
			return;
		firstEnemySpawned = true;
		firstEnemyAI = FindObjectOfType<AIFighter>();
		firstEnemy = firstEnemyAI.gameObject;
		firstEnemyAI.cowardice = 30;
		firstEnemyAI.despawnDistance = 75f;
		objectiveText.text = "OBJECTIVES:\n" +
			"* Defend Yourself";
	}
	void FirstEnemyKilled()
	{
		if(!firstEnemyDefeated)
			Director.instance.flowchart.SendFungusMessage("eKilled");
		firstEnemyDefeated = true;
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
		Vector2 spawnPos = player.transform.position - (player.transform.position.normalized * 250);
		toWingmenWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, new Vector2[]{spawnPos}, 20);
		toWingmenWaypoint.destroyWhenReached = true;
		toWingmenWaypoint.OnReachedEvents = wingmenReachedEvents;
		objectiveText.text = "OBJECTIVES:\n" +
			"* Rejoin your squadmates";
	}

	public void ReachedWingmen()
	{
		arrow2.SetActive(true);
		arrow2.transform.position = toWingmenWaypoint.transform.position + (Vector3)(Random.insideUnitCircle.normalized*3);
		arrow3.SetActive(true);
		arrow3.transform.position = toWingmenWaypoint.transform.position + (Vector3)(Random.insideUnitCircle.normalized*3);
		rightPanel.SetActive(true);
		player.GetComponentInChildren<SquadronLeader>().activeWingmen.Add(arrow2);
		player.GetComponentInChildren<SquadronLeader>().activeWingmen.Add(arrow3);
		player.GetComponentInChildren<SquadronLeader>().SetUp();
		if(playFrom != PlayFrom.ReachConvoy && playFrom != PlayFrom.EscortComplete) //TODO: Add each subsequent checkpoint here too
			Director.instance.flowchart.SendFungusMessage("mw");
		PlayerPrefs.SetInt("checkpoint", 3);
		print("SET NEW INT 3");

	}

	void OrderedWingmenIntoPosition()
	{
		if(!haveOrderedCoverMeAtLeastOnce && (playFrom != PlayFrom.ReachConvoy))		
		{
			haveOrderedCoverMeAtLeastOnce = true;
			Director.instance.flowchart.SendFungusMessage("wif");
		}
	}

	void HeadForConvoy()
	{
		headingForConvoy = true;

		Vector2 dir = player.transform.position.normalized;
		Vector2 newConvoyPosition = dir * 500;
		if(Vector2.Distance(player.transform.position, newConvoyPosition) < 150)
		{
			print("Player was too close");
			newConvoyPosition += dir * 150;
		}

		toConvoyWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Move, new Vector2[]{newConvoyPosition}, 20);
		toConvoyWaypoint.destroyWhenReached = true;
		convoy.transform.localScale = Vector3.one;
		convoy.transform.position = newConvoyPosition;
		convoy.GetComponent<moverBasic>().enabled = false;
		convoy.SetActive(true);
		objectiveText.text = "OBJECTIVES:\n" +
			"* Rendezvous with Freighters";
	}

	void ReachedConvoy()
	{
		reachedConvoy = true;
		convoy.GetComponent<moverBasic>().speed = new Vector3(0, 1.5f, 0);
		convoy.GetComponent<moverBasic>().enabled = true;
		if(!escortComplete)
			Director.instance.flowchart.SendFungusMessage("rc");
		asteroidFieldCollider.enabled = false;
		PlayerPrefs.SetInt("checkpoint", 4);
		print("SET NEW INT 4");
	}

	void SetEscortWaypoint()
	{
		convoyEscortWaypoint = Tools.instance.CreateWaypoint(Waypoint.WaypointType.Escort, convoy.transform);
		convoyEscortWaypoint.zoneBoxAnimation.SetActive(false);
		convoyEscortWaypoint.playChimeOnEnter = false;
		objectiveText.text = "OBJECTIVES:\n" +
			"* Escort Civilian Convoy";
	}

	void ToggleEnemySpawner()
	{
		GetComponent<Spawner>().enabled = !GetComponent<Spawner>().enabled;
	}

	void LastOfThemMessage()
	{
		lastOfEmMessagePlayed = true;
	}

	void TransportsClearedAsteroidField()
	{
		escortComplete = true;
		Director.instance.flowchart.SendFungusMessage("tcaf");
		Destroy(convoyEscortWaypoint.gameObject);
		objectiveText.text = "OBJECTIVES:\n" +
			"* Return to base";
	}

	IEnumerator MissionComplete()
	{
		ClearCheckpoints();

		Tools.instance.CommenceFade(0, 4, Color.clear, Color.black, true);
		AudioMasterScript.instance.FadeChannel("Master vol", -80, 2, 6);

		yield return new WaitForSeconds(3);
		StartCoroutine( PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false));

		yield return new WaitForSeconds(1.5f);
		Tools.instance.MoveCanvasToFront(demoCanvas);
		objectiveText.text = "";
		missionCompleteScreen.SetActive(true);

		_battleEventManager.instance.CallMissionComplete();

		yield return new WaitForSeconds(3.5f);
		Destroy(AudioMasterScript.instance.gameObject);
	}

	void CallPlayerDied()
	{
		StopCoroutine("MissionComplete");
		StartCoroutine("PlayerDied");
	}
	IEnumerator PlayerDied()
	{
		StartCoroutine( PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, true));

		//means main menu Restart will go to checkpoint, not total restart
		mainRestartButton.onClick = restartEventsIfPlayerDead;

		yield return new WaitForSeconds(5);

		Tools.instance.CommenceFade(0, 4, Color.clear, Color.black, true);
		AudioMasterScript.instance.FadeChannel("Master vol", -80, 2, 6);

		yield return new WaitForSeconds(3);
		StartCoroutine( PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false));

		yield return new WaitForSeconds(1.5f);
		Tools.instance.MoveCanvasToFront(demoCanvas);
		objectiveText.text = "";
		deathScreen.SetActive(true);

		yield return new WaitForSeconds(2.5f);
		Destroy(AudioMasterScript.instance.gameObject);
	}

	#endregion

	#region Tutorials
	public void OpenTutorialWindowPopup(string header, string body, Sprite[] tutorialFrames, float framesPerSecond, bool setPlayerFullyInactive)
	{
		Tools.instance.StopCoroutine("FadeScreen");
		Tools.instance.MoveCanvasToFront(Tools.instance.blackoutCanvas);
		Tools.instance.MoveCanvasToFront(demoCanvas);
		Tools.instance.blackoutPanel.color = Color.Lerp (Color.black, Color.clear, 0.1f);
		AudioMasterScript.instance.masterMixer.SetFloat("Master vol", -15f);

		Tools.instance.AlterTimeScale(0);
		Tools.instance.VibrationStop();

		if(setPlayerFullyInactive)
			StartCoroutine( PlayerAILogic.instance.TogglePlayerControl(false, false, false, false, false, false, false));

		tutorialHeaderText.text = header;
		tutorialImage.GetComponent<SpriteAnimator>().framesPrimary = tutorialFrames;
		tutorialImage.GetComponent<SpriteAnimator>().framesPerSecond = framesPerSecond;
		tutorialBodyText.text = body;

		tutorialWindow.SetActive(true);
	}

	public void CloseTutorialWindow()
	{
		Tools.instance.MoveCanvasToRear (Tools.instance.blackoutCanvas);
		Tools.instance.MoveCanvasToRear (demoCanvas);
		Tools.instance.blackoutPanel.color = Color.clear;
		AudioMasterScript.instance.masterMixer.SetFloat("Master vol", 0);

		bool[] bools = PlayerAILogic.instance.previousPlayerControlBools;
		StartCoroutine( PlayerAILogic.instance.TogglePlayerControl (bools[0], bools[1], bools[2], bools[3], bools[4], bools[5], bools[6]));
		Tools.instance.AlterTimeScale(1);

		tutorialWindow.SetActive(false);
	}

	void AfterburnerFuelTutorial()
	{
		PlayerAILogic.instance.engineScript.hasAfterburner = true;

		if(!Tools.instance.useHintsThisSession)
		{
			Debug.Log("Not Using Hints This Session. Returning..");
			return;
		}
		afterburnerTutorialActive = true;

		const string header = "Afterburners";
		string commandKey = InputManager.instance.inputFrom == InputManager.InputFrom.controller? "\"X button\"" : "\"Left Shift\"";
		string body = "Afterburners can help you to get around faster, or to get behind an enemy.\n" +
			"Be careful, though. Afterburners use up nitro, which can't be refueled.\n\n" +
			"Press " + commandKey + " to use Afterburners now..";
		
		OpenTutorialWindowPopup(header, body, afterburnersFuelFrames, 10, true);
	}

	void SpaceBrakeTutorial()
	{
		if(!Tools.instance.useHintsThisSession)
		{
			Debug.Log("Not Using Hints This Session. Returning..");
			return;
		}
		if(checkedOutBridge)
		{
			return;
		}
		else if(bridgeViewSlider.value != 0)
		{
			Invoke("SpaceBrakeTutorial", bridgeCheckoutTime);
			return;
		}
		spaceBrakeTutorialActive = true;

		const string header = "Space Brake";
		string commandKey = InputManager.instance.inputFrom == InputManager.InputFrom.controller? 
			"\"Left & Right Trigger (together)\"" : "\"Up + Down arrows (together)\"";
		string body = "Use the Space Brake to come to a complete stop.\n\n" +
			"Use " + commandKey + " now..";

		OpenTutorialWindowPopup(header, body, spaceBrakeFrames, 10, true);
	}

	void DodgeTutorial()
	{
		if(!Tools.instance.useHintsThisSession)
		{
			Debug.Log("Not Using Hints This Session. Returning..");
			return;
		}

		if(tutorialWindow.activeSelf || RadialRadioMenu.instance.radialMenuShown)
		{
			Debug.Log("Tutorial Window or Radial Menu Already Up. Waiting..");
			Invoke("DodgeTutorial", 1.5f);
			return;
		}

		string header = "Dodging";
		string commandKey = InputManager.instance.inputFrom == InputManager.InputFrom.controller? "\"B Button\"" : "\"Left Ctrl\"";
		string body = "You can dodge bullets or asteroids.\n" +
			"Enemies can do this too.\n" +
			"Note: Enemies are easier to hit from the sides or rear.\n\n\"" +
			"Press the Dodge button (" + commandKey + ") now..";

		dodgeTutorialActive = true;
		OpenTutorialWindowPopup(header, body, dodgeTutorialFrames, 15, true);
	}

	void CoverMeTutorial()
	{
		if(!Tools.instance.useHintsThisSession)
		{
			Debug.Log("Not Using Hints This Session. Returning..");
			return;
		}

		if(haveOrderedCoverMeAtLeastOnce)
			return;
		if(RadialRadioMenu.instance.radialMenuShown)
		{
			Invoke("CoverMeTutorial", 1.5f);
			return;
		}
		coverMeTutorialActive = true;

		string header = "Ordering Wingmen";
		string commandKey = InputManager.instance.inputFrom == InputManager.InputFrom.controller? "\"D-pad Up\"" : "\"Q\"";
		string commandInstruction = InputManager.instance.inputFrom == InputManager.InputFrom.controller?
			"\"Left Stick\"" : "\"Left/Right arrow\" keys";
		string body = "To give orders to your wingmen, open the orders menu with " + commandKey + " and use the "
			+ commandInstruction + " and \"Fire\" button to select orders.\n\n" +
			"To give the \"Cover Me\" order to all wingmen now, first open the Orders Menu ("+ commandKey +")..";

		OpenTutorialWindowPopup(header, body, ordersCoverMeFrames, 10, true);
	}

	#endregion
	
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

	public void LeaveFeedback()
	{
		Application.OpenURL("https://goo.gl/forms/eifVi4PmTx2ZnTEk2");
	}
}

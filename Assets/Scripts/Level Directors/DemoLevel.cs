using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class DemoLevel : MonoBehaviour {

	public static DemoLevel instance;

	HealthFighter playerHealth;
	AITransport AITrans;
	Spawner spawnerScript;
	AICommander enemyCommander;
	WeaponsPrimaryFighter playerWeapons;

	public AIFighter mate2AIScript;
	public AIFighter mate3AIScript;

	public int killLimitForDemo = 4;
	public float convoyGuardDistance = 100f;
	public GameObject convoyPointerArrow;
	public Transform convoy;
	public CircleCollider2D asteroidField;

	public Text objectiveText;
	public Button feedbackButton;
	public Button controlsButton;
	public Button resumeButton;
	public Button quitButton;
	public GameObject deathPanel;
	public GameObject equipPanel;
	public PlayerAILogic playerLogicScript;
	GameObject player;
	float timer;
	
	[Header("Start, End & Death Menu stuff")]
	public Text keyboardControlsText;
	string startKeyboardText;
	public Image controlsAtStartImage;
	public Sprite controls1;
	public Sprite controls2;
	public GameObject instructionsPanel;
	public Button backButton;
	public Button restartAtDeathButton;
	public Button buttonToStartOn;
	public Text instructionsText;
	public Canvas demoStuffCanvas;
	public Canvas playerUICanvas;
	public Canvas playerWorldspaceUI;
	public Canvas arrow2WorldspaceUI;
	public Canvas arrow3WorldspaceUI;
	public GameObject missionCompleteMenu;

	[Header("Craft Equip Stuff")]
	public GameObject playerMissiles;

	public Text tutorialHeaderText;
	public Text tutorialBodyText;
	[TextArea(3, 6)]
	public string dodgeTutorialString;
	public Button defaultEquipButton;
	public Button finalLaunchButton;

	[Header("Hints Stuff")]
	public Camera tacMapCamera;

	bool playerKnowsHowToMove = false;
	bool playerKnowsHowToShoot = false;
	bool playerKnowsHowToDodge = false;
	bool playerKnowsHowToAfterburn = false;
	bool playerKnowsMenu = false;
	bool playerKnowsOrders = false;
	bool playerKnowsMap = false;
	bool playerKnowsDocking = false;
	bool postedEndMessage = false;

	float timeWhenFirstSawFighterTransportPickup = -1;

	bool setWingmanOrders = false;

	[Header("For warp in")]
	public GameObject transport;
	public List<GameObject> playerGroup;
	[HideInInspector] public Vector2 startPos;

	[HideInInspector] public bool objectivesComplete = false;
	[HideInInspector] public bool missionIsOver = false;
	[HideInInspector] public float clearedMissionObjectiveAtThisTime = Mathf.Infinity;

	[Header("End of level stuff")]
	public string levelToLoad = "";
	public float waitTimeAfterDeath = 20;
	float timePlayerStartedLeaving = 0;

	
	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were 2 DemoLevel scripts. Destroying 1.");
			Destroy(gameObject);
			return;
		}

		ToggleSquadUI();
		player = GameObject.FindGameObjectWithTag ("PlayerFighter");
		playerWeapons = player.GetComponentInChildren<WeaponsPrimaryFighter>();
		playerWeapons.allowedToFire = false;

		playerHealth = player.GetComponent<HealthFighter> ();

		enemyCommander = GameObject.Find ("Enemy Commander").GetComponent<AICommander> ();

		startKeyboardText = keyboardControlsText.text;

		/*feedbackButton.gameObject.SetActive (true);
		Navigation navUp = resumeButton.navigation;
		navUp.selectOnUp = feedbackButton;
		resumeButton.navigation = navUp;

		Navigation navUp2 = quitButton.navigation;
		navUp2.selectOnUp = feedbackButton;
		quitButton.navigation = navUp2;

		Navigation navDown = controlsButton.navigation;
		navDown.selectOnDown = feedbackButton;
		controlsButton.navigation = navDown;*/

		objectiveText.enabled = false;

		startPos = (GameObject.FindGameObjectWithTag ("AIManager").transform.FindChild ("PMC Commander").position - Vector3.zero).normalized * 500;

		spawnerScript = GetComponent<Spawner> ();

		InvokeRepeating("CheckPlayerHasSeenTransport", 17, 1);
		InvokeRepeating("CheckPlayerAndConvoyLocation", 10, 4);
	}

	void OnEnable()
	{
		_battleEventManager.playerLeavingByWarp += TidyUpAsPlayerLeaves;
		_battleEventManager.playerShotDown += PlayerWasShotDown;
		_battleEventManager.playerRescued += PlayerWasRescued;
		_battleEventManager.playerBeganDocking += PlayerBeganDocking;
	}

	void OnDisable()
	{
		_battleEventManager.playerLeavingByWarp -= TidyUpAsPlayerLeaves;
		_battleEventManager.playerShotDown -= PlayerWasShotDown;
		_battleEventManager.playerRescued -= PlayerWasRescued;
		_battleEventManager.playerBeganDocking -= PlayerBeganDocking;
	}

	void Start()
	{
		AudioMasterScript.instance.MuteSFX ();
		ClickToPlay.instance.escCanGiveQuitMenu = false;
		instructionsPanel.SetActive (true);
		Tools.instance.AlterTimeScale(0);
		ClickToPlay.instance.paused = true;

		convoyPointerArrow.SetActive(false);

		Tools.instance.MoveCanvasToFront(demoStuffCanvas);
	}


	public void StartLevel()
	{
		AudioMasterScript.instance.ClearAll ();

		//for warp in
		transport.transform.position = startPos;
		AITrans = transport.GetComponent<AITransport> ();
		AITrans.theCaller = GameObject.FindGameObjectWithTag ("AIManager");
		AITrans.ChangeToNewState (AITransport.StateMachine.WarpIn);
		AITrans.thisWasInitialInsertionJump = true;
		AITrans.waypoint = Vector2.zero;
		AITrans.warpInTime = 3;
		AITrans.reelingInPlayerGroup = true;
		AITrans.SetUpReferences ();

		AITrans.InstantAttachFighters (playerGroup.ToArray());

		//for other
		equipPanel.SetActive (false);
		ClickToPlay.instance.escCanGiveQuitMenu = true;
		instructionsPanel.SetActive (false);
		Tools.instance.AlterTimeScale(1);
		ClickToPlay.instance.paused = false;
		playerWeapons.InvokeAllowedToFire ();
		objectiveText.enabled = true;
		CharacterPoolAvatarsForDemo();

		playerUICanvas.sortingOrder = 0;
	}


	public void CharacterPoolAvatarsForDemo()
	{
		bool changeAvatar1 = false;
		bool changeAvatar2 = false;

		if(PlayerPrefsManager.GetCharacterPoolUsageKey() == "Random & Character Pool")
		{
			if(Random.Range(0, 2) == 1) //50:50 chance to bring in a character from the pool
				changeAvatar1 = true;
			if(Random.Range(0, 2) == 1)
				changeAvatar2 = true;
		}
		else if(PlayerPrefsManager.GetCharacterPoolUsageKey() == "Character Pool Only")
		{
			changeAvatar1 = true;
			changeAvatar2 = true;
		}
		else if(PlayerPrefsManager.GetCharacterPoolUsageKey() == "Random Only")
		{
			//do nothing. leave as they are
			return;
		}
		else
		{
			Debug.LogError("Something went wrong with PlayerPrefsManager - CharacterPoolBehaviour");
			return;
		}

		//get the two avatars
		Character[] allWingmenCharacterScripts = FindObjectsOfType<Character>();

		//get all possible characters in the pool

		string[] allIDs = new string[] {};
		if (ES2.Exists ("allCharacterIDs" + CharacterPool.encryption)) 
		{
			allIDs = ES2.Load<string> ("allCharacterIDs" + CharacterPool.encryption)
				.Split (new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);
		}
		else 
		{
			Debug.Log ("allCharacterIDs file did NOT exist. NO characters to load.");
		}

		//put two random ones in
		int choice1 = Random.Range(0, allIDs.Length);
		int choice2 = choice1;

		if(allIDs.Length > 1)
		{
			while(choice2 == choice1)
			{
				choice2 = Random.Range(0, allIDs.Length);
			}
		}

		if(changeAvatar1 && allIDs.Length != 0)
		{
			string characterInfo = ES2.Load<string>(allIDs[choice1] + CharacterPool.encryption);
			SetUpSpecificAvatar(characterInfo, allWingmenCharacterScripts[0]);
		}
		if(changeAvatar2 && choice2 != choice1)
		{
			string characterInfo = ES2.Load<string>(allIDs[choice2] + CharacterPool.encryption);
			SetUpSpecificAvatar(characterInfo, allWingmenCharacterScripts[1]);
		}

		//put their callsigns over their heads

	}

	public void SetUpSpecificAvatar(string characterInfo, Character character)
	{
		string[] parameters = new string[]{"ID:","FN:","LN:","CS:", "BIO:", "APP:"};
		string[] savedData = characterInfo.Split(parameters, System.StringSplitOptions.None);

		character.characterID = savedData[1];
		character.firstName = savedData[2];
		character.lastName = savedData[3];
		character.callsign = savedData[4];
		character.characterBio = savedData[5];
		character.appearanceSeed = savedData[6];
		character.myAIFighterScript.nameHUDText.text = character.callsign;

		character.GenerateAppearanceBySeed(character.appearanceSeed.ToCharArray());
	}


	public void CraftEquipScreen()
	{
		equipPanel.SetActive (true);
	}
		

	public void DodgeInstructions()
	{
		tutorialHeaderText.text = "DODGING";
		tutorialBodyText.text = dodgeTutorialString;
	}

	void CheckPlayerAndConvoyLocation()
	{
		if(objectivesComplete || missionIsOver)
		{
			CancelInvoke("CheckPlayerLocation");
			convoyPointerArrow.SetActive(false);
			return;
		}

		//tell player to turn around if too far from convoy
		if(Vector2.Distance(player.transform.position, convoy.position) > convoyGuardDistance)
		{
			Subtitles.instance.PostHint(new string[] {"Return to the Convoy!"});
			convoyPointerArrow.SetActive(true);
		}
		else
		{
			convoyPointerArrow.SetActive(false);
		}

		//mark objectives complete if convoy is out of asteroid field
		if(!asteroidField.bounds.Contains(convoy.position))
		{
			objectivesComplete = true;
			clearedMissionObjectiveAtThisTime = timer;

			PlayEndAudio();
			Subtitles.instance.PostSubtitle(new string[] {"Convoy: \"Thanks for the escort, Arrow Squadron. We should be alright from here\"."});

			spawnerScript.CancelInvoke("Spawn");
			enemyCommander.CallFullRetreat();
			spawnerScript.enabled = false;
		}
	}

	void Update () 
	{
		if(!playerHealth.dead)
		{
			DoHints ();

			timer += Time.deltaTime;
		}
			
			
		//for mission insertion

		if(AITrans != null && AITrans.currentState == AITransport.StateMachine.HoldingPosition && timer >5 && !setWingmanOrders)
		{
			ToggleSquadUI();
			mate2AIScript.ChangeToNewState(new AIFighter.StateMachine[]{AIFighter.StateMachine.Covering}, new float[]{1});
			mate3AIScript.ChangeToNewState(new AIFighter.StateMachine[]{AIFighter.StateMachine.Covering}, new float[]{1});
			setWingmanOrders = true;
		}

		if(AITrans != null && AITrans.currentState == AITransport.StateMachine.HoldingPosition && timer > 8)
		{
			AITrans.currentState = AITransport.StateMachine.WarpOut;
		}


		//for ending the demo with kill limit
	/*	if(playerLogicScript.kills >= killLimitForDemo && clearedKillLimitAtThisTime == Mathf.Infinity)
		{
			clearedKillLimitAtThisTime = timer;
			spawnerScript.CancelInvoke("Spawn");
			enemyCommander.CallFullRetreat();
			Invoke("PlayEndAudio", 2);
		}
		else if(!postedEndMessage && !missionIsOver && playerLogicScript.kills >= killLimitForDemo && timer > clearedKillLimitAtThisTime + 5)
		{
			Subtitles.instance.PostHint(new string[] {"You've taken out enough pirates to make them think twice. Call for EXTRACTION with the RADIO, " +
				"then dock to complete the mission."});
			postedEndMessage = true;
		}

		if(timer > clearedKillLimitAtThisTime + 25)
		{
			postedEndMessage = false;
			clearedKillLimitAtThisTime = timer;
		}*/

		//for ending Demo by convoy location
		if(!postedEndMessage && !missionIsOver && objectivesComplete && timer > clearedMissionObjectiveAtThisTime + 5)
		{
			Subtitles.instance.PostHint(new string[] {"The convoy is safe. Call for EXTRACTION with the RADIO, " +
				"then dock to complete the mission."});
			postedEndMessage = true;
		}
		if(timer > clearedMissionObjectiveAtThisTime + 25)
		{
			postedEndMessage = false;
			clearedMissionObjectiveAtThisTime = timer;
		}

		if(timePlayerStartedLeaving != 0)
		{
			objectiveText.color = Color.Lerp(Color.white, Color.clear, (timer - timePlayerStartedLeaving)/ 2);
		}
	}//end of Update

	void PlayEndAudio()
	{
		GetComponent<AudioSource>().Play();
		//Subtitles.instance.PostSubtitle (new string[]{"Convoy: \"They're retreating! Thanks for the help!\""});
	}

	void DoHints ()
	{
		if(!Tools.instance.useHintsThisSession)
		{
			if(!spawnerScript.enabled && timer >= 5)
				spawnerScript.enabled = true;			

			return;
		}
			
		//for HINTS

		//these set the hints to KNOWN
		if (!playerKnowsHowToMove && Input.GetAxis ("Accelerate") != 0)
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
		if (!missionIsOver && !objectivesComplete && !playerKnowsHowToMove && timer > 9) 
		{
			if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint (new string[] {
					"Press UP ARROW to ACCELERATE"
				});
			else
				if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
					Subtitles.instance.PostHint (new string[] {
						"Press RIGHT TRIGGER to ACCELERATE"
					});
			Subtitles.instance.CoolDownHintNoise ();
			Subtitles.instance.CoolDownHintHighlight ();
		}
		else if (!missionIsOver && !objectivesComplete  && playerKnowsHowToMove && !playerKnowsHowToShoot && timer > 14) 
		{
				if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
					Subtitles.instance.PostHint (new string[] {
						"Press SPACEBAR to SHOOT"
					});
				else
					if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
						Subtitles.instance.PostHint (new string[] {
							"Press A to SHOOT"
						});
				Subtitles.instance.CoolDownHintNoise ();
				Subtitles.instance.CoolDownHintHighlight ();
		}
		else if (!missionIsOver && !objectivesComplete  && playerKnowsHowToMove && playerKnowsHowToShoot && !playerKnowsHowToDodge && timer > 21) 
		{
				if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
					Subtitles.instance.PostHint (new string[] {
						"Press LEFT CTRL to DODGE"
					});
				else
					if (InputManager.instance.inputFrom == InputManager.InputFrom.controller)
						Subtitles.instance.PostHint (new string[] {
							"Press B to DODGE"
						});
				Subtitles.instance.CoolDownHintNoise ();
				Subtitles.instance.CoolDownHintHighlight ();
		}
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
		else if (!missionIsOver && !playerKnowsDocking && 
			timeWhenFirstSawFighterTransportPickup > 0 && timer > timeWhenFirstSawFighterTransportPickup + 7) 
		{
			Subtitles.instance.PostHint (new string[]
				{"To DOCK, fly over the Transport and then respond to the new RADIO command." + " This is timed. Try again if you miss."});
							Subtitles.instance.CoolDownHintNoise ();
							Subtitles.instance.CoolDownHintHighlight ();
		}

		//to start the enemies spawning once you know how to move
		if (!spawnerScript.enabled && !objectivesComplete && playerKnowsHowToMove && playerKnowsHowToShoot && playerKnowsHowToDodge) {
			spawnerScript.enabled = true;
		}
	}

	void CommenceFadeout()
	{
		AudioMasterScript.instance.MuteMusic ();
		Tools.instance.CommenceFade (0, 3, Color.clear, Color.black);
	}

	void PostMissionCompleteMessage()
	{
		if(objectivesComplete || player.GetComponent<PlayerAILogic>().kills >= killLimitForDemo)
		{
			Subtitles.instance.PostHint (new string[]{"---MISSION COMPLETE---"});
		}
		else
		{
			Subtitles.instance.PostHint (new string[]{"You abandoned the convoy. You will not be paid for this mission."});
		}
		Subtitles.instance.gameObject.GetComponentInParent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
		Subtitles.instance.gameObject.GetComponentInParent<Canvas> ().sortingOrder = 12;

		AudioMasterScript.instance.MuteMusic();
	}


	public void ChangeControlsImage(string which)
	{
		controlsAtStartImage.sprite = ClickToPlay.instance.controlsImage.sprite;

		if(which == "StickRotates")
		{
			keyboardControlsText.text = startKeyboardText;
		}
		else if(which == "StickPoints")
		{
			keyboardControlsText.text = "KEYBOARD\n\n" +
				"This mode is NOT designed for keyboard. Do not use!";
		}
	}

	public void ShowGamepadControlsAtStart()
	{
		ChangeControlsImage (PlayerPrefsManager.GetControllerStickBehaviourKey());
		controlsAtStartImage.enabled = true;
		instructionsText.gameObject.SetActive (false);
	}

	public void ShowKeyboardControlsAtStart()
	{
		instructionsText.gameObject.SetActive(true);
		instructionsText.text = keyboardControlsText.text;
		controlsAtStartImage.enabled = false;
	}

	void ActivateDeathButtons()
	{
		Button[] buttons = deathPanel.transform.GetComponentsInChildren<Button> ();
		foreach(Button button in buttons)
		{
			button.interactable = true;
		}
	}

	void MissionCompleteScreen()
	{
		AudioMasterScript.instance.MuteSFX ();
		missionCompleteMenu.SetActive (true);
		missionCompleteMenu.GetComponentInParent<Canvas>().sortingOrder += 10;
		Invoke ("LoadAutoPlayLevel", 20f);
	}

	public void LoadAutoPlayLevel()
	{
		SceneManager.LoadScene (levelToLoad);
	}

	void FadeOutAudio()
	{
		AudioMasterScript.instance.FadeChannel("SFX vol", -80, 6, 6);
	}

	void PlayerWasShotDown()
	{
	}
	void PlayerWasRescued()
	{
		deathPanel.SetActive(true);
		deathPanel.GetComponentInParent<Canvas>().sortingOrder += 10;
		missionIsOver = true;

		Invoke("ActivateDeathButtons", 1f);
		//Invoke("CommenceFadeout", waitTimeAfterDeath - 4);
		Invoke("LoadAutoPlayLevel", waitTimeAfterDeath);
		FadeOutAudio();
	}
	void CheckPlayerHasSeenTransport()
	{
		if(FindObjectOfType<AITransport>() != null)
		{
			if(GameObject.Find("Transport 1") != null)
			{
				if(Vector2.Distance(GameObject.Find("Transport 1").transform.position, GameObject.FindGameObjectWithTag("PlayerFighter").transform.position)
					< 15)
				{
					timeWhenFirstSawFighterTransportPickup = Time.time;
					CancelInvoke("CheckPlayerHasSeenTransport");
				}
			}
		}
	}
	void PlayerBeganDocking()
	{
		playerKnowsDocking = true;
		playerKnowsOrders = true;
		playerKnowsMap = true;
	}
	
	void TidyUpAsPlayerLeaves()
	{
		timePlayerStartedLeaving = timer;

		ToggleSquadUI();
		missionIsOver = true;
		Invoke("PostMissionCompleteMessage", 6);
		playerLogicScript.orders = PlayerAILogic.Orders.NA;
		Invoke("MissionCompleteScreen", 10.5f);
	}

	void ToggleSquadUI()
	{
		playerWorldspaceUI.enabled = !playerWorldspaceUI.enabled;
		arrow2WorldspaceUI.enabled = !arrow2WorldspaceUI.enabled;
		arrow3WorldspaceUI.enabled = !arrow3WorldspaceUI.enabled;
	}

	public void LeaveFeedback()
	{
		Application.OpenURL("https://docs.google.com/forms/d/1ZDuBgmUohSbOX_ytS6N0cHFdVfRdNzQwjH7PsJhGpj4/viewform");
	}

}//Mono

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoLevel : MonoBehaviour {

	HealthFighter playerHealth;
	AITransport AITrans;
	Spawner spawnerScript;
	AICommander enemyCommander;

	public AIFighter mate2AIScript;
	public AIFighter mate3AIScript;

	public int killLimitForDemo = 4;

	public Text playerKillsText;
	public Text missileAmmoText;
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
	public Canvas playerUICanvas;
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
	bool playerKnowsOrders = false;
	bool playerKnowsMap = false;
	bool postedEndMessage = false;

	bool setWingmanOrders = false;

	[Header("For warp in")]
	public GameObject transport;
	public List<GameObject> playerGroup;
	[HideInInspector] public Vector2 startPos;

	bool missionComplete = false;
	float clearedKillLimitAtThisTime = Mathf.Infinity;

	[Header("End of level stuff")]
	public string levelToLoad = "";
	public float waitTimeAfterDeath = 20;
	float timePlayerStartedLeaving = 0;

	
	void Awake()
	{
		player = GameObject.FindGameObjectWithTag ("PlayerFighter");
		player.GetComponentInChildren<WeaponsPrimaryFighter> ().allowedToFire = false;

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
	}

	void OnEnable()
	{
		_battleEventManager.playerLeaving += TidyUpAsPlayerLeaves;
	}

	void OnDisable()
	{
		_battleEventManager.playerLeaving -= TidyUpAsPlayerLeaves;
	}

	void Start()
	{
		AudioMasterScript.instance.MuteSFX ();
		ClickToPlay.instance.escGivesQuitMenu = false;
		instructionsPanel.SetActive (true);
		Time.timeScale = 0;
		ClickToPlay.instance.paused = true;

		if(GameObject.FindGameObjectWithTag("PlayerFighter").GetComponentInChildren<WeaponsSecondaryFighter>() == null)
		{
			missileAmmoText.gameObject.SetActive(false);
		}

		Tools.instance.blackoutPanel.GetComponentInParent<Canvas> ().sortingOrder = -1;
		Tools.instance.CommenceFadeIn ();
	}


	public void StartLevel()
	{
		AudioMasterScript.instance.ClearAll ();

		//for warp in
		transport.transform.position = startPos;
		AITrans = transport.GetComponent<AITransport> ();
		AITrans.theCaller = GameObject.FindGameObjectWithTag ("AIManager");
		AITrans.ChangeToNewState (AITransport.StateMachine.warpIn);
		AITrans.thisWasInitialInsertionJump = true;
		AITrans.waypoint = Vector2.zero;
		AITrans.warpInTime = 3;
		AITrans.reelingInPlayerGroup = true;
		AITrans.SetUpReferences ();

		AITrans.InstantAttachFighters (playerGroup [0], playerGroup [1], playerGroup [2]);

		//for other
		equipPanel.SetActive (false);
		ClickToPlay.instance.escGivesQuitMenu = true;
		instructionsPanel.SetActive (false);
		Time.timeScale = 1;
		ClickToPlay.instance.paused = false;
		player.GetComponentInChildren<WeaponsPrimaryFighter> ().InvokeAllowedToFire ();
		objectiveText.enabled = true;

		playerUICanvas.sortingOrder = 0;
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

	void Update () 
	{
		//for HINTS
		if (!playerKnowsHowToMove && Input.GetAxis ("Accelerate") != 0)
			playerKnowsHowToMove = true;
		if (!playerKnowsHowToShoot && player.GetComponentInChildren<WeaponsPrimaryFighter> ().allowedToFire
		    && player.GetComponentInChildren<WeaponsPrimaryFighter> ().enabled && Input.GetButtonDown ("FirePrimary"))
			playerKnowsHowToShoot = true;
		if (!playerKnowsHowToDodge && Input.GetButton ("Dodge"))
			playerKnowsHowToDodge = true;
		if (!playerKnowsHowToAfterburn && Input.GetButton ("Afterburners"))
			playerKnowsHowToAfterburn = true;
		if(!playerKnowsOrders && RadioCommands.instance.buttonsShown)
			playerKnowsOrders = true;
		if (!playerKnowsMap && tacMapCamera.enabled)
			playerKnowsMap = true;

		if(!missionComplete && !playerKnowsHowToMove && timer > 9) 
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Press UP ARROW to ACCELERATE"});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Press RIGHT TRIGGER to ACCELERATE"});			
			Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}
		else if(!missionComplete && playerKnowsHowToMove && !playerKnowsHowToShoot && timer > 14)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Press SPACEBAR to SHOOT"});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Press A to SHOOT"});	
			Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}
		else if (!missionComplete && playerKnowsHowToMove && playerKnowsHowToShoot && !playerKnowsHowToDodge && timer > 21)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Press LEFT CTRL to DODGE"});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Press B to DODGE"});				
			Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}
		else if (!missionComplete && playerKnowsHowToMove && playerKnowsHowToShoot && playerKnowsHowToDodge && !playerKnowsHowToAfterburn && timer > 27)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Subtitles.instance.PostHint(new string[] {"Hold LEFT SHIFT while accelerating for AFTERBURNERS"});
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				Subtitles.instance.PostHint(new string[] {"Hold X while accelerating for AFTERBURNERS"});
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
				Subtitles.instance.PostHint(new string[] {"Press BACK to view the TACTICAL MAP"});				Subtitles.instance.CoolDownHintNoise();
			Subtitles.instance.CoolDownHintHighlight();
		}

		if(playerKnowsHowToMove && playerKnowsHowToShoot && playerKnowsHowToDodge)
		{
			spawnerScript.enabled = true;
		}



		//for other stuff
		if(!playerHealth.dead)
		{
			timer += Time.deltaTime;
		}
		else if(playerHealth.dead && deathPanel.activeSelf == false)
		{
			deathPanel.SetActive(true);
			missionComplete = true;

			Invoke("ActivateDeathButtons", 1f);
			Invoke("CommenceFadeout", waitTimeAfterDeath - 4);
			Invoke("LoadAutoPlayLevel", waitTimeAfterDeath);
		}

		playerKillsText.text = "Kills: " + playerLogicScript.kills;

		//for mission insertion

		if(AITrans != null && AITrans.currentState == AITransport.StateMachine.holdingPosition && timer >5 && !setWingmanOrders)
		{
			mate2AIScript.ChangeToNewState(new AIFighter.StateMachine[]{AIFighter.StateMachine.Covering}, new float[]{1});
			mate3AIScript.ChangeToNewState(new AIFighter.StateMachine[]{AIFighter.StateMachine.Covering}, new float[]{1});
			setWingmanOrders = true;
		}

		if(AITrans != null && AITrans.currentState == AITransport.StateMachine.holdingPosition && timer > 8)
		{
			AITrans.currentState = AITransport.StateMachine.warpOut;
		}

		//for mission extraction

		if(playerLogicScript.orders == PlayerAILogic.Orders.RTB)
		{
			missionComplete = true;
			Invoke("PostMissionCompleteMessage", 6);
			playerLogicScript.orders = PlayerAILogic.Orders.NA;
			Invoke("MissionCompleteScreen", 10.5f);
		}

		//for ending the demo with kill limit
		if(playerLogicScript.kills >= killLimitForDemo && clearedKillLimitAtThisTime == Mathf.Infinity)
		{
			clearedKillLimitAtThisTime = timer;
			spawnerScript.CancelInvoke("Spawn");
			enemyCommander.CallFullRetreat();
			Invoke("PlayEndAudio", 2);
		}
		else if(!postedEndMessage && !missionComplete && playerLogicScript.kills >= killLimitForDemo && timer > clearedKillLimitAtThisTime + 5)
		{
			Subtitles.instance.PostHint(new string[] {"You've taken out enough pirates to make them think twice. Call for EXTRACTION with the RADIO, " +
				"then dock to complete the mission."});
			postedEndMessage = true;
		}

		if(timer > clearedKillLimitAtThisTime + 25)
		{
			postedEndMessage = false;
			clearedKillLimitAtThisTime = timer;
		}

		if(timePlayerStartedLeaving != 0)
		{
			objectiveText.color = Color.Lerp(Color.white, Color.clear, (timer - timePlayerStartedLeaving)/ 2);
		}
	}//end of Update

	void PlayEndAudio()
	{
		GetComponent<AudioSource>().Play();
		Subtitles.instance.PostSubtitle (new string[]{"Convoy: \"They're retreating! Thanks for the help!\""});
	}

	void CommenceFadeout()
	{
		AudioMasterScript.instance.MuteMusic ();

		Tools.instance.CommenceFadeout (3);
	}

	void PostMissionCompleteMessage()
	{
		if(player.GetComponent<PlayerAILogic>().kills >= killLimitForDemo)
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

	public void Survey()
	{
		Application.OpenURL("https://docs.google.com/forms/d/1ZDuBgmUohSbOX_ytS6N0cHFdVfRdNzQwjH7PsJhGpj4/viewform");
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
		Invoke ("LoadAutoPlayLevel", 20f);
	}

	public void LoadAutoPlayLevel()
	{
		SceneManager.LoadScene (levelToLoad);
	}

	
	void TidyUpAsPlayerLeaves()
	{
		timePlayerStartedLeaving = timer;
	}

	public void LeaveFeedback()
	{
		Application.OpenURL("https://docs.google.com/forms/d/1ZDuBgmUohSbOX_ytS6N0cHFdVfRdNzQwjH7PsJhGpj4/viewform");
	}

}//Mono

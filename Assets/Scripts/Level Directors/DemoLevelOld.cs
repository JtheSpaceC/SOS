using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoLevelOld : MonoBehaviour {

	HealthFighter playerHealth;
	AITransport AITrans;
	Spawner spawnerScript;
	AICommander enemyCommander;

	public AIFighter mate2AIScript;
	public AIFighter mate3AIScript;

	public int killLimitForDemo = 4;

	public Text playerKills;
	public Text gameTime;
	public Text deathScoreText;
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
	public Image controlsImage;
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
	public Button restartAtWinButton;

	string cheatCode;

	[Header("Craft Equip Stuff")]
	public GameObject playerMissiles;

	public Text explanatoryText;
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

		ChangeControlsImage (PlayerPrefs.GetString("Control Scheme"));

		feedbackButton.gameObject.SetActive (true);
		Navigation navUp = resumeButton.navigation;
		navUp.selectOnUp = feedbackButton;
		resumeButton.navigation = navUp;

		Navigation navUp2 = quitButton.navigation;
		navUp2.selectOnUp = feedbackButton;
		quitButton.navigation = navUp2;

		Navigation navDown = controlsButton.navigation;
		navDown.selectOnDown = feedbackButton;
		controlsButton.navigation = navDown;

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
		buttonToStartOn.Select();

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
		defaultEquipButton.Select();
	}


	public void ChooseUpgrade(int whichUpgrade)
	{
		finalLaunchButton.Select ();

		switch (whichUpgrade) {
		default: Debug.Log("Invalid update selected");
			break;
		case 1:
			playerHealth.playerHasAutoDodge = true;
			playerHealth.maxHealth = 100;
			playerHealth.health = 100;
			player.GetComponent<Rigidbody2D>().mass = 1;
			missileAmmoText.gameObject.SetActive(false);
			playerMissiles.SetActive(false);
			break;
		case 2:
			playerHealth.playerHasAutoDodge = false;
			playerHealth.maxHealth = 150;
			playerHealth.health = 150;
			player.GetComponent<Rigidbody2D>().mass = 1.2f;
			missileAmmoText.gameObject.SetActive(false);
			playerMissiles.SetActive(false);
			break;
		case 3:
			playerHealth.playerHasAutoDodge = false;
			playerHealth.maxHealth = 100;
			playerHealth.health = 100;
			player.GetComponent<Rigidbody2D>().mass = 1;
			missileAmmoText.gameObject.SetActive(true);
			playerMissiles.SetActive(true);
			break;
		}
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
			Subtitles.instance.PostHint(new string[] {"Press RT (on controller) or the Up arrow to ACCELERATE."});
			Subtitles.instance.CoolDownHintNoise();
		}
		else if(!missionComplete && playerKnowsHowToMove && !playerKnowsHowToShoot && timer > 14)
		{
			Subtitles.instance.PostHint(new string[] {"Press A (on controller) or Spacebar to SHOOT."});
			Subtitles.instance.CoolDownHintNoise();
		}
		else if (!missionComplete && playerKnowsHowToMove && playerKnowsHowToShoot && !playerKnowsHowToDodge && timer > 21)
		{
			Subtitles.instance.PostHint(new string[] {"Press B (on controller) or Left Ctrl to DODGE enemy fire or asteroids."});
			Subtitles.instance.CoolDownHintNoise();
		}
		else if (!missionComplete && playerKnowsHowToMove && playerKnowsHowToShoot && playerKnowsHowToDodge && !playerKnowsHowToAfterburn && timer > 27)
		{
			Subtitles.instance.PostHint(new string[] {"Hold X (on controller) or Left Shift to use AFTERBURNERS."});
			Subtitles.instance.CoolDownHintNoise();
		}
		else if(!missionComplete && !playerKnowsOrders && timer > 60 && Subtitles.instance.hintsPanel.color == Color.clear)
		{
			Subtitles.instance.PostHint(new string[] {"Experiment with the RADIO to give orders to wingmen or call for EXTRACTION.",
				"Use the D pad (on controller) or numbers 1,2,3,4 for up, down, left and right RADIO commandds."});
			Subtitles.instance.CoolDownHintNoise();
		}
		else if(!missionComplete && !playerKnowsMap && timer > 90)
		{
			Subtitles.instance.PostHint(new string[] {"Bring up the TACTICAL MAP by pressing TAB, M, or BACK (on controller) "});
			Subtitles.instance.CoolDownHintNoise();
		}

		//for CHEATCODE
		if(Input.GetKeyDown(KeyCode.R))
		{
			cheatCode = "r";
			Invoke("ResetCheatCode", 4f);
		}
		if(cheatCode == "r" && Input.GetKeyDown(KeyCode.E))
		{
			cheatCode = "re";
		}
		if(cheatCode == "re" && Input.GetKeyDown(KeyCode.S))
		{
			cheatCode = "res";
		}
		if(cheatCode == "res" && Input.GetKeyDown(KeyCode.E))
		{
			cheatCode = "rese";
		}
		if(cheatCode == "rese" && Input.GetKeyDown(KeyCode.T))
		{
			cheatCode = "reset";			
			ResetHighScore();
		}

		//for other stuff
		if(!playerHealth.dead)
		{
			timer += Time.deltaTime;
		}
		else if(playerHealth.dead && deathPanel.activeSelf == false)
		{
			deathPanel.SetActive(true);
			restartAtDeathButton.Select();
			missionComplete = true;

			int playerScore = player.GetComponent<PlayerAILogic>().kills;

			if(playerScore > PlayerPrefs.GetInt("HighScore"))
			{
				PlayerPrefs.SetInt("HighScore", playerScore);
				PlayerPrefs.Save();
			}
			deathScoreText.gameObject.SetActive(true);
			deathScoreText.text = "Your Score: " + playerScore + "\n" +
				"High Score: " + PlayerPrefs.GetInt("HighScore");

			Invoke("ActivateDeathButtons", 1f);
			Invoke("CommenceFadeout", waitTimeAfterDeath - 4);
			Invoke("LoadAutoPlayLevel", waitTimeAfterDeath);
		}

		playerKills.text = "Kills: " + playerLogicScript.kills;

		float mins = Mathf.FloorToInt (timer / 60);

		float seconds = Mathf.FloorToInt(timer);

		while (seconds >= 60)
		{
			seconds -= 60;
		}
		string minsString = mins < 10 ? "0" + mins.ToString () : mins.ToString ();
		string secsString = seconds < 10 ? "0" + seconds.ToString () : seconds.ToString ();

		gameTime.text = "Time: " + minsString + ":" + secsString;

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
		Subtitles.instance.PostHint (new string[]{"---MISSION COMPLETE---"});
		Subtitles.instance.gameObject.GetComponentInParent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
		Subtitles.instance.gameObject.GetComponentInParent<Canvas> ().sortingOrder = 12;

		try{
			MusicManager.instance.muteMusic = true;}catch{}
	}


	public void ChangeControlsImage(string which)
	{
		if(which == "StickRotates")
		{
			controlsImage.sprite = controls1;
			keyboardControlsText.text = startKeyboardText;
		}
		else if(which == "StickPoints")
		{
			controlsImage.sprite = controls2;
			keyboardControlsText.text = "KEYBOARD\n\n" +
				"This mode is NOT designed for keyboard. Do not use!";
		}
		backButton.Select();
	}

	public void ShowGamepadControlsAtStart()
	{
		Sprite imageToShow = PlayerPrefs.GetString ("Control Scheme") == "StickPoints" ? controls2 : controls1;
		controlsAtStartImage.sprite = imageToShow;
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

	public void ResetHighScore()
	{
		PlayerPrefs.SetInt ("HighScore", 0);
		PlayerPrefs.Save ();
	}

	void ResetCheatCode()
	{
		cheatCode = "";
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
		restartAtWinButton.Select ();
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

}//Mono

using UnityEngine;
using UnityEngine.UI;

public class Director : MonoBehaviour {

	public static Director instance;

	MissionSetup missionSetupScript;

	public float timer;
	float mins;
	float seconds;

	Text gameTimeText;
	Text playerKillsText;

	public GameObject pilotEVAPrefab;

	[HideInInspector] public int playerKills = 0;
	[HideInInspector] public int playerMissileKills = 0;
	[HideInInspector] public int numberOfManualDodges = 0;
	[HideInInspector] public int numberOfSuccessfulDodges = 0;
	[HideInInspector] public int numberOfAutomatedDodges = 0;
	[HideInInspector] public int numberOfSpecialsUsed = 0;

	[HideInInspector] public float timeUntilFirstKill = 0;

	[HideInInspector] public int radioButtonPresses = 0;
	[HideInInspector] public int tacMapUses = 0;


	[Tooltip("Just for turning something on/of in the scene for testing. Like a background or spawner. Just one object.")]
	public bool screenshotMode = false;
	public GameObject toggleableObject9;
	public GameObject toggleableObject8;
	public GameObject toggleableObject7;
	public GameObject toggleableObject6;
	public GameObject toggleableObject5;
	public GameObject toggleableObject4;
	public GameObject toggleableObject3;
	public GameObject toggleableObject2;
	public GameObject toggleableObject1;


	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		}
		else
		{
			Debug.LogError("There were two Directors. Deleting one");
			Destroy(this.gameObject);
		}

		try{
		gameTimeText = GameObject.Find("GUI Mission Time").GetComponent<Text>();
		playerKillsText = GameObject.Find("GUI Kills").GetComponent<Text>();
		}catch{}
	}

	void Start()
	{
		if(FindObjectOfType<MissionSetup>())
		{
			missionSetupScript = FindObjectOfType<MissionSetup>();
			SetUpMission();
		}
	}

	void OnEnable()
	{
		_battleEventManager.playerRescued += WarpPlayerToSafety;
		_battleEventManager.playerGotKill += PlayerGotAKill;
	}

	void OnDisable()
	{
		_battleEventManager.playerRescued -= WarpPlayerToSafety;
		_battleEventManager.playerGotKill -= PlayerGotAKill;
	}

	void SetUpMission()
	{
		//first make sure there's no conflicting info in the MissionSetup script, like no transport to warp in, but WarpIn selected
		missionSetupScript.ValidateChoices(); 

		//set a background

		//set a player craft, their weapons, avatar, etc

		GameObject player = Instantiate (missionSetupScript.playerCraft.shipType) as GameObject;
		player.name = "1 - " + missionSetupScript.playerCraft.callSign;

		//set player squadmates, avatars, names, skills etc

		for(int i = 0; i < missionSetupScript.playerSquad.Count; i++)
		{
			GameObject squadmate = Instantiate(missionSetupScript.playerSquad[i].shipType) as GameObject;
			squadmate.name = (i+1) + " - " + missionSetupScript.playerSquad[i].callSign;
			player.GetComponentInChildren<SquadronLeader>().activeWingmen.Add(squadmate);
		}

		//set starting positions for all craft

		//set mission type

		//set any waypoints, asteroids, and mines

		//set any discover-ables

		//set bases, cap ships, or transports

		//maybe generate any RandomNumberGenerator results and save them

		//set screen black and prepare to fade it in
		Tools.instance.CommenceFadeIn(0, 2);
	}

	void Update () 
	{
		if(!ClickToPlay.instance.paused)
		{
			if(gameTimeText && playerKillsText)
			{
				//Mission Clock Stuff

				timer += Time.deltaTime;
				mins = Mathf.FloorToInt (timer / 60);
				seconds = Mathf.FloorToInt(timer);

				while (seconds >= 60)
				{
					seconds -= 60;
				}
				string minsString = mins < 10 ? "0" + mins.ToString () : mins.ToString ();
				string secsString = seconds < 10 ? "0" + seconds.ToString () : seconds.ToString ();

				gameTimeText.text = "Time: " + minsString + " : " + secsString;
				playerKillsText.text = "Kills: " + playerKills;
			}

			#if UNITY_EDITOR

			if(Input.GetKeyDown(KeyCode.Delete))
				GameObject.FindGameObjectWithTag("PlayerFighter").GetComponent<HealthFighter>().health = 0;
			
			#endif

			if(!screenshotMode)
				return;
			
			if(Input.GetKeyDown(KeyCode.Keypad9) && toggleableObject9 != null)
				toggleableObject9.SetActive(!toggleableObject9.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad8) && toggleableObject8 != null)
				toggleableObject8.SetActive(!toggleableObject8.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad7) && toggleableObject7 != null)
				toggleableObject7.SetActive(!toggleableObject7.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad6) && toggleableObject6 != null)
				toggleableObject6.SetActive(!toggleableObject6.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad5) && toggleableObject5 != null)
				toggleableObject5.SetActive(!toggleableObject5.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad4) && toggleableObject4 != null)
				toggleableObject4.SetActive(!toggleableObject4.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad3) && toggleableObject3 != null)
				toggleableObject3.SetActive(!toggleableObject3.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad2) && toggleableObject2 != null)
				toggleableObject2.SetActive(!toggleableObject2.activeSelf);
			if(Input.GetKeyDown(KeyCode.Keypad1) && toggleableObject1 != null)
				toggleableObject1.SetActive(!toggleableObject1.activeSelf);
		}
	}
		

	public void SpawnPilotEVA(Vector2 pos, Quaternion rot, bool isPlayer)
	{
		GameObject pilot = Instantiate(pilotEVAPrefab, pos, rot) as GameObject;
		if(isPlayer)
		{
			pilot.tag = "PlayerEVA";
			pilot.name = "EVA Pilot";
			PMCMisisonSupports.instance.AutoRetrievePlayer();
			//StartCoroutine(Camera.main.GetComponent<CameraControllerFighter>().OrthoCameraZoomToSize(3, 1.5f, 6));
			StartCoroutine(Camera.main.GetComponent<CameraControllerFighter>().PerspectiveCamZoom(-5, 1.5f, 6));
		}
	}

	public void PlayerGotAKill()
	{
		playerKills ++;

		if(playerKills == 1)
		{
			timeUntilFirstKill = timer;
		}
	}

	public void WarpPlayerToSafety()
	{
		PMCMisisonSupports.instance.retrievalShuttle.GetComponent<AIAssaultShuttle>().
			ChangeToNewState(AIAssaultShuttle.StateMachine.WarpOut);	
	}
}

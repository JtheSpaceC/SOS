using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class Director : MonoBehaviour {

	public static Director instance;

	MissionSetup missionSetupScript;

	public float timer;
	float mins;
	float seconds;

	Text gameTimeText;
	Text playerKillsText;

	[Range(0, 100f)]
	public float chanceOfSlowMoDeath = 20f;

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

	public KeyCode controlOfCameraKey = KeyCode.F1;

	GameObject player;
	GameObject hangarToExit;

	public UnityEvent[] testActions;
	public KeyCode[] hotkeysForTestActions;

	[HideInInspector] public Color sceneTint;

	string canvasesOnOrOff = "";


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

		if(FindObjectOfType<MissionSetup>())
		{
			missionSetupScript = FindObjectOfType<MissionSetup>();
			SetUpMission();
		}

		//TODO: More dynamic system, based on world map location
		sceneTint = Tools.instance.environments.GetASceneColour();
	}

	void Start()
	{
		if(!FindObjectOfType<MissionSetup>())
		{
			//REMOVE: when mission setup dresses the scene always
			DressTheScene();
		}
			
		Tools.instance.CommenceFade(0, 2, Color.black, Color.clear);
	}

	void OnEnable()
	{
		_battleEventManager.playerRescued += WarpPlayerToSafety;
		_battleEventManager.playerGotKill += PlayerGotAKill;
		_battleEventManager.pmcFightersSpawned += AlliesSpawned;
		_battleEventManager.enemyFightersSpawned += EnemiesSpawned;
	}

	void OnDisable()
	{
		_battleEventManager.playerRescued -= WarpPlayerToSafety;
		_battleEventManager.playerGotKill -= PlayerGotAKill;
		_battleEventManager.pmcFightersSpawned -= AlliesSpawned;
		_battleEventManager.enemyFightersSpawned -= EnemiesSpawned;
	}
		
	void SetUpMission()
	{
		//first make sure there's no conflicting info in the MissionSetup script, like no transport to warp in, but WarpIn selected
		missionSetupScript.ValidateChoices(); 

		//set a background
		DressTheScene();

		//set a player craft, their weapons, avatar, etc

		if(missionSetupScript.playerCraft != null)
		{
			player = Instantiate (missionSetupScript.playerCraft.shipType) as GameObject;
			player.name = "1 - " + missionSetupScript.playerCraft.callSign;		

			//set player squadmates, avatars, names, skills etc

			if(missionSetupScript.playerSquad.Count != 0 )
			{
				for(int i = 0; i < missionSetupScript.playerSquad.Count; i++)
				{
					GameObject squadmate = Instantiate(missionSetupScript.playerSquad[i].shipType) as GameObject;
					squadmate.transform.position += (Vector3)Random.insideUnitCircle.normalized * 2;
					squadmate.name = (i+2) + " - " + missionSetupScript.playerSquad[i].callSign;
					player.GetComponentInChildren<SquadronLeader>().activeWingmen.Add(squadmate);
					squadmate.GetComponent<AIFighter>().ChangeToNewState(
						new AIFighter.StateMachine[]{AIFighter.StateMachine.Covering}, new float[] {1});
				}
			}
		}

		//set starting positions for all craft, don't worry about insertion points for craft about to attach to a warp or hangar ship

		for(int i = 0; i < missionSetupScript.pmcCraft.Count; i++)
		{
			GameObject pmcCraft = Instantiate(missionSetupScript.pmcCraft[i].shipType) as GameObject;
			pmcCraft.transform.position += missionSetupScript.pmcCraft[i].spawnPos;

			//make a record of which hangar we've to exit (in case that's the chosen insertion method)
			if(missionSetupScript.hangarCraft == missionSetupScript.pmcCraft[i]) 
			{
				hangarToExit = pmcCraft;
			}
		}

		for(int i = 0; i < missionSetupScript.enemyCraft.Count; i++)
		{
			GameObject enemy = Instantiate(missionSetupScript.enemyCraft[i].shipType) as GameObject;
			enemy.transform.position = missionSetupScript.enemyCraft[i].spawnPos;
		}

		for(int i = 0; i < missionSetupScript.civilianCraft.Count; i++)
		{

		}

		//set MISSION type

		//set INSERTION type

		if(missionSetupScript.insertionType == MissionSetup.InsertionType.AlreadyPresent)
		{//do nothing
		}
		else if(missionSetupScript.insertionType == MissionSetup.InsertionType.LeaveHangar)
		{
			player.transform.position = hangarToExit.GetComponent<SupportShipFunctions>().hangars[0].transform.position;
			player.transform.rotation = hangarToExit.GetComponent<SupportShipFunctions>().hangars[0].transform.rotation;
			StartCoroutine(PlayerShipLaunchFromHangar());

			for(int i = 0; i < missionSetupScript.playerSquad.Count; i++)
			{
				player.GetComponentInChildren<SquadronLeader>().activeWingmen[i].transform.position = 
					hangarToExit.GetComponent<SupportShipFunctions>().hangars[i+1].transform.position;
				player.GetComponentInChildren<SquadronLeader>().activeWingmen[i].transform.rotation = 
					hangarToExit.GetComponent<SupportShipFunctions>().hangars[i+1].transform.rotation;
				StartCoroutine
				(ShipLaunchFromHangar(player.GetComponentInChildren<SquadronLeader>().activeWingmen[i].GetComponent<AIFighter>()));
			}
		}
		else if(missionSetupScript.insertionType == MissionSetup.InsertionType.NotPresent)
		{
			player = null;
		}
		else if(missionSetupScript.insertionType == MissionSetup.InsertionType.RestInPosition)
		{
			Debug.LogError("Not Set up for this yet");
		}
		else if(missionSetupScript.insertionType == MissionSetup.InsertionType.WarpIn)
		{
			
		}

		//set CAMERA starting position

		if(missionSetupScript.playerCraft != null)
		{
			Vector3 camPos = Camera.main.transform.position;
			Camera.main.transform.position = (Vector2)player.transform.position;
			Camera.main.transform.position += new Vector3 (0, 0, camPos.z);
		}

		//set any waypoints, asteroids, and mines

		//set any discover-ables

		//set bases, cap ships, or transports

		//maybe generate any RandomNumberGenerator results and save them

		//set screen black and prepare to fade it in
		//(happens in Start())
	}

	void DressTheScene()
	{
		SpriteRenderer[] renderers = GameObject.Find("Fancy Background").GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer renderer in renderers)
		{
			if(renderer.name != "sun shaft")
				renderer.color = AdjustColour(renderer.color, sceneTint);
		}
		if(GameObject.Find("Asteroid Field"))
		{
			ParticleSystem ps = GameObject.Find("Asteroid Field").GetComponentInChildren<ParticleSystem>();
			ps.startColor = AdjustColour(ps.startColor, sceneTint);
			ps.gameObject.SetActive(false);
			ps.gameObject.SetActive(true);
		}
	}
	Color AdjustColour(Color oldColour, Color sceneTint)
	{
		return sceneTint * oldColour;
	}

	IEnumerator ShipLaunchFromHangar(AIFighter shipAI)
	{
		shipAI.healthScript.enabled = false;
		shipAI.enabled = false;
		float startingZ = shipAI.transform.position.z;

		yield return new WaitForSeconds(Random.Range(1.75f, 2.25f));

		float startTime = timer;

		while(timer < (startTime + 2))
		{
			shipAI.engineScript.MoveToTarget(shipAI.transform.position + (shipAI.transform.up * 10), false);

			//bring ship up towards zero from the lower hangar starting position
			float newZ = Mathf.Lerp(startingZ, 0, (Mathf.Pow((timer - startTime), 2)
				/ Mathf.Pow(2f, 2)));
			Vector3 pos = shipAI.transform.position;
			pos.z = newZ;
			shipAI.transform.position = pos;

			yield return new WaitForEndOfFrame();
		}
		Vector3 position = shipAI.transform.position;
		position.z = 0;
		shipAI.transform.position = position;

		shipAI.healthScript.enabled = true;
		shipAI.enabled = true;
	}

	IEnumerator PlayerShipLaunchFromHangar()
	{
		player.GetComponent<PlayerAILogic>().TogglePlayerControl(false, false, false, false);
		float startingZ = player.transform.position.z;

		yield return new WaitForSeconds(2);

		float startTime = timer;

		while(timer < (startTime + 2))
		{
			player.GetComponent<EnginesFighter>().MoveToTarget(player.transform.position + (player.transform.up * 10), false);

			//bring ship up towards zero from the lower hangar starting position
			float newZ = Mathf.Lerp(startingZ, 0, (Mathf.Pow((timer - startTime), 2)
				/ Mathf.Pow(2f, 2)));
			Vector3 pos = player.transform.position;
			pos.z = newZ;
			player.transform.position = pos;
			yield return new WaitForEndOfFrame();
		}
		Vector3 position = player.transform.position;
		position.z = 0;
		player.transform.position = position;

		player.GetComponent<PlayerAILogic>().TogglePlayerControl(true, true, true, true);

		for(int i = 0; i < PlayerAILogic.instance.squadLeaderScript.activeWingmen.Count; i++)
		{
			PlayerAILogic.instance.squadLeaderScript.CoverMe
			(PlayerAILogic.instance.squadLeaderScript.activeWingmen[i].GetComponent<AIFighter>());
		}
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

				gameTimeText.text = "Time: " + "<color>" + minsString + " : " + secsString + "</color>";
				playerKillsText.text = "Kills: " + "<color>" + playerKills + "</color>";
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

			if(Input.GetKeyDown(controlOfCameraKey))
				Camera.main.GetComponent<RTSCamera>().enabled = !Camera.main.GetComponent<RTSCamera>().enabled;

			if(hotkeysForTestActions.Length > 0)
			{
				for(int i = 0; i < hotkeysForTestActions.Length; i++)
				{
					if(Input.GetKeyDown(hotkeysForTestActions[i]))
					{
						testActions[i].Invoke();
					}
				}
			}
		}
	}

	public void ToggleWorldspaceGUIs()
	{
		if(canvasesOnOrOff == "off")
			canvasesOnOrOff = "on";
		else
			canvasesOnOrOff = "off";
		
		Canvas[] allCanvases = FindObjectsOfType<Canvas>();
		{
			foreach(Canvas canvas in allCanvases)
			{
				if(canvas.renderMode == RenderMode.WorldSpace)
				{
					if(canvasesOnOrOff == "off")
						canvas.enabled = false;
					else if(canvasesOnOrOff == "on")
						canvas.enabled = true;
				}
			}
		}
	}
		

	public void SpawnPilotEVA(Vector3 pos, Quaternion rot, bool isPlayer)
	{
		GameObject pilot = Instantiate(pilotEVAPrefab, pos + pilotEVAPrefab.transform.position, rot) as GameObject;
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

	void AlliesSpawned()
	{		
		//REMOVE currently just a decoy function to prevent null ref when ships spawn. Might do something with this.
	}
	void EnemiesSpawned()
	{
		//REMOVE currently just a decoy function to prevent null ref when ships spawn. Might do something with this.
	}
}

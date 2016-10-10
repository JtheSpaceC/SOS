using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoSelfPlayingLevel : MonoBehaviour {

	public GameObject levelCamera;
	float cameraZPos;
	public GameObject target;
	public bool changeTargetsAtIntervals = true;
	public Text followCamText;
	public Text pressStartText;
	public Text scoresText;
	public bool keepScores = false;
	public AICommander pmcCommander;
	public AICommander enemyCommander;

	public float sceneResetTime = 90;
	float originalSceneResetTime;
	public float maxPMCFighters = 3;
	public float maxStormwallFighters = 6;

	public GameObject PMCFighterTrioPrefab;
	public GameObject EnemyFighterTrioPrefab;
	public Transform theFleet;

	private Vector3 velocity = Vector3.zero;

	public bool spawnerOn = true;
	bool gotNewTarget = false;
	float blackoutTime = 1.5f;
	public string SceneToGoToFromHere = "";

	List<GameObject> PMCcraft = new List<GameObject>();

	public GameObject[] itemsToTurnOff;

	public GameObject radarCamera;

	bool haveLoadedMenu = false;
	GameObject mainMenu;

	public bool cameraAutoChanges = true;
	public bool showEnemyUI = true;

	[Header("Difficulty Stuff")]
	[Tooltip("Do we replace prefab health with ones we specifically want for demo?")] 
	public bool overridePrefabHealths = true;
	public int enemyStartingAwareness = 0;
	public int enemyMaxAwareness = 2;
	public int enemySnapFocus = 0;
	public int enemyAwarenessRecharge = 3;
	public int PMCStartingAwareness = 4;
	public int PMCMaxAwareness = 4;
	public int PMCSnapFocus = 1;
	public int PMCAwarenessRecharge = 2;



	void Start () 
	{
		target = GameObject.FindGameObjectWithTag ("Fighter");

		if(cameraAutoChanges)
			Invoke("FindATargetFalse", 1);

		followCamText.text = "";
		if (spawnerOn) 
		{
			InvokeRepeating ("SpawnPMC", 2, 5);
			InvokeRepeating ("SpawnEnemy", 3, 5);
			InvokeRepeating ("ChangeFlashyText", 2, 2);
		}

		//this is for bringing music back after the logo plays after the demo is over
		try{ AudioMasterScript.instance.ClearAll();}catch{}

		Tools.instance.playerUI.SetActive(true);
		foreach(GameObject obj in itemsToTurnOff)
			obj.SetActive(false);

		radarCamera.transform.SetParent(Camera.main.transform);

		cameraZPos = levelCamera.transform.position.z;
		levelCamera.GetComponent<ClickToPlay>().escCanGiveQuitMenu = false;
		levelCamera.GetComponent<CameraControllerFighter>().cameraBehaviour = CameraControllerFighter.CameraBehaviour.SelfPlayScene;

		originalSceneResetTime = sceneResetTime;

		if(!keepScores)
		{
			scoresText.text = "";
		}
	}

	void OnEnable()
	{
		if(overridePrefabHealths)
		{
			_battleEventManager.pmcFightersSpawned += FixDemoSpecificConcerns;
			_battleEventManager.enemyFightersSpawned += FixDemoSpecificConcerns;
		}
	}

	void OnDisable()
	{
		if(overridePrefabHealths)
		{
			_battleEventManager.pmcFightersSpawned -= FixDemoSpecificConcerns;
			_battleEventManager.enemyFightersSpawned -= FixDemoSpecificConcerns;
		}
	}


	void Update()
	{
		if(target != null)
		{
			followCamText.text = "Following: " + target.name + "\n" +
				"Orders: " + StaticTools.SplitCamelCase(target.GetComponent<AIFighter> ().currentState.ToString());

			if(target.GetComponent<AIFighter>().target != null)
			{
				followCamText.text += "\n" +
					"Target: " + target.GetComponent<AIFighter>().target.name;
			}
		}

		if (Input.GetKeyDown (KeyCode.C))
			FindATarget (true);

		if (Input.GetKeyDown (KeyCode.R))
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		/*
		if (Input.GetButtonDown ("Start"))
		{
			Tools.instance.CommenceFadeout(blackoutTime);
			Invoke("LoadMainLevel", 1.6f);
		}*/

		if(Input.GetButtonDown ("Start"))
		{
			if(FindObjectOfType<MainMenu>())
			{
				mainMenu = FindObjectOfType<MainMenu>().gameObject;	
			}

			if(!haveLoadedMenu && FindObjectOfType<MainMenu>() == null)
			{
				SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
				haveLoadedMenu = true;

				sceneResetTime = Mathf.Infinity;
				AudioMasterScript.instance.FadeChannel("Master vol", -10, 0, 0.3f);
			}
			else if(haveLoadedMenu && mainMenu.activeSelf && 
				!CharacterPool.instance.characterPoolPanel.activeInHierarchy &&
				!CharacterPool.instance.characterCreationPanel.activeInHierarchy && 
				!CharacterPool.instance.poolImportExportEditPanel.activeInHierarchy)
			{
				mainMenu.SetActive(false);
				sceneResetTime = Director.instance.timer + originalSceneResetTime;
				AudioMasterScript.instance.FadeChannel("Master vol", 0, 0, 0.3f);
			}
			else if(haveLoadedMenu && !mainMenu.activeSelf)
			{
				mainMenu.SetActive(true);
				sceneResetTime = Mathf.Infinity;
				AudioMasterScript.instance.FadeChannel("Master vol", -10, 0, 0.3f);
			}
		}

		if(Director.instance.timer > sceneResetTime && sceneResetTime > 0)
		{
			Director.instance.timer = -999;
			Tools.instance.CommenceFade(0, 2.5f, Color.clear, Color.black);
			Invoke("RestartScene", 2.5f);
		}

		if(keepScores)
		{
			scoresText.text = "Craft Lost:\n-------\n" + 
				"PMC: " + pmcCommander.losses + "\n" +
				"Enemy:" + enemyCommander.losses 
				+ "\n\n" +
				"Craft Retreated:\n-------\n" + 
				"PMC: " + pmcCommander.retreated + "\n" +
				"Enemy:" + enemyCommander.retreated;
		}
	}

	void RestartScene()
	{
		ClickToPlay.instance.LoadScene("_logo");
	}

	void ChangeFlashyText()
	{
		if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
			pressStartText.text = pressStartText.text == "PRESS START" ? "DEMO MODE" : "PRESS START";
		else if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
			pressStartText.text = pressStartText.text == "PRESS 'ESC'" ? "DEMO MODE" : "PRESS 'ESC'";		
	}
	void LoadMainLevel()
	{
		SceneManager.LoadScene (SceneToGoToFromHere);
	}

	void FindATargetFalse()
	{
		FindATarget (false);
	}

	void FindATarget(bool nextTarget)
	{
		if(!changeTargetsAtIntervals)
			return;
		
		CancelInvoke("FindATargetFalse");
		gotNewTarget = false;
		PMCcraft.Clear ();

		//check there are PMC fighters in the scene
		if(CountFighters(LayerMask.NameToLayer("PMCFighters")) == 0)
		{
			SpawnPMC ();
			Invoke ("FindATargetFalse", 1);
			return;
		}

		//make a list of them
		GameObject[] targets = GameObject.FindGameObjectsWithTag ("Fighter");

		foreach(GameObject exampleTarget in targets)
		{
			if(exampleTarget.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				PMCcraft.Add(exampleTarget);
			}
		}

		if(!nextTarget)
		{
			GameObject proposedTarget = PMCcraft [Random.Range (0, PMCcraft.Count - 1)];

			if(proposedTarget == target)
			{
				if(target == PMCcraft[PMCcraft.Count-1])
					target = PMCcraft[0];
				else 
					target = PMCcraft[PMCcraft.Count-1];
			}
			else target = proposedTarget;
		}
		else if(nextTarget)
		{
			for(int i = 0; i < PMCcraft.Count; i++)
			{
				if(PMCcraft[i] == target)
				{
					if(i == PMCcraft.Count -1)
					{
						target = PMCcraft[0];
						gotNewTarget = true;
						break;
					}
					else 
					{
						target = PMCcraft[i+1];
						gotNewTarget = true;
						break;
					}
				}
			}
			if(!gotNewTarget)
				target = PMCcraft[0];
		}

		if (target == null || !target.activeSelf) 
		{
			Debug.LogError("This should never happen");
			SpawnPMC ();
			Invoke ("FindATargetFalse", 1);
		}

		if(cameraAutoChanges)
			Invoke ("FindATargetFalse", Random.Range (5, 10));
	}

	void SpawnPMC()
	{
		if (CountFighters(LayerMask.NameToLayer("PMCFighters")) >= maxPMCFighters)
			return;

		Instantiate (PMCFighterTrioPrefab, ((Vector2)levelCamera.transform.position + Random.insideUnitCircle.normalized * 55), Quaternion.identity);
	}
	void SpawnEnemy()
	{
		if (CountFighters(LayerMask.NameToLayer("EnemyFighters")) >= maxStormwallFighters)
		{
			return;
		}
		
		Instantiate (EnemyFighterTrioPrefab, ((Vector2)levelCamera.transform.position + Random.insideUnitCircle.normalized * 100), Quaternion.identity);
	}
	void FixDemoSpecificConcerns()
	{
		AIFighter[] fighterscripts = FindObjectsOfType<AIFighter>();
		foreach(AIFighter fighter in fighterscripts)
		{
			if (!fighter.statsAlreadyAdjusted && fighter.whichSide == TargetableObject.WhichSide.Ally)
			{
				if(overridePrefabHealths)
				{
					fighter.healthScript.awareness = PMCStartingAwareness;
					fighter.healthScript.maxAwareness = PMCMaxAwareness;
					fighter.healthScript.snapFocusAmount = PMCSnapFocus;
					fighter.healthScript.awarenessRechargeTime = PMCAwarenessRecharge;
				}

				if(fighter.GetComponentInChildren<SquadronLeader>())
				{
					fighter.GetComponent<AIFighter>().escortShip = theFleet;
				}
				fighter.statsAlreadyAdjusted = true;
			}
			else if (!fighter.statsAlreadyAdjusted && fighter.whichSide == TargetableObject.WhichSide.Enemy)				
			{
				if(!showEnemyUI)
				{
					fighter.healthScript.healthSlider.gameObject.SetActive(false);
					fighter.healthScript.awarenessSlider.gameObject.SetActive(false);
				}
				if(overridePrefabHealths)
				{
					fighter.healthScript.awareness = enemyStartingAwareness;
					fighter.healthScript.maxAwareness = enemyMaxAwareness;
					fighter.healthScript.snapFocusAmount = enemySnapFocus;
					fighter.healthScript.awarenessRechargeTime = enemyAwarenessRecharge;
				}
				fighter.statsAlreadyAdjusted = true;
			}
		}
	}


	void FixedUpdate()
	{
		if(target == null)
			return;
		Vector3 pos = target.transform.position;
		pos.z = cameraZPos;
		levelCamera.transform.position = Vector3.SmoothDamp (levelCamera.transform.position, pos, ref velocity, 0.3f);
	}

	int CountFighters(LayerMask whichLayer)
	{
		int numFighters = 0;
		GameObject [] allFighters = GameObject.FindGameObjectsWithTag ("Fighter");
		if (allFighters.Length == 0)
			return numFighters;

		foreach(GameObject fighter in allFighters)
		{
			if(fighter.layer == whichLayer)
				numFighters ++;
		}

		return numFighters;
	}


	public void LoadDemo()
	{
		Tools.instance.CommenceFade(0, blackoutTime, Color.clear, Color.black);
		Invoke("LoadMainLevel", 1.6f);
	}
		
}//Mono

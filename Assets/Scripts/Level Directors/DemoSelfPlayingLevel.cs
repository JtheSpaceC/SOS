using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoSelfPlayingLevel : MonoBehaviour {

	public GameObject levelCamera;
	public GameObject target;
	public bool changeTargetsAtIntervals = true;
	public Text followCamText;
	public Text pressStartText;

	public float sceneResetTime = 90;
	public float maxPMCFighters = 3;
	public float maxStormwallFighters = 6;

	public GameObject PMCFighterTrioPrefab;
	public GameObject EnemyFighterTrioPrefab;

	private Vector3 velocity = Vector3.zero;

	public bool spawnerOn = true;
	bool gotNewTarget = false;
	float blackoutTime = 1.5f;
	public string SceneToGoToFromHere = "";

	List<GameObject> PMCcraft = new List<GameObject>();

	public GameObject radarLayout;
	public GameObject[] itemsToTurnOff;

	public GameObject radarCamera;


	void Start () 
	{
		target = GameObject.FindGameObjectWithTag ("Fighter");
		Invoke("FindATargetFalse", 1);

		Invoke("FixMaxAwareness", 0.05f);

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
		radarLayout.SetActive(true);
		foreach(GameObject obj in itemsToTurnOff)
			obj.SetActive(false);

		radarCamera.transform.SetParent(Camera.main.transform);
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

		if (Input.GetButtonDown ("Start"))
		{
			Tools.instance.CommenceFadeout(blackoutTime);
			Invoke("LoadMainLevel", 1.6f);
		}

		if(Director.instance.timer > sceneResetTime)
		{
			Director.instance.timer = -999;
			Tools.instance.CommenceFadeout(2.5f);
			Invoke("RestartScene", 2.5f);
		}
	}

	void RestartScene()
	{
		ClickToPlay.instance.LoadScene("_logo");
	}

	void ChangeFlashyText()
	{
		pressStartText.text = pressStartText.text == "PRESS START" ? "DEMO MODE" : "PRESS START";
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

		Invoke ("FindATargetFalse", Random.Range (5, 10));
	}

	void SpawnPMC()
	{
		if (CountFighters(LayerMask.NameToLayer("PMCFighters")) >= maxPMCFighters)
			return;

		Instantiate (PMCFighterTrioPrefab, ((Vector2)levelCamera.transform.position + Random.insideUnitCircle.normalized * 55), Quaternion.identity);

		Invoke("FixMaxAwareness", 0.05f);
	}
	void SpawnEnemy()
	{
		if (CountFighters(LayerMask.NameToLayer("EnemyFighters")) >= maxStormwallFighters)
		{
			return;
		}
		
		Instantiate (EnemyFighterTrioPrefab, ((Vector2)levelCamera.transform.position + Random.insideUnitCircle.normalized * 100), Quaternion.identity);
		Invoke("FixMaxAwareness", 0.05f);
	}
	void FixMaxAwareness()
	{
		AIFighter[] fighterscripts = FindObjectsOfType<AIFighter>();
		foreach(AIFighter fighter in fighterscripts)
		{
			if (fighter.whichSide == TargetableObject.WhichSide.Ally)
			{
				fighter.healthScript.maxAwareness = 3;
			}
			else
			{
				fighter.healthScript.healthSlider.gameObject.SetActive(false);
				fighter.healthScript.awarenessSlider.gameObject.SetActive(false);
			}			
		}
	}


	void FixedUpdate()
	{
		if(target == null)
			return;

		Vector3 pos = target.transform.position;
		pos.z = -50;
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

}//Mono

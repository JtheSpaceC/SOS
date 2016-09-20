using UnityEngine;

public class PlayerAILogic : FighterFunctions {

	public PlayerAILogic instance;

	[HideInInspector]public HealthFighter healthScript;
	[HideInInspector]public PlayerFighterMovement engineScript;
	[HideInInspector]public WeaponsPrimaryFighter shootScript;
	[HideInInspector]public WeaponsSecondaryFighter missilesScript;
	[HideInInspector]public Dodge dodgeScript;
	SquadronLeader squadLeaderScript;

	
	public enum Orders {FighterSuperiority, Patrol, Escort, RTB, NA}; //set by commander
	public Orders orders;
	
	public GameObject target;

	bool radialMenuShown = false;

	
	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogError("There were 2 PlayerAILogic scripts");
			Destroy(gameObject);
			return;
		}
		healthScript = GetComponent<HealthFighter> ();
		engineScript = GetComponent<PlayerFighterMovement> ();
		shootScript = GetComponentInChildren<WeaponsPrimaryFighter> ();
		missilesScript = GetComponentInChildren<WeaponsSecondaryFighter> ();
		dodgeScript = GetComponentInChildren<Dodge>();
		myRigidbody = GetComponent<Rigidbody2D>();
		squadLeaderScript = GetComponentInChildren<SquadronLeader>();

		if(transform.FindChild("Effects/GUI"))
		{
			myGui = transform.FindChild("Effects/GUI").gameObject;
		}

		SetUpSideInfo();

		//Friendly Commander script automatically adds player to known craft
		enemyCommander.knownEnemyFighters.Add (this.gameObject); //TODO; AI Commander instantly knows all enemies. Make more complex
	}


	void Update()
	{
		if(!radialMenuShown && (Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Orders Vertical")) > 0.5f))
		{
			Tools.instance.StopCoroutine("FadeScreen");
			Tools.instance.MoveCanvasToFront(Tools.instance.blackoutPanel.GetComponentInParent<Canvas>());
			Tools.instance.blackoutPanel.color = Color.Lerp (Color.black, Color.clear, 0.1f);
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", -15f);
			print(Time.fixedDeltaTime);
			Time.timeScale = 0.02f;
			Time.fixedDeltaTime /= 50;
			radialMenuShown = true;
		}
		else if(radialMenuShown && (Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Orders Vertical")) > 0.5f))
		{
			Tools.instance.MoveCanvasToRear(Tools.instance.blackoutPanel.GetComponentInParent<Canvas>());
			Tools.instance.blackoutPanel.color = Color.clear;
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", 0f);
			Time.timeScale = 1f;
			Time.fixedDeltaTime *= 50;

			radialMenuShown =false;
		}
	}


	public void TogglePlayerControl(bool healthScriptenabled, bool engineScriptEnabled, bool dodgeScriptEnabled, bool shootScriptEnabled)
	{
		healthScript.enabled = healthScriptenabled;
		engineScript.enabled = engineScriptEnabled;
		dodgeScript.enabled = dodgeScriptEnabled;
		shootScript.enabled = shootScriptEnabled;
	}


	void TargetDestroyed()
	{
		if(myAttackers.Contains(target))
		{
			myAttackers.Remove(target);
		}
		if(myCommander.knownEnemyFighters.Contains(target))
		{
			myCommander.knownEnemyFighters.Remove(target);
		}
		target = null;
	}

	void ReportActivity()
	{
		CameraTactical.reportedInfo = this.name + "\n" + "(You)\n";

		if ((float)healthScript.health / healthScript.maxHealth < (0.33f)) 
			CameraTactical.reportedInfo += "Heavily Damaged";		
		else if (healthScript.health == healthScript.maxHealth) 
			CameraTactical.reportedInfo += "Fully Functional";		
		else
			CameraTactical.reportedInfo += "Damaged";
	}

	public void CallDumpAwarenessMana(int howMany)
	{
		StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(howMany));
	}
}//Mono

using UnityEngine;
using System.Collections;

public class PlayerAILogic : FighterFunctions {

	public static PlayerAILogic instance;

	[HideInInspector]public PlayerFighterMovement engineScript;
	[HideInInspector]public WeaponsPrimaryFighter shootScript;
	[HideInInspector]public WeaponsSecondaryFighter missilesScript;
	[HideInInspector]public Dodge dodgeScript;
	[HideInInspector]public SquadronLeader squadLeaderScript;
	
	public enum Orders {FighterSuperiority, Patrol, Escort, RTB, NA}; //set by commander
	public Orders orders;
	
	public GameObject target;

	[HideInInspector] public bool[] previousPlayerControlBools = new bool[4];


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

		if(useSolEdStatsToOverride)
			PullStatsFromSolEd();

		if(transform.FindChild("Effects/GUI"))
		{
			myGui = transform.FindChild("Effects/GUI").gameObject;
		}

		SetUpSideInfo();

		//Friendly Commander script automatically adds player to known craft
		enemyCommander.knownEnemyFighters.Add (this.gameObject); //TODO; AI Commander instantly knows all enemies. Make more complex

	}

	public void TogglePlayerControl(bool healthScriptenabled, bool engineScriptEnabled,
		bool dodgeScriptEnabled, bool shootScriptEnabled, bool radioEnabled, bool tacMapEnabled, bool escMenuEnabled)
	{
		previousPlayerControlBools = new bool[]
		{healthScript.enabled, engineScript.enabled, dodgeScript.enabled, shootScript.enabled, 
			RadialRadioMenu.instance.canAccessRadialRadio, CameraTactical.instance.canAccessTacticalMap, ClickToPlay.instance.escCanGiveQuitMenu};

		healthScript.enabled = healthScriptenabled;
		engineScript.enabled = engineScriptEnabled;
		dodgeScript.enabled = dodgeScriptEnabled;
		shootScript.enabled = shootScriptEnabled;
		RadialRadioMenu.instance.canAccessRadialRadio = radioEnabled;
		CameraTactical.instance.canAccessTacticalMap = tacMapEnabled;
		ClickToPlay.instance.escCanGiveQuitMenu = escMenuEnabled;
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

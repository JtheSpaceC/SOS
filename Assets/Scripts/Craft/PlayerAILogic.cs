using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAILogic : FighterFunctions {

	public PlayerAILogic instance;

	[HideInInspector]public HealthFighter healthScript;
	[HideInInspector]public PlayerFighterMovement engineScript;
	[HideInInspector]public WeaponsPrimaryFighter shootScript;
	[HideInInspector]public WeaponsSecondaryFighter missilesScript;
	[HideInInspector]public Dodge dodgeScript;

	
	public enum Orders {FighterSuperiority, Patrol, Escort, RTB, NA}; //set by commander
	public Orders orders;
	
	public GameObject target;

	
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

		SetUpAICommander();

		//Friendly Commander script automatically adds player to known craft
		enemyCommander.knownEnemyFighters.Add (this.gameObject); //TODO; AI Commander instantly knows all enemies. Make more complex
	}


	public void RemovePlayerControl(bool disableHealthScript, bool disableEngineScript, bool disableShootScript)
	{
		healthScript.enabled = !disableHealthScript;
		engineScript.enabled = !disableEngineScript;
		shootScript.enabled = !disableShootScript;
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


		if (healthScript.health / healthScript.maxHealth < (0.33f)) {
			CameraTactical.reportedInfo += "Heavily Damaged";
		}
		else if (healthScript.health / healthScript.maxHealth < (0.66f)) {
			CameraTactical.reportedInfo += "Damaged";
		}
		else
			CameraTactical.reportedInfo += "Fully Functional";
	}

	public void CallDumpAwarenessMana(int howMany)
	{
		StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(howMany));
	}
}//Mono

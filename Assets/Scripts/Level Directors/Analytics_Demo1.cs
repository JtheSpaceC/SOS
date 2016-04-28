using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class Analytics_Demo1 : MonoBehaviour {

	public static Analytics_Demo1 instance;
	public bool doAnalytics = true;

	float timeTakenToCallTransport;

	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were 2 Analytics_Demo1 scripts. Destroying 1.");
			Destroy(gameObject);
		}

		if(!doAnalytics)
			this.enabled = false;
	}

	void OnEnable()
	{
		_battleEventManager.pickupOnTheWay += PlayerCalledTransport;
		_battleEventManager.playerBeganDocking += PlayerCommencedDocking;
		_battleEventManager.playerLeaving += PlayerWarpedOut;
		_battleEventManager.playerShotDown += PlayerDied;
	}
	void OnDisable()
	{
		_battleEventManager.pickupOnTheWay -= PlayerCalledTransport;
		_battleEventManager.playerBeganDocking += PlayerCommencedDocking;
		_battleEventManager.playerLeaving -= PlayerWarpedOut;
		_battleEventManager.playerShotDown -= PlayerDied;
	}

	void PlayerCalledTransport()
	{
		Debug.Log("Player Called Transport Analytic");

		if(DemoLevel.instance.clearedKillLimitAtThisTime == Mathf.Infinity)
			return;
		
		timeTakenToCallTransport = Time.time - DemoLevel.instance.clearedKillLimitAtThisTime;

		Analytics.CustomEvent("Called Transport AFter Complete", new Dictionary<string, object>
			{
				{"Time since mission complete", timeTakenToCallTransport}
			});
	}

	void PlayerCommencedDocking()
	{
		Debug.Log("Player Commenced Docking Analytic");

		if(DemoLevel.instance.clearedKillLimitAtThisTime == Mathf.Infinity)
			return;

		float timeToDock = Time.time - (DemoLevel.instance.clearedKillLimitAtThisTime + timeTakenToCallTransport);

		Analytics.CustomEvent("Docked With Transport AFter Complete", new Dictionary<string, object>
			{
				{"Time since calling Transport", timeToDock}
			});
	}

	void PlayerWarpedOut()
	{
		Debug.Log("Player Warped Out Analytic");

		Analytics.CustomEvent("Player Warped Out", new Dictionary<string, object>
			{
				{"Time in session", Director.instance.timer},
				{"Kills: ", Director.instance.playerKills},
				{"Time until first kill: ", Director.instance.timeUntilFirstKill},
				{"Manual Dodges made", Director.instance.numberOfManualDodges},
				{"Successful Dodges made", Director.instance.numberOfSuccessfulDodges},
				{"Automated Dodges made", Director.instance.numberOfAutomatedDodges},
				{"One Hit Kills", Director.instance.numberOfSpecialsUsed},
				{"Radio Button Presses", Director.instance.radioButtonPresses},
				{"Tac Map Uses", Director.instance.tacMapUses}
			});
	}

	void PlayerDied()
	{
		Debug.Log("Player Died Analytic");

		Analytics.CustomEvent("Player Died", new Dictionary<string, object>
			{
				{"Time in session", Director.instance.timer},
				{"Kills: ", Director.instance.playerKills},
				{"Time until first kill: ", Director.instance.timeUntilFirstKill},
				{"Manual Dodges made", Director.instance.numberOfManualDodges},
				{"Successful Dodges made", Director.instance.numberOfSuccessfulDodges},
				{"Automated Dodges made", Director.instance.numberOfAutomatedDodges},
				{"One Hit Kills", Director.instance.numberOfSpecialsUsed},
				{"Radio Button Presses", Director.instance.radioButtonPresses},
				{"Tac Map Uses", Director.instance.tacMapUses}
			});
	}

	public void PlayerWalkedAwayFromConsole()
	{
		Debug.Log("Player Walked Away Analytic");

		Analytics.CustomEvent("Game Reset (Idle)");
	}

}

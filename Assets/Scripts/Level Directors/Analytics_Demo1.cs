using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class Analytics_Demo1 : MonoBehaviour {

	public static Analytics_Demo1 instance;

	float levelStartTime;


	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were 2 Analytics_Demo1 scripts. Destroying 1.");
			Destroy(gameObject);
		}
	}

	void OnEnable()
	{
		_battleEventManager.pickupOnTheWay += PlayerLeft;
		_battleEventManager.playerLeaving += PlayerLeft;
		_battleEventManager.playerShotDown += PlayerDied;
	}
	void OnDisable()
	{
		_battleEventManager.pickupOnTheWay -= PlayerLeft;
		_battleEventManager.playerLeaving -= PlayerLeft;
		_battleEventManager.playerShotDown -= PlayerDied;
	}

	void PlayerLeft()
	{
		
	}

	void PlayerDied()
	{
		
	}

	void Other()
	{
		Analytics.CustomEvent("Level Complete", new Dictionary<string, object>
			{
				{"Time in session", Director.instance.timer},
				{"Kills: ", Director.instance.playerKills}
			});
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FleetActionScene : MonoBehaviour {

	List<AIFighter> leaderAIScripts = new List<AIFighter>();


	void Start ()
	{
		Invoke("SetDestinations", 0.1f); 
	}

	void SetDestinations ()
	{
		SquadronLeader[] leaderArrowScripts = FindObjectsOfType<SquadronLeader> ();

		foreach (SquadronLeader sl in leaderArrowScripts) {
			if (sl.transform.root.tag != "PlayerFighter")
			{
				leaderAIScripts.Add (sl.transform.root.GetComponent<AIFighter> ());
			}
		}
		foreach (AIFighter aiScript in leaderAIScripts) 
		{
			aiScript.patrolPoint = (Vector2)transform.position + Vector2.up * 1000;
		}
	}
}

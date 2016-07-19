using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnerGroup : MonoBehaviour {

	AICommander myCommander;

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;

	public GameObject objectPrefab;
	public GameObject squadLeaderPrefab;

	[Range(1, 12)] public int numberToSpawn = 3;

	[HideInInspector] public List<GameObject> craft;

	[HideInInspector] public string squadName;


	void Start()
	{
		//check which side we're on and get a squad name from it
		if (whichSide == WhichSide.Ally)
		{
			myCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("PMC Commander").GetComponent<AICommander> ();
		}
		else if (whichSide == WhichSide.Enemy)
		{
			myCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("Enemy Commander").GetComponent<AICommander> ();
		}
		squadName = myCommander.RequestSquadronName ();

		SpawnTrio ();


		Destroy (gameObject);
	}

	void SpawnTrio ()
	{
		//spawn the required number of craft
		for (int i = 0; i < numberToSpawn; i++) {
			GameObject obj = Instantiate (objectPrefab, (Vector2)transform.position + Random.insideUnitCircle, Quaternion.identity) as GameObject;
			obj.name = squadName + " " + (i + 1);
			craft.Add (obj);
		}
		//create SquadLeader object and put it on the leader
		squadLeaderPrefab = Instantiate (squadLeaderPrefab, transform.position, Quaternion.identity) as GameObject;
		squadLeaderPrefab.transform.SetParent (craft [0].transform.FindChild ("Abilities"));
		squadLeaderPrefab.transform.localPosition = Vector3.zero;
		squadLeaderPrefab.GetComponent<SquadronLeader> ().whichSide = (whichSide == WhichSide.Ally) ? SquadronLeader.WhichSide.Ally : SquadronLeader.WhichSide.Enemy;
		//set up the wingmen to know who their leader is and cover him
		for (int i = 1; i < craft.Count; i++) {
			squadLeaderPrefab.GetComponent<SquadronLeader> ().activeWingmen.Add (craft [i]);
			craft [i].GetComponent<AIFighter>().flightLeader = craft [0];
			craft [i].GetComponent<AIFighter>().flightLeadSquadronScript = craft [0].GetComponentInChildren<SquadronLeader> ();
			craft [i].GetComponent<AIFighter>().currentState = AIFighter.StateMachine.Covering;
		}
	}

}

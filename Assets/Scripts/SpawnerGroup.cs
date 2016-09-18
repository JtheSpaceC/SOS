using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnerGroup : MonoBehaviour {

	AICommander myCommander;

	public enum SpawnMode {Normal, ApproachFromDepth};
	public SpawnMode spawnMode;

	[Tooltip("If ApproachFromDepth is selected, what is the positive value Z depth to be used?")]
	public float depth;
	[Tooltip("If ApproachFromDepth is selected, what is the negative value speed of approach?")]
	public float speed = -20f;

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;

	public GameObject objectPrefab;
	public GameObject squadLeaderPrefab;

	[Range(1, 12)] public int numberToSpawn = 3;

	[HideInInspector] public List<GameObject> craft;



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

		if(spawnMode == SpawnMode.Normal)
		{
			SpawnGroup ();
		}
		else if (spawnMode == SpawnMode.ApproachFromDepth)
		{
			GetComponent<TrailRenderer>().enabled = true;
			GetComponent<moverBasic>().enabled = true;
			GetComponent<moverBasic>().speed.z = speed;

			Vector3 pos = transform.position;
			pos.z = depth;
			transform.position = pos;
		}


	}

	void Update()
	{
		if(spawnMode == SpawnMode.ApproachFromDepth)
		{
			if(transform.position.z < 0)
			{
				Vector3 pos = transform.position;
				pos.z = 0;
				transform.position = pos;

				SpawnGroup();
			}
		}
	}

	void SpawnGroup ()
	{
		string squadronName = myCommander.RequestSquadronName ();

		//spawn the required number of craft
		for (int i = 0; i < numberToSpawn; i++) {
			GameObject obj = Instantiate (objectPrefab, (Vector2)transform.position + Random.insideUnitCircle.normalized *2f, 
				Quaternion.identity) as GameObject;
			obj.name = squadronName + " " + (i + 1);
			craft.Add (obj);
		}
		//create SquadLeader object and put it on the leader
		squadLeaderPrefab = Instantiate (squadLeaderPrefab, transform.position, Quaternion.identity) as GameObject;
		squadLeaderPrefab.transform.SetParent (craft [0].transform.FindChild ("Abilities"));
		squadLeaderPrefab.transform.localPosition = Vector3.zero;
		squadLeaderPrefab.GetComponent<SquadronLeader>().whichSide = (whichSide == WhichSide.Ally) ? SquadronLeader.WhichSide.Ally : SquadronLeader.WhichSide.Enemy;
		squadLeaderPrefab.GetComponent<SquadronLeader>().squadName = squadronName;
		myCommander.mySquadrons.Add(squadLeaderPrefab.GetComponent<SquadronLeader>());

		//set up the wingmen to know who their leader is and cover him
		for (int i = 1; i < craft.Count; i++) {
			squadLeaderPrefab.GetComponent<SquadronLeader> ().activeWingmen.Add (craft [i]);
			craft [i].GetComponent<AIFighter>().flightLeader = craft [0];
			craft [i].GetComponent<AIFighter>().flightLeadSquadronScript = craft [0].GetComponentInChildren<SquadronLeader> ();
			craft [i].GetComponent<AIFighter>().currentState = AIFighter.StateMachine.Covering;
		}

		if(whichSide == WhichSide.Ally)
			_battleEventManager.instance.CallPMCFightersSpawned();
		else if(whichSide == WhichSide.Enemy)
			_battleEventManager.instance.CallEnemyFightersSpawned();

			Destroy (gameObject);
	}

}

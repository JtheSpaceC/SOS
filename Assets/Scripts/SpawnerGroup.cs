using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnerGroup : MonoBehaviour {

	AICommander myCommander;

	public enum SpawnMode {Normal, ApproachFromDepth};
	public SpawnMode spawnMode;

	public GameObject trailRendererPrefab;
	List<TrailRenderer> trailRenderersList = new List<TrailRenderer>();

	[Tooltip("If ApproachFromDepth is selected, what is the positive value Z depth to be used?")]
	public float depth = 250f;
	[Tooltip("If ApproachFromDepth is selected, what is the negative value speed of approach?")]
	public float approachSpeed = 20f;
	public float approachFromDepthSpawnRadius = 25f;
	float approachTime;
	float startTime;

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;

	public GameObject objectPrefab;
	public GameObject squadLeaderPrefab;

	[Tooltip("If in Normal spawn mode, spawn this many ships. Otherwise spawn 1.")]
	[Range(1, 12)] public int numberToSpawn = 3;

	[HideInInspector] public List<GameObject> craft;

	[Header("SolEd")]
	public int solEdLevel = 1;
	public string solEdSpecialTag;

	Vector3 pos;
	Vector3 destination;
	Vector3 startPos;
	Vector2 dir;

	bool alreadySpawned = false;

	float coroutineStartTime;
	float coroutineDuration;


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
			pos = transform.position;
			pos.z = depth;
			transform.position = pos;

			startPos = pos;
			dir = Random.insideUnitCircle.normalized;

			approachTime = depth/approachSpeed;
			startTime = Time.time;

			if(numberToSpawn != 0)
			{
				for(int i = 0; i < numberToSpawn; i++)
				{
					GameObject newChild = Instantiate(trailRendererPrefab);
					newChild.transform.SetParent(this.transform);
					newChild.transform.localPosition = Random.insideUnitCircle.normalized*2;
					trailRenderersList.Add(newChild.GetComponent<TrailRenderer>());
				}
			}
		}
	}

	void Update()
	{
		if(spawnMode == SpawnMode.ApproachFromDepth)
		{
			if(transform.position.z > 0) //move up
			{
				//recheck destination
				destination = (Vector2)Camera.main.transform.position + (dir * approachFromDepthSpawnRadius);

				//move towards it
				transform.position = Vector3.Lerp(startPos, destination, (Time.time - startTime)/approachTime);
			}
			else if(transform.position.z <= 0 && !alreadySpawned) //we've moved far enough. Spawn now.
			{
				pos = transform.position;
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
		for (int i = 0; i < numberToSpawn; i++) 
		{
			GameObject obj = Instantiate (objectPrefab, (Vector2)transform.position + Random.insideUnitCircle.normalized *2f, 
				Quaternion.identity) as GameObject;
			obj.name = squadronName + " " + (i + 1);
			obj.GetComponent<AIFighter>().myLevel = solEdLevel;
			obj.GetComponent<AIFighter>().specialShip = solEdSpecialTag;
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

		alreadySpawned = true;

		if(spawnMode == SpawnMode.Normal)
			Destroy(gameObject);
		else StartCoroutine("ShrinkTrail");
	}

	IEnumerator ShrinkTrail()
	{
		coroutineStartTime = Time.time;
		coroutineDuration = trailRenderersList[0].time - 0.1f;

		while((Time.time - coroutineStartTime) < coroutineDuration)
		{
			for(int i = 0; i < trailRenderersList.Count; i++)
			{
				trailRenderersList[i].startWidth -= (1/coroutineDuration * Time.deltaTime);
				//shrink it a little
				yield return new WaitForEndOfFrame();
			}
		}
		Destroy(gameObject);
	}
}//mono



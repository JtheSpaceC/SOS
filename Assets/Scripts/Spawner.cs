using UnityEngine;
using System.Collections;


public class Spawner : MonoBehaviour {

	GameObject player;
	public GameObject spawnObj;
	public int numberInGroup = 3;
	public SpawnerGroup.SpawnMode mySpawnMode;
	public float spawnTime = 4;
	public float spawnRadius = 25;
	public AICommander enemyCommanderScript;

	[Header ("For Difficulty")]
	public float firstDelay = 8;
	public float maxFightersAtOnce = 3;
	public bool decreaseSpawnGapOverTime;
	public float decreaseAmount = 0.5f;
	public float minSpawnTime = 3;
	public float maxSpawnTime = 20;

	public bool automaticSpawning = true;
	public KeyCode myKey;

	[Header("SolEd")]
	public int level = 1;
	public string specialTag;


	void Start()
	{
		if(automaticSpawning)
			Invoke ("Spawn", firstDelay);
	
		player = GameObject.FindGameObjectWithTag ("PlayerFighter");

		if (decreaseSpawnGapOverTime)
			spawnTime = maxSpawnTime;
	}

	void Update()
	{
		if(Input.GetKeyDown(myKey))
		{
			DoTheSpawn();
		}
	}

	void Spawn()
	{
		if (enemyCommanderScript.myFighters.Count >= maxFightersAtOnce)
		{
			Invoke ("Spawn", spawnTime);
			return;
		}

		DoTheSpawn();

		if(decreaseSpawnGapOverTime)
			spawnTime = Mathf.Clamp (spawnTime -= decreaseAmount, minSpawnTime, maxSpawnTime);

		Invoke ("Spawn", spawnTime);
	}

	void DoTheSpawn()
	{
		Vector3 spawnPoint = player == null ? Vector3.zero : player.transform.position;

		GameObject newFighter =
			Instantiate (spawnObj, spawnObj.transform.position + spawnPoint + (Vector3)Random.insideUnitCircle.normalized * spawnRadius, 
				Quaternion.identity) as GameObject;

		newFighter.GetComponent<SpawnerGroup>().spawnMode = mySpawnMode;
		newFighter.GetComponent<SpawnerGroup>().numberToSpawn = numberInGroup;
		newFighter.GetComponent<SpawnerGroup>().solEdSpecialTag = specialTag;
		newFighter.GetComponent<SpawnerGroup>().solEdLevel = level;
		//Instantiate (spawnObj, newFighter.transform.position + (Vector3)Random.insideUnitCircle, Quaternion.identity);
	}

	void OnDisable()
	{
		CancelInvoke("Spawn");
	}
}

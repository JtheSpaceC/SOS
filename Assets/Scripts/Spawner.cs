using UnityEngine;
using System.Collections;


public class Spawner : MonoBehaviour {

	GameObject player;
	public GameObject spawnObj;
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
	int i = 0;

	public bool automaticSpawning = true;
	public KeyCode myKey;


	void Start()
	{
		if(automaticSpawning)
			Invoke ("Spawn", firstDelay);
	
		player = GameObject.FindGameObjectWithTag ("PlayerFighter");

		if (decreaseSpawnGapOverTime)
			spawnTime = maxSpawnTime;
	}

	#if UNITY_EDITOR

	void Update()
	{
		if(Input.GetKeyDown(myKey))
		{
			DoTheSpawn();
		}
	}

	#endif

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
		i++;

		Vector3 spawnPoint = player == null ? Vector3.zero : player.transform.position;

		GameObject newFighter =
			Instantiate (spawnObj, spawnObj.transform.position + spawnPoint + (Vector3)Random.insideUnitCircle.normalized * spawnRadius, 
				Quaternion.identity) as GameObject;
		newFighter.name = "Stormwall Fighter " +i;
		//Instantiate (spawnObj, newFighter.transform.position + (Vector3)Random.insideUnitCircle, Quaternion.identity);
	}
}

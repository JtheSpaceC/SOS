using UnityEngine;

public class AsteroidSpawner : MonoBehaviour {

	ObjectPoolerScript asteroidPoolerScript;
	public int firstSpawnDelay = 1;
	public float spawnRate = 1;
	private GameObject player;
	public int asteroidCount;
	public int maxAsteroids = 100;
	public float maxSpawnRadius = 50.0f;
	public float noSpawnRadius = 25f;

	private CircleCollider2D asteroidField;
	
	
	void Awake()
	{
		asteroidPoolerScript = GameObject.Find ("asteroid Pooler").GetComponent<ObjectPoolerScript> ();
		asteroidField = GetComponent<CircleCollider2D>();
		asteroidCount = 0;
	}
	
	void OnEnable()
	{	
		InvokeRepeating ("SpawnAsteroid", firstSpawnDelay, spawnRate);
	}
	
	public void SpawnAsteroid()
	{	
		if(asteroidCount < maxAsteroids)
		{
			Vector3 spawnPosition = (Vector2)Camera.main.transform.position + 
				(Random.insideUnitCircle.normalized*Random.Range(noSpawnRadius, maxSpawnRadius));

			if(asteroidField.bounds.Contains(spawnPosition))
			{
				GameObject obj = asteroidPoolerScript.current.GetPooledObject ();
				obj.transform.position = spawnPosition + new Vector3 (0, 0, obj.transform.position.z);
				obj.GetComponent<Asteroid>().myAsteroidSpawner = this;
				obj.SetActive (true);
				
				asteroidCount ++;
			}
		}
	}
	
	void OnDisable()
	{
		CancelInvoke ();
	}
	
}
using UnityEngine;
using System.Collections;

public class AsteroidField : MonoBehaviour {
	
	AsteroidSpawner asteroidSpawner;
	public bool restrictPlanetApproach = false;
	
	void Awake () 
	{
		asteroidSpawner = GetComponent<AsteroidSpawner> ();
	}
	
	void OnTriggerEnter2D (Collider2D other) 
	{
		if(other.transform.parent.gameObject == Camera.main.gameObject)
		{
			asteroidSpawner.enabled = true;
		}
	}
	
	void OnTriggerExit2D (Collider2D other)
	{
		if(other.transform.parent.gameObject == Camera.main.gameObject)
		{
			asteroidSpawner.enabled = false;
		}
	}

	void OnDisable()
	{
	}
}

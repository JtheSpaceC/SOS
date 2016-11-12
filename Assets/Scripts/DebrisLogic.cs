using UnityEngine;
using System.Collections;

public class DebrisLogic : MonoBehaviour {

	public bool destroyAfterTime = true;
	public float destroyTime = 8;

	public float timeSinceInvisible;
	public bool isVisible;

	void OnBecameInvisible()
	{
		isVisible = false;
		timeSinceInvisible = 0;
	}

	void OnBecameVisible()
	{
		isVisible = true;
		timeSinceInvisible = 0;
	}

	void Update()
	{
		if(isVisible == false)
			timeSinceInvisible += Time.deltaTime;

		if(destroyAfterTime && timeSinceInvisible >= destroyTime)
			DestroyThis();
	}

	void DestroyThis()
	{
		gameObject.SetActive(false);
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.gameObject.tag == "Asteroid")
		{
			if(Random.Range(0, 2) == 0)
			{
				Tools.instance.SpawnAsteroidPoof(transform.position);
			}
			else
			{
				Tools.instance.SpawnExplosionMini(this.gameObject, .1f);
				Destroy(gameObject);
			}
		}
	}
}

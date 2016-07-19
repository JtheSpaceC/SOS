using UnityEngine;
using System.Collections;

public class DebrisLogic : MonoBehaviour {

	public bool destroyAfterTime = true;
	public float time = 12;

	void OnBecameInvisible()
	{
		if(destroyAfterTime)
			Invoke("DestroyThis", time);	
	}

	void OnBecameVisible()
	{
		if(destroyAfterTime)
			CancelInvoke("DestroyThis");
	}

	void DestroyThis()
	{
		Destroy(gameObject);
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

using UnityEngine;
using System.Collections;

public class DebrisLogic : MonoBehaviour {

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

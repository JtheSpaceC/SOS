using UnityEngine;

public class DeleteAfterInvisible : MonoBehaviour {

	float birthTime;
	public float minLifetime = 4f; //won't destory before this time even if off screen

	void Awake()
	{
		birthTime = Time.time;
	}

	void OnBecameInvisible()
	{
		if(Time.time > birthTime + minLifetime)
		{
			Destroy(gameObject);
		}
		else
			InvokeRepeating("CheckDestroy", 1, 1);
	}

	void CheckDestroy()
	{
		if(GetComponent<Renderer>().isVisible)
		{
			Destroy(gameObject);
		}
	}
}

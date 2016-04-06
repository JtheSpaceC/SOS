using UnityEngine;
using System.Collections;

public class Heartbeat : MonoBehaviour {

	TrailRenderer tr;

	public float bpm = 90f;
	public float scrollSpeed = 3;
	public float verticalSpeed = 30;
	float lastBeatTime;

	public float peakHeight = 2.5f;
	public float rightBounds = 10;
	public float leftBounds = 0;

	Vector2 position;
	bool beating = false;

	void Awake()
	{
		tr = GetComponent<TrailRenderer>();
	}
	
	void Update () 
	{
		position = transform.localPosition;
		position.x += scrollSpeed * Time.deltaTime;

		if(position.x > rightBounds)
		{
			tr.Clear();
			position.x = leftBounds;
		}

		if(!beating)
			position.y = 0;

		if(Time.time >= lastBeatTime + 60/bpm)
		{
			StopCoroutine("Beat");
			StartCoroutine("Beat");
		}

		transform.localPosition = position;
	}

	IEnumerator Beat()
	{
		lastBeatTime = Time.time;
		beating = true;

		while(position.y < peakHeight)
		{
			position.y += verticalSpeed * Time.deltaTime;
			transform.localPosition = position;
			yield return new WaitForEndOfFrame();
		}
		while(position.y > -peakHeight)
		{
			position.y -= verticalSpeed * Time.deltaTime;
			transform.localPosition = position;

			yield return new WaitForEndOfFrame();
		}
		while(position.y < 0)
		{
			position.y += verticalSpeed * Time.deltaTime;
			transform.localPosition = position;

			yield return new WaitForEndOfFrame();
		}
		position.y = 0;
		transform.localPosition = position;

		beating = false;
	}
}

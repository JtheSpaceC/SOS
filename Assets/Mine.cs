using UnityEngine;
using System.Collections;

public class Mine : MonoBehaviour {

	public SpriteRenderer flashingLightRenderer;
	public float flashSpeedMin = 0.5f;
	public float flashSpeedMax = 3;
	float flashSpeed;
	float startTime;
	public Color flashColour;



	void Start () 
	{
		flashSpeed = Random.Range(flashSpeedMin, flashSpeedMax);
		Invoke("StartFlashing", Random.Range(0, 0.9f));
	}

	void StartFlashing()
	{
		StartCoroutine(FlashingLight(flashColour));
	}

	IEnumerator FlashingLight(Color colour)
	{
		flashingLightRenderer.color = colour;
		startTime = Time.time;

		while(flashingLightRenderer.color != Color.clear)
		{
			flashingLightRenderer.color = Color.Lerp(colour, Color.clear, (Time.time - startTime)/flashSpeed);
			yield return new WaitForEndOfFrame();
		}

		startTime = Time.time;
		while(flashingLightRenderer.color != colour)
		{
			flashingLightRenderer.color = Color.Lerp(Color.clear, colour, (Time.time - startTime)/flashSpeed);
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine(FlashingLight(flashColour));
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		//send message to other about getting hit

		//destroy this
	}
}

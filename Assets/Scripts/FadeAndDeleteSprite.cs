using UnityEngine;
using System.Collections;

public class FadeAndDeleteSprite : MonoBehaviour {

	SpriteRenderer myRenderer;

	[Tooltip("How long to wait before starting the fade.")]
	public float delayBeforeFade = 2f;

	[Tooltip("How long it should take to fade out once started.")]
	public float fadeDuration = 3f;
	[Tooltip("Higher numbers will start the fade slowly and end it more rapidly, but still over the fadeDuration. Leave at 1 for linear fade")]
	public float powerToStartSlowEndFast = 4;

	[Tooltip("When the object is faded out it will be deleted. Should we also delete the parent of the object this script is on?")]
	public bool deleteMyParent = false;
	
	bool begunFade = false;
	float timer = 0;
	Color startColour;
	Color endColour = Color.clear;
	float startTime;



	void Start () 
	{
		myRenderer = GetComponent<SpriteRenderer> ();
		startColour = myRenderer.color;

		//InvokeRepeating ("Fade", delayBeforeFade, Time.deltaTime);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!begunFade) 
		{
			timer += Time.deltaTime;
		}

		if (!begunFade && timer >= delayBeforeFade) 
		{
			begunFade = true;
			startTime = Time.time;
		}

		if (begunFade) 
		{
			Color lerpedColourValue = Color.Lerp (startColour, endColour, Mathf.Pow((Time.time - startTime)/ fadeDuration, powerToStartSlowEndFast));
			Fade (lerpedColourValue);
		}

		if (begunFade && myRenderer.color == Color.clear || myRenderer.enabled == false)
		{
			if(deleteMyParent)
				Destroy(transform.parent.gameObject);
			else
				Destroy (gameObject);
		}
	}


	void Fade(Color lerpVal)
	{
		myRenderer.color = lerpVal;
	}
}

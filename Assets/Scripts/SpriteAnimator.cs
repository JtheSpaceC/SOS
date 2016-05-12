using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteAnimator : MonoBehaviour {

	public Sprite[] frames;
	public bool startImmediately = true;
	SpriteRenderer myRenderer;
	public float framesPerSecond = 4f;

	int currentFrame = 0;


	void Start () 
	{
		myRenderer = GetComponent<SpriteRenderer> ();
		currentFrame = 0;

		if(startImmediately)
			StartAnimating();
	}


	public void Animate()
	{
		if (currentFrame >= frames.Length)
		{
			currentFrame = 0;
		}

		myRenderer.sprite = frames [currentFrame];
		currentFrame++;
	}

	public void StartAnimating()
	{
		InvokeRepeating("Animate", 0, 1/framesPerSecond);
	}

	public void StopAnimating()
	{
		currentFrame = 0;
		myRenderer.sprite = frames [currentFrame];
		CancelInvoke("Animate");
	}
}//Mono

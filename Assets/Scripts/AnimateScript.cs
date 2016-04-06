using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimateScript : MonoBehaviour {

	public Sprite[] frames;
	public bool startImmediately = true;
	Image myRenderer;
	public float framesPerSecond = 4f;

	int currentFrame = 0;


	void Start () 
	{
		myRenderer = GetComponent<Image> ();
		currentFrame = 0;


		if(startImmediately)
			StartAnimating();
	}


	public IEnumerator Animate()
	{
		if (currentFrame >= frames.Length)
		{
			currentFrame = 0;
		}

		myRenderer.sprite = frames [currentFrame];
		currentFrame++;

		yield return new WaitForSeconds (1/framesPerSecond);


	}

	public void StartAnimating()
	{
		StartCoroutine (Animate ());
	}

	public void StopAnimating()
	{
		currentFrame = 0;
		myRenderer.sprite = frames [currentFrame];
		StopAllCoroutines ();
	}
}//Mono

using UnityEngine;
using UnityEngine.Events;

public class SpriteAnimator : MonoBehaviour {

	public bool startImmediately = true;
	public bool disableAfterOneCycle = false;

	[Header ("For Sprite Swap")]
	public Sprite[] frames;
	SpriteRenderer myRenderer;
	public float framesPerSecond = 4f;

	int currentFrame = 0;

	[Header("For Color Change")]
	public Color[] myColors;
	Color originalColour;
	Color newColour;
	[Range(0f, 1f)]
	public float newColourBalance = 0.5f;
	public float colourCycleTime = 1;
	float timePerColour;
	float colourReachedTime;
	int currentColourInt;

	public UnityEvent myOnEnableEvents;
	public UnityEvent myOnDisableEvents;



	void OnEnable () 
	{
		myRenderer = GetComponent<SpriteRenderer> ();
		originalColour = myRenderer.color;
		currentFrame = 0;

		if(startImmediately)
		{
			StartAnimatingSpriteSwap();
		}

		timePerColour = colourCycleTime/myColors.Length;

		myOnEnableEvents.Invoke();
	}

	void OnDisable()
	{
		myOnDisableEvents.Invoke();
		StopAnimatingSpriteSwap();
	}

	void Update()
	{
		AnimateColours();
	}


	public void AnimateSpriteSwap()
	{
		if(frames.Length == 0)
		{
			StopAnimatingSpriteSwap();
			return;
		}

		if (currentFrame >= frames.Length)
		{
			if(disableAfterOneCycle)
				this.gameObject.SetActive(false);
			currentFrame = 0;
		}

		myRenderer.sprite = frames [currentFrame];
		currentFrame++;
	}

	public void StartAnimatingSpriteSwap()
	{
		InvokeRepeating("AnimateSpriteSwap", 0, 1/framesPerSecond);
	}

	public void StopAnimatingSpriteSwap()
	{
		currentFrame = 0;
		if(frames.Length >0)
			myRenderer.sprite = frames [currentFrame];
		CancelInvoke("AnimateSpriteSwap");
	}

	public void AnimateColours()
	{
		if(myColors.Length == 0 || myColors.Length == 1)
			return;

		if(Time.time >= colourReachedTime + (colourCycleTime/(float)myColors.Length))
		{
			colourReachedTime = Time.time;
			currentColourInt++;

			if(currentColourInt == myColors.Length)
			{
				currentColourInt = 0;
			}
		}

		if(currentColourInt == myColors.Length-1)
		{
			newColour = Color.Lerp(myColors[currentColourInt], myColors[0], (Time.time - colourReachedTime)/ timePerColour);
		}
		else
		{
			newColour = Color.Lerp(myColors[currentColourInt], myColors[currentColourInt+1], (Time.time - colourReachedTime)/ timePerColour);
		}

		myRenderer.color = Color.Lerp(originalColour, newColour, newColourBalance);
	}


}//Mono

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class SpriteAnimator : MonoBehaviour {

	public bool startImmediately = true;
	public bool looping = false;
	public bool disableGameObjectAfterLoop = false;
	[SerializeField] bool playInReverseOrder = false;

	public enum RendererType {SpriteRenderer, ImageUI};

	[Header ("For Sprite Swap")]

	public RendererType myRendererType;
	[Tooltip("Will the animation play the same even if TimeScale is slowed?")] 
	public bool framerateIndependent = false;

	public Sprite[] frames;
	SpriteRenderer mySpriteRenderer;
	Image myImage;
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

	//for coroutine animation
	bool doAnimation = false;
	float timeOfNextFrame;



	void OnEnable () 
	{
		if(myRendererType == RendererType.SpriteRenderer)
		{
			mySpriteRenderer = GetComponent<SpriteRenderer> ();
			originalColour = mySpriteRenderer.color;
		}
		else if(myRendererType == RendererType.ImageUI)
			myImage = GetComponent<Image>();
			
		currentFrame = 0;

		if(startImmediately)
		{
			if(!framerateIndependent)
				StartAnimatingSpriteSwap();			
			else
				StartCoroutine("AnimateFramerateIndependentSpriteSwap");
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

	public void SetPlayInReverseOrder(bool doReverse)
	{
		playInReverseOrder = doReverse;
		if(playInReverseOrder)
			currentFrame = frames.Length -1;
		else 
			currentFrame = 0;
	}

	void AnimateSpriteSwap()
	{
		if(frames.Length == 0)
		{
			StopAnimatingSpriteSwap();
			return;
		}

		SelectCorrectFrame();
	}

	IEnumerator AnimateFramerateIndependentSpriteSwap()
	{
		doAnimation = true;
		timeOfNextFrame = Time.unscaledTime;
			
		while(doAnimation)
		{
			if(Time.unscaledTime >= timeOfNextFrame)
			{
				SelectCorrectFrame();
				timeOfNextFrame += 1/framesPerSecond;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	void SelectCorrectFrame()
	{
		if ((!playInReverseOrder && currentFrame >= frames.Length) || (playInReverseOrder && currentFrame == 0))
		{
			if(!looping)
			{
				StopAnimatingSpriteSwap();
				if(disableGameObjectAfterLoop)
					this.gameObject.SetActive(false);
			}
			if(!playInReverseOrder)
				currentFrame = 0;
			else 
				currentFrame = frames.Length - 1;
		}

		if(myRendererType == RendererType.SpriteRenderer)
			mySpriteRenderer.sprite = frames [currentFrame];
		else if(myRendererType == RendererType.ImageUI)
			myImage.sprite = frames [currentFrame];

		if(!playInReverseOrder)
			currentFrame++;
		else
			currentFrame--;
	}

	public void StartAnimatingSpriteSwap()
	{
		if(playInReverseOrder)
			currentFrame = frames.Length -1;
		InvokeRepeating("AnimateSpriteSwap", 0, 1/framesPerSecond);
	}

	public void StopAnimatingSpriteSwap()
	{
		doAnimation = false;

		currentFrame = 0;
		if(frames.Length > 0)
		{
			if(myRendererType == RendererType.SpriteRenderer)
				mySpriteRenderer.sprite = frames [currentFrame];
			else if(myRendererType == RendererType.ImageUI)
				myImage.sprite = frames [currentFrame];
		}
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

		mySpriteRenderer.color = Color.Lerp(originalColour, newColour, newColourBalance);

		if(myRendererType == RendererType.SpriteRenderer)
			mySpriteRenderer.color = Color.Lerp(originalColour, newColour, newColourBalance);
		else if(myRendererType == RendererType.ImageUI)
			myImage.color = Color.Lerp(originalColour, newColour, newColourBalance);
	}

}//Mono

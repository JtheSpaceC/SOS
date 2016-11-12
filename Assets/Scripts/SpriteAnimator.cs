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

	public bool switchToSecondaryForControllerInput = false;
	[HideInInspector] public Sprite[] frames;
	public Sprite[] framesPrimary;
	[Tooltip("Only used in conjunction with bool above, if it's True.")] 
	public Sprite[] framesSecondary;
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

	public enum ScaleAnimationType {None, Looping, PlayOnce, PlayOnceAndReturn, PlayOnceAndSetInactive, BounceLooping, BounceOnce};

	[Header("For Scale Animation")]
	public ScaleAnimationType scaleAnimationType;
	public Vector3 startScale = Vector3.one;
	public Vector3 endScale = Vector3.one;
	[Tooltip("Length of time to complete one loop (or half a bounce loop).")]
	public float scaleAnimationDuration = 1;
	bool bouncedOnce = false;


	[Header("Events")]
	public UnityEvent myOnEnableEvents;
	public UnityEvent myOnDisableEvents;

	//for coroutine animation
	bool doAnimation = false;
	float timeOfNextFrame;



	void OnEnable () 
	{
		if(switchToSecondaryForControllerInput && InputManager.instance.inputFrom == InputManager.InputFrom.controller)
			frames = framesSecondary;
		else
			frames = framesPrimary;

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

		if(scaleAnimationType != ScaleAnimationType.None)
		{
			StartCoroutine("ScaleAnimation");
		}
	}

	void OnDisable()
	{
		myOnDisableEvents.Invoke();
		StopAnimatingSpriteSwap();
	}

	void Update()
	{
		AnimateColours();

		if(switchToSecondaryForControllerInput)
		{
			if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
				frames = framesSecondary;
			else if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				frames = framesPrimary;				
		}
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

	IEnumerator ScaleAnimation()
	{
		//do the checks
		
		if(transform.localScale == endScale)
		{
			if((scaleAnimationType == ScaleAnimationType.BounceOnce && !bouncedOnce) ||
				scaleAnimationType == ScaleAnimationType.BounceLooping)
			{
				bouncedOnce = true;
				endScale = startScale;
				startScale = transform.localScale;
			}
			//superfluous check
			/*else if(scaleAnimationType == ScaleAnimationType.Looping)
			{
				
			}*/
		}

		//set starting constants
		transform.localScale = startScale;
		float startTime = Time.time;

		//do the animation
		while(transform.localScale != endScale)
		{
			transform.localScale = Vector3.Lerp(startScale, endScale, (Time.time - startTime)/scaleAnimationDuration);
			yield return new WaitForEndOfFrame();
		}
		//unless we're done with a loop, call again
		if(scaleAnimationType == ScaleAnimationType.PlayOnce || (scaleAnimationType == ScaleAnimationType.BounceOnce && bouncedOnce))
		{
			//do nothing. ends loop
		}
		else if(scaleAnimationType == ScaleAnimationType.PlayOnceAndReturn)
		{
			transform.localScale = startScale;
		}
		else if(scaleAnimationType == ScaleAnimationType.PlayOnceAndSetInactive)
		{
			this.gameObject.SetActive(false);
		}
		else
		{
			StartCoroutine("ScaleAnimation");
		}			
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
		CancelInvoke("AnimateSpriteSwap");

		if(playInReverseOrder)
			currentFrame = frames.Length -1;
		else currentFrame = 0;
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

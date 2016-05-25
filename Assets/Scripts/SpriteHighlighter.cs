using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[RequireComponent (typeof(Collider2D))]
public class SpriteHighlighter : MonoBehaviour {

	SpriteRenderer myRenderer;
	AudioSource myAudioSource;
	Color startColor;
	Shader shaderGUItext;
	Shader shaderSpritesDefault;

	public enum MouseOverBehaviour {None, SlideTransform, ColourChange, Grow, Pop};

	[Header("Mouse Over Behaviour + Variables")]
	public MouseOverBehaviour mouseOverBehaviour;
	public Vector3 slideDistance = new Vector3 (1, 0, 0);
	[Tooltip("How many seconds to reach new position.")]
	public float slideTime = 0.5f;
	[Tooltip("Specify another object to control. If blank, defaults to this.")]
	public Transform objectInQuestion;
	public Color newColour = Color.white;
	[Range(0f, 1f)]
	public float newColourBalance = 0.5f;
	public Vector3 growthAddition = new Vector3 (0.2f, 0.2f, 0);
	public float growTime = 0.1f;
	public Text popupText;
	public bool displayNameOnly = false;
	public string textToDisplay = "";
	public bool playAudioOnOver = false;
	public bool playAudioOnExit = false;
	public UnityEvent myMouseOverEvents;
	public UnityEvent myMouseExitEvents;


	[Header("Do On Click")]
	[Tooltip("For picking up an item or transitioning within room. DON'T USE WHEN CHANGING ROOMS!! Causes Stack Overflow Exception.")]
	public bool doBlink = false;
	[Tooltip("Fading in and out both take this long. Total time is double this.")]
	public float blinkTime = 0.35f;
	public UnityEvent myClickEvents;

	[Header("Debugging")]
	[Tooltip("If true, the hotkey will toggle the sprite whitener on/off")]
	public bool debugMode = false;
	public KeyCode hotkey;

	Vector3 startPos;
	Vector3 animStartPos;
	Vector3 normalScale;
	float transformSlideAnimStartTime;
	float growAnimStartTime;
	float adjustedSlideSpeed;


	void Awake()
	{
		if(objectInQuestion == null)
		{
			objectInQuestion = this.transform;
		}
		myRenderer = objectInQuestion.GetComponent<SpriteRenderer>();
		myAudioSource = GetComponent<AudioSource>();
		startColor = myRenderer.color;
		startPos = transform.position;
		normalScale = objectInQuestion.localScale;

		shaderGUItext = Shader.Find("GUI/Text Shader");
		shaderSpritesDefault = Shader.Find("Sprites/Default"); // or whatever sprite shader is being used
	}


	void Update()
	{
		if(debugMode && Input.GetKeyDown(hotkey))
		{
			if(myRenderer.material.shader == shaderSpritesDefault)
				WhitenSprite();
			else
				NormalizeSprite();
		}
	}

	void OnMouseEnter()
	{
		CAGManager.instance.callingSpriteHightlerScript = this;
		myMouseOverEvents.Invoke();

		if(mouseOverBehaviour == MouseOverBehaviour.ColourChange)
		{
			WhitenSprite();
			myRenderer.color = Color.Lerp(myRenderer.color, newColour, newColourBalance);
		}
		else if(mouseOverBehaviour == MouseOverBehaviour.SlideTransform)
		{
			StopCoroutine("SlideAnimation");
			StartCoroutine("SlideAnimation", startPos + slideDistance);
		}
		else if(mouseOverBehaviour == MouseOverBehaviour.Grow || mouseOverBehaviour == MouseOverBehaviour.Pop)
		{
			StopCoroutine("GrowSprite");
			StartCoroutine("GrowSprite", false);
		}

		if(playAudioOnOver && myAudioSource != null)
		{
			myAudioSource.Play();
		}

		if(popupText != null && displayNameOnly)
			popupText.text = objectInQuestion.name;
		else if(popupText != null && textToDisplay != "")
			popupText.text = textToDisplay;
	}

	void OnMouseExit()
	{
		myMouseExitEvents.Invoke();

		if(mouseOverBehaviour == MouseOverBehaviour.ColourChange)
		{
			NormalizeSprite();
		}
		else if(mouseOverBehaviour == MouseOverBehaviour.SlideTransform)
		{
			StopCoroutine("SlideAnimation");
			StartCoroutine("SlideAnimation", startPos);
		}
		else if(mouseOverBehaviour == MouseOverBehaviour.Grow)
		{
			StopCoroutine("GrowSprite");
			StartCoroutine("GrowSprite", true);
		}

		if(playAudioOnExit && myAudioSource != null)
		{
			myAudioSource.Play();
		}

		if(popupText != null)
			popupText.text = "";
	}

	void OnMouseDown()
	{
		if(doBlink)
		{
			CAGManager.instance.SetFadeToClearAfterBlack();
			CAGManager.instance.CallFadeToBlack(blinkTime);
		}
		else
			myClickEvents.Invoke();
	}

	IEnumerator SlideAnimation(Vector3 destination)
	{
		animStartPos = objectInQuestion.position;
		transformSlideAnimStartTime = Time.time;
		adjustedSlideSpeed = slideTime * (Vector3.Distance(objectInQuestion.position, destination)/ 
											Vector3.Distance(startPos, startPos + slideDistance));

		while(objectInQuestion.position != destination)
		{
			objectInQuestion.position = Vector3.Lerp(animStartPos, destination, (Time.time - transformSlideAnimStartTime)/adjustedSlideSpeed);
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator GrowSprite(bool shrinkNow)
	{
		growAnimStartTime = Time.time;

		if(!shrinkNow)
		{
			while(objectInQuestion.localScale != normalScale + growthAddition)
			{
				objectInQuestion.localScale = Vector3.Lerp(normalScale, normalScale + growthAddition, (Time.time - growAnimStartTime)/growTime);
				yield return new WaitForEndOfFrame();
			}
		}
		if(mouseOverBehaviour == MouseOverBehaviour.Pop || shrinkNow)
		{
			growAnimStartTime = Time.time;
			transform.localScale = normalScale + growthAddition;

			while(objectInQuestion.localScale != normalScale)
			{
				objectInQuestion.localScale = Vector3.Lerp(normalScale + growthAddition, normalScale, (Time.time - growAnimStartTime)/growTime);
				yield return new WaitForEndOfFrame();
			}
		}
	}

	public void ResetMovingParts()
	{
		objectInQuestion.transform.position = startPos;
	}

	public void WhitenSprite() 
	{
		myRenderer.material.shader = shaderGUItext;
		myRenderer.color = Color.white;
	}

	public void NormalizeSprite() 
	{
		myRenderer.material.shader = shaderSpritesDefault;
		myRenderer.color = startColor;
	}

	void OnDisable()
	{
		objectInQuestion.localScale = normalScale;
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Subtitles : MonoBehaviour {

	public static Subtitles instance;

	AudioSource myAudioSource;
	public Animator hintsAnimator;

	public Color desiredPanelColour;

	public Image subtitlesPanel;
	public Text subtitlesText;

	public Image hintsPanel;
	public Text hintsText;
	public Text secondHintsText;
	public Image controlImage;

	public float fadeInTime = 0.35f;
	public float fadeOutTime = 1.5f;
	public float timeToReadPerLetter = 0.03f;

	float startTimeSubs;
	float startTimeHints;

	public bool fadeInSubtitles = false;
	public bool fadeOutSubtitles = false;
	public bool fadeInSubtitlesStarted = false;
	public bool fadeOutSubtitlesStarted = false;

	public bool fadeInHints = false;
	public bool fadeOutHints = false;
	public bool fadeInHintsStarted = false;
	public bool fadeOutHintsStarted = false;

	bool hintNoiseOnCooldown = false;
	bool hintPopHighlightIsOnCooldown = false;


	void Awake () 
	{
		if(instance == null)
		{
			instance = this;
		}
		else 
		{
			Debug.LogError("There were two Subtitle objects. Destroying "+gameObject.name);
			Destroy(gameObject);
		}

		myAudioSource = GetComponent<AudioSource> ();

		subtitlesPanel.color = Color.clear;
		subtitlesText.color = Color.clear;

		hintsPanel.color = Color.clear;
		hintsText.color = Color.clear;
	}


	void Update()
	{
		//for FADING IN SUBTITLES

		if(fadeInSubtitles && !fadeInSubtitlesStarted && subtitlesPanel.color.a < desiredPanelColour.a)
		{
			startTimeSubs = Time.time;
			fadeOutSubtitles = false;
			fadeOutSubtitlesStarted = false;
			
			fadeInSubtitlesStarted = true;
		}
		else if(fadeInSubtitles && fadeInSubtitlesStarted && subtitlesPanel.color != desiredPanelColour)
		{
			subtitlesPanel.color = Color.Lerp(Color.clear, desiredPanelColour, (Time.time - startTimeSubs) / fadeInTime);
			subtitlesText.color = Color.Lerp(Color.clear, Color.white, (Time.time - startTimeSubs) / fadeInTime);
		}
		else if(fadeInSubtitles && subtitlesPanel.color == desiredPanelColour)
		{
			fadeInSubtitlesStarted = false;
			fadeInSubtitles = false;
		}

		//for FADING OUT SUBTITLES

		if(fadeOutSubtitles && !fadeOutSubtitlesStarted && subtitlesPanel.color != Color.clear)
		{
			startTimeSubs = Time.time;
			fadeInSubtitles = false;
			fadeInSubtitlesStarted = false;

			fadeOutSubtitlesStarted = true;
		}
		else if(fadeOutSubtitles && fadeOutSubtitlesStarted && subtitlesPanel.color != Color.clear)
		{
			subtitlesPanel.color = Color.Lerp(desiredPanelColour, Color.clear, (Time.time - startTimeSubs) / fadeOutTime);
			subtitlesText.color = Color.Lerp(Color.white, Color.clear, (Time.time - startTimeSubs) / fadeOutTime);
		}
		else if(fadeOutSubtitles && subtitlesPanel.color == Color.clear)
		{
			fadeOutSubtitlesStarted = false;
			fadeOutSubtitles = false;
		}

		//for FADING IN HINTS

		if(fadeInHints && !fadeInHintsStarted && hintsPanel.color.a < desiredPanelColour.a)
		{
			startTimeHints = Time.time;
			fadeOutHints = false;
			fadeOutHintsStarted = false;
			
			fadeInHintsStarted = true;
		}
		else if(fadeInHints && fadeInHintsStarted && hintsPanel.color != desiredPanelColour)
		{
			hintsPanel.color = Color.Lerp(Color.clear, desiredPanelColour, (Time.time - startTimeHints) / fadeInTime);
			hintsText.color = Color.Lerp(Color.clear, Color.white, (Time.time - startTimeHints) / fadeInTime);
		}
		else if(fadeInHints && hintsPanel.color == desiredPanelColour)
		{
			fadeInHintsStarted = false;
			fadeInHints = false;
		}
		
		//for FADING OUT HINTS
		
		if(fadeOutHints && !fadeOutHintsStarted && hintsPanel.color != Color.clear)
		{
			startTimeHints = Time.time;
			fadeInHints = false;
			fadeInHintsStarted = false;
			
			fadeOutHintsStarted = true;
		}
		else if(fadeOutHints && fadeOutHintsStarted && hintsPanel.color != Color.clear)
		{
			hintsPanel.color = Color.Lerp(desiredPanelColour, Color.clear, (Time.time - startTimeHints) / fadeOutTime);
			hintsText.color = Color.Lerp(Color.white, Color.clear, (Time.time - startTimeHints) / fadeOutTime);
		}
		else if(fadeOutHints && hintsPanel.color == Color.clear)
		{
			fadeOutHintsStarted = false;
			fadeOutHints = false;
		}
	}//end of UPDATE


	public void PostSubtitle(string[] potentialMessages)
	{
		string message = potentialMessages[Random.Range(0, potentialMessages.Length)];
		if(subtitlesPanel.color.a < desiredPanelColour.a)
		{
			fadeInSubtitles = true;
		}
		CancelInvoke ("FadeOutSubtitles");

		subtitlesText.text = message;
		Invoke("FadeOutSubtitles", ((message.ToCharArray().Length * timeToReadPerLetter) + 1.5f ));
	}


	public void PostHint(string[] potentialMessages)
	{
		string message = potentialMessages[Random.Range(0, potentialMessages.Length)];
		if(hintsPanel.color.a < desiredPanelColour.a)
		{
			fadeInHints = true;
		}
		CancelInvoke ("FadeOutHints");
		
		hintsText.text = message;
		Invoke("FadeOutHints", ((message.ToCharArray().Length * timeToReadPerLetter) + 1.5f ));
	
		if(!myAudioSource.isPlaying && !hintNoiseOnCooldown)
			myAudioSource.Play ();
		if(!hintPopHighlightIsOnCooldown)
			hintsAnimator.SetTrigger("Highlight");
	}

	public void CoolDownHintNoise()
	{
		if (!hintNoiseOnCooldown) {
			hintNoiseOnCooldown = true;
			Invoke("ReactivateHintNoise", 4);
		}
	}
	void ReactivateHintNoise()
	{
		hintNoiseOnCooldown = false;
	}

	public void CoolDownHintHighlight()
	{
		if(!hintPopHighlightIsOnCooldown){
			hintPopHighlightIsOnCooldown = true;
			Invoke("ReactivateHintHighlight", 4);
		}
	}
	void ReactivateHintHighlight()
	{
		hintPopHighlightIsOnCooldown = false;
	}

	void FadeOutSubtitles()
	{
		if(subtitlesPanel.color != Color.clear)
		{
			fadeOutSubtitles = true;
		}
	}

	void FadeOutHints()
	{
		if(hintsPanel.color != Color.clear)
		{
			fadeOutHints = true;
		}
	}


	void OnDisable()
	{
		subtitlesPanel.color = Color.clear;
		subtitlesText.color = Color.clear;
		hintsPanel.color = Color.clear;
		hintsText.color = Color.clear;
	}
}

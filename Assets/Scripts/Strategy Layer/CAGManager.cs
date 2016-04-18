using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class CAGManager : MonoBehaviour {

	public static CAGManager instance;

	[HideInInspector] public SpriteHighlighter callingSpriteHightlerScript;

	public Image blackoutPanel;
	public Text contextualText;

	bool fadeToClearAfterBlack = false;

	public UnityEvent leaveQuartersEvents;

	[Header("Rooms")]
	public GameObject quarters;
	public Image quartersHeaderSprite;
	public GameObject brig;
	public Image brigHeaderSprite;
	public GameObject squadronHQ;
	public Image squadronHQHeaderSprite;
	public GameObject quartermaster;
	public Image quartermasterHeaderSprite;
	public GameObject bar;
	public Image barHeaderSprite;


	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were two CAGManagers. Destroying one.");
			Destroy(this.gameObject);
		}
	}

	public void SetCallingSpriteHightlerScript(SpriteHighlighter spriteHighlighter)
	{
		callingSpriteHightlerScript = spriteHighlighter;
	}

	public void NewDay()
	{
		CAGDirector.instance.gameDay++;
		CAGDirector.instance.wearingClothes = false;
	}

	public void PutOnJumpsuit()
	{
		CAGDirector.instance.wearingClothes = true;
	}

	public void CheckQuartersDoor()
	{
		if(!CAGDirector.instance.wearingClothes)
		{
			callingSpriteHightlerScript.textToDisplay = "Woah, sir! You're Naked!";
		}
		else
		{
			callingSpriteHightlerScript.textToDisplay = "Start Your Rounds";
		}
	}
	public void LeaveQuarters()
	{
		if(CAGDirector.instance.wearingClothes)
		{
			CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
			Invoke("SetAllRoomsInactive", callingSpriteHightlerScript.blinkTime);
		}
	}

	void SetAllRoomsInactive()
	{
		quarters.SetActive(false);
		quartersHeaderSprite.color = Color.white;
		brig.SetActive(false);
		brigHeaderSprite.color = Color.white;
		squadronHQ.SetActive(false);
		squadronHQHeaderSprite.color = Color.white;
		quartermaster.SetActive(false);
		quartermasterHeaderSprite.color = Color.white;
		bar.SetActive(false);
		barHeaderSprite.color = Color.white;
		contextualText.text = "";

		//set an active room
		CAGDirector.instance.ActivateNextRoom();

		//fade in
		CallFadeToClear(callingSpriteHightlerScript.blinkTime);
	}

	#region Fade/Blackout Functions
	public void CallFadeToBlack(float fadeTime)
	{
		FadeTimes myFadeTime = new FadeTimes();
		myFadeTime.startTime = Time.time;
		myFadeTime.fadeTime = fadeTime;
		StartCoroutine("FadeToBlack", myFadeTime);
	}

	public void CallFadeToClear(float fadeTime)
	{
		FadeTimes myFadeTime = new FadeTimes();
		myFadeTime.startTime = Time.time;
		myFadeTime.fadeTime = fadeTime;
		StartCoroutine("FadeToClear", myFadeTime);
	}

	public IEnumerator FadeToBlack(FadeTimes fadeInfo)
	{
		while(blackoutPanel.color != Color.black)
		{
			blackoutPanel.color = Color.Lerp(Color.clear, Color.black, (Time.time - fadeInfo.startTime)/fadeInfo.fadeTime);
			yield return new WaitForEndOfFrame();
		}
		if(fadeToClearAfterBlack)
		{
			callingSpriteHightlerScript.myClickEvents.Invoke();
			CallFadeToClear(fadeInfo.fadeTime);
			fadeToClearAfterBlack = false;
		}
	}

	public IEnumerator FadeToClear(FadeTimes fadeInfo)
	{
		while(blackoutPanel.color != Color.clear)
		{
			blackoutPanel.color = Color.Lerp(Color.black, Color.clear, (Time.time - fadeInfo.startTime)/fadeInfo.fadeTime);
			yield return new WaitForEndOfFrame();
		}
	}

	public void SetFadeToClearAfterBlack()
	{
		fadeToClearAfterBlack = true;
	}
	#endregion
}

public class FadeTimes
{
	public float startTime;
	public float fadeTime;
}

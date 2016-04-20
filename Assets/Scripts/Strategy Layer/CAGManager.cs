using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class CAGManager : MonoBehaviour {

	public static CAGManager instance;

	[HideInInspector] public SpriteHighlighter callingSpriteHightlerScript;

	public Image blackoutPanel;
	public Text contextualText;
	public Transform cameraBackButton;

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

	Vector3 cameraRestorePosition;


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

	#region Leave Various Room Conditions

	public void LeaveQuarters()
	{
		if(CAGDirector.instance.wearingClothes)
		{
			CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
			Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime);
		}
	}
	public void LeaveBrig()
	{
		CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
		Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime );
	}
	public void LeaveSquadronHQ()
	{
		CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
		Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime);
	}
	public void LeaveQuartermaster()
	{
		CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
		Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime);
	}
	public void LeaveBar()
	{
		CAGDirector.instance.NewDay();
		CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
		Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime);

		//TODO: Put in an end of day info panel
	}
	#endregion

	void CallLoadNextRoom()
	{
		//reset moving sprites like open doors
		callingSpriteHightlerScript.ResetMovingParts();

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

	public void CameraCloseup(Transform who)
	{
		cameraRestorePosition = Camera.main.transform.position;
		Camera.main.transform.position = new Vector3 (who.position.x, who.position.y, Camera.main.transform.position.z);
		Camera.main.orthographicSize /= 3;
		cameraBackButton.localScale = new Vector3(cameraBackButton.localScale.x, Camera.main.orthographicSize/3, 1);
		cameraBackButton.localPosition = new Vector3(0, - Camera.main.orthographicSize, cameraBackButton.localPosition.z);
		cameraBackButton.GetComponent<Collider2D>().enabled = true;

		EnablePrisonerColliders(false);
	}

	public void CameraRestore()
	{
		Camera.main.transform.position = cameraRestorePosition;
		Camera.main.orthographicSize *= 3;
		cameraBackButton.GetComponent<Collider2D>().enabled = false;

		EnablePrisonerColliders(true);
	}

	public void EnablePrisonerColliders(bool trueOrFalse)
	{
		BrigPerson[] brigPeople = Object.FindObjectsOfType<BrigPerson>();

		foreach(BrigPerson brigPerson in brigPeople)
		{
			brigPerson.myCollider.enabled = trueOrFalse;
		}
	}
}

public class FadeTimes
{
	public float startTime;
	public float fadeTime;
}

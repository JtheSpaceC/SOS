using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;

public class CAGManager : MonoBehaviour {

	public static CAGManager instance;

	[HideInInspector] public SpriteHighlighter callingSpriteHightlerScript;

	public Image blackoutPanel;
	public Text contextualText;
	public Text dayText;
	public Transform cameraBackButton;
	public Text objectivesText;
	public Text missionsText;

	bool fadeToClearAfterBlack = false;

	Vector3 cameraRestorePosition;

	public int gameDay = 1;
	public int currentRoom = 0;

	[Header("Room specific stuff")]

	public Room[] rooms;
	[HideInInspector] public bool wearingClothes = false;
	[HideInInspector] public bool tookLaptop = false;
	public GameObject jumpsuit;
	public GameObject laptop;
	public GameObject map;
	public GameObject mapInfoPanel;
	public Text mapInfoHeaderText;
	public Text mapInfoBodyText;
	public Button reconButton;
	public GameObject podium;
	public GameObject pilotAssignment;
	public GameObject squadronHQExit;




	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were two CAGManagers. Destroying one.");
			DestroyImmediate(this.gameObject);
		}
	}

	void Start()
	{
		LoadStatus();
		ActivateNextRoom();
	}


	public void NewDay()
	{
		gameDay++;
		ResetQuarters();
		currentRoom = 0;
	}
	void ResetQuarters()
	{
		jumpsuit.GetComponent<SpriteRenderer>().enabled = true;
		jumpsuit.GetComponent<BoxCollider2D>().enabled = true;
		laptop.GetComponent<SpriteRenderer>().enabled = true;
		laptop.GetComponent<Collider2D>().enabled = true;
		wearingClothes = false;
		laptop.SetActive(true);
		tookLaptop = false;
	}
	public void PutOnJumpsuit()
	{
		wearingClothes = true;
		jumpsuit.GetComponent<SpriteRenderer>().enabled = false;
		jumpsuit.GetComponent<BoxCollider2D>().enabled = false;
		jumpsuit.GetComponent<AudioSource>().Play();
	}
	public void PickUpLaptop()
	{
		laptop.GetComponent<SpriteRenderer>().enabled = false;
		laptop.GetComponent<Collider2D>().enabled = false;
		laptop.GetComponent<AudioSource>().Play();
		//TODO: PopulateMissionsAndObjectives()
		objectivesText.enabled = true;
		missionsText.enabled = true;
		tookLaptop = true;
	}

	public void PilotAssignmentScreen()
	{
		pilotAssignment.SetActive(true);
	}
	public void EnableLeaveSquadronHQ(bool trueOrFalse)
	{
		squadronHQExit.GetComponent<SpriteRenderer>().enabled = trueOrFalse;
		squadronHQExit.GetComponent<BoxCollider2D>().enabled = trueOrFalse;
		podium.SetActive(!trueOrFalse);
	}

	public void ActivateNextRoom()
	{
		DeactivateRooms();
		rooms[currentRoom].gameObject.SetActive(true);
		rooms[currentRoom].myHeaderIcon.color = Color.green;

		currentRoom++;

		CAGManager.instance.dayText.text = "Day " + gameDay;
	}
	void DeactivateRooms()
	{
		foreach(Room room in rooms)
		{
			room.gameObject.SetActive(false);
			room.myHeaderIcon.color = Color.white;
		}
		CAGManager.instance.contextualText.text = "";

		ES2.DeleteDefaultFolder();
	}

	public void SetCallingSpriteHightlerScript(SpriteHighlighter spriteHighlighter)
	{
		callingSpriteHightlerScript = spriteHighlighter;
	}
		
	public void CheckQuartersDoor()
	{
		if(!wearingClothes)
		{
			callingSpriteHightlerScript.textToDisplay = "Woah, sir! You're Naked!";
		}
		else if(!tookLaptop)
		{
			callingSpriteHightlerScript.textToDisplay = "I think you should check the morning intel, Sir.";
		}
		else
		{
			callingSpriteHightlerScript.textToDisplay = "Head To Bridge";
		}
	}

	#region Leave Various Room Conditions

	public void LeaveQuarters()
	{
		if(wearingClothes && tookLaptop)
		{
			CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
			Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime);
		}
	}
	public void LeaveBridge()
	{
		CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
		Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime );
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
		EnableLeaveSquadronHQ(false);
	}
	public void LeaveBar()
	{
		NewDay();
		CallFadeToBlack(callingSpriteHightlerScript.blinkTime);
		Invoke("CallLoadNextRoom", callingSpriteHightlerScript.blinkTime);

		//TODO: Put in an end of day info panel
	}
	#endregion

	void CallLoadNextRoom()
	{
		//set an active room
		ActivateNextRoom();

		//fade in
		CallFadeToClear(callingSpriteHightlerScript.blinkTime);
	}

	public void StartMission()
	{
		ClickToPlay.instance.LoadScene("test");
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

	public void SaveStatus()
	{
		ES2.Save(gameDay, "Day");
		ES2.Save(currentRoom, "Room");

		//If Easy Save stops functioning, comment this back in and comment out ES2

		/*PlayerPrefs.SetInt("Day", gameDay);
		PlayerPrefs.SetInt("Room", currentRoom);
		PlayerPrefs.Save();*/
	}

	public void LoadStatus()
	{
		print("Loading");

		//If Easy Save stops functioning, comment this back in and comment out ES2

		/*if(PlayerPrefs.GetInt("Day") != 0)
		{
			gameDay = PlayerPrefs.GetInt("Day");
		}
		else Debug.Log("Day not saved.");

		if(PlayerPrefs.GetInt("Room") != 0)
		{
			currentRoom = PlayerPrefs.GetInt("Room") - 1; 
		}
		else Debug.Log("Room not saved.");*/
		if(ES2.Exists("Day"))
		{
			Debug.Log("Day Exists");
			gameDay = ES2.Load<int>("Day");;
		}
		else Debug.Log("Day Doesn't Exist");

		if(ES2.Exists("Room"))
		{
			Debug.Log("Room Exists");
			currentRoom = ES2.Load<int>("Room") - 1;
		}
		else Debug.Log("Room Doesn't Exist");
	}

	[ContextMenu("Delete Saves")]
	public void DeleteSaves()
	{
		//If Easy Save stops functioning, comment this back in and comment out ES2

		//PlayerPrefs.DeleteKey("Day");
		//PlayerPrefs.DeleteKey("Room");

		if(ES2.Exists("Day"))
		{
			ES2.Delete("Day");
			ES2.Delete("Room");

			ES2.DeleteDefaultFolder();
			Debug.Log("Deleting Default Folder");
		}
		else print("Saves didn't exist");

	}

	void OnDisable()
	{
		SaveStatus();
	}



}//end of CAGManager


public class FadeTimes
{
	public float startTime;
	public float fadeTime;
}


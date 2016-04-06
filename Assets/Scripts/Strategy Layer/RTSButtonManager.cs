using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RTSButtonManager : MonoBehaviour {

	public static RTSButtonManager instance;

	[HideInInspector] public RTSCamera rtsCamera;

	public Transform contextMenu;
	[Tooltip("This is an invisible panel covering the whole screen. It allows you to click away from menus to cancel them.")]
	public List<GameObject> subButtons;

	[Header("Planet Info")]
	public GameObject objectInfoPanel;
	public Text objectNameText;
	public Image objectPictureImage;
	public Text descriptionText;
	public Scrollbar descriptionScrollbar;
	public Image scrollBarHandle;
	public Text valueText;
	public Button garrisonButton;
	public GameObject garrisonCraftPanel;
	public GameObject garrisonTroopersPanel;

	[Header("Context Screens")]
	public GameObject agentsPanel;
	public GameObject fullReportPanel;
	public GameObject counterIntelligencePanel;
	public GameObject activityLogPanel;
	public GameObject manageFlaghsipPanel;
	public GameObject requisitionsPanel;
	public GameObject incomingMessagePanel;
	public Button[] disableWhenMessageIncoming;

	string buttonNameFromText;
	Vector3 newPosition;

	public bool menusAreShown = false;
	[HideInInspector] public bool objectInfoWasShown = false;


	void Awake () 
	{
		if (instance == null)
			instance = this;

		newPosition = contextMenu.position;
		rtsCamera = Camera.main.GetComponent<RTSCamera> ();
	}


	public void TurnOffMenus()
	{
		contextMenu.gameObject.SetActive (false);
		menusAreShown = false;

		if (objectInfoWasShown)
		{
			HideAllMenuAndInfoScreens ();
		}
		else if (objectInfoPanel.activeSelf || 
		         agentsPanel.activeSelf || fullReportPanel.activeSelf || counterIntelligencePanel.activeSelf || activityLogPanel.activeSelf
		         ||manageFlaghsipPanel.activeSelf || requisitionsPanel.activeSelf)
		{
			objectInfoWasShown = true;
			rtsCamera.canZoom = false;
			rtsCamera.canMove = false;
			menusAreShown = true;
		}
	}

	public void WhichPrimaryButtonGotPressed(GameObject buttonGameObject)
	{
		if (objectInfoWasShown)
		{
			HideAllMenuAndInfoScreens();
			return;
		}
		contextMenu.gameObject.SetActive (true);
		menusAreShown = true;
		rtsCamera.canZoom = false;
		rtsCamera.canMove = false;
		newPosition = contextMenu.position;
		newPosition.x = buttonGameObject.transform.position.x;
		contextMenu.position = newPosition;

		foreach (GameObject button in subButtons)
			button.SetActive (false);


		buttonNameFromText = buttonGameObject.GetComponentInChildren<Text> ().text;

		switch (buttonNameFromText.ToLower())
		{
		default:
			Debug.LogError("Wrong Button Name");
			break;
		case "intel" :
			subButtons[0].SetActive(true);
			subButtons[0].GetComponentInChildren<Text>().text = "AGENTS";
			subButtons[1].SetActive(true);
			subButtons[1].GetComponentInChildren<Text>().text = "FULL REPORT";
			subButtons[2].SetActive(true);
			subButtons[2].GetComponentInChildren<Text>().text = "COUNTER-\nINTELLIGENCE";
			subButtons[3].SetActive(true);
			subButtons[3].GetComponentInChildren<Text>().text = "ACTIVITY LOG";
			break;
		case "quartermaster":
			subButtons[0].SetActive(true);
			subButtons[0].GetComponentInChildren<Text>().text = "MANAGE FLAGSHIP";			
			subButtons[1].SetActive(true);
			subButtons[1].GetComponentInChildren<Text>().text = "REQUISITIONS";
			break;
		case "personnel":
			subButtons[0].SetActive(true);
			subButtons[0].GetComponentInChildren<Text>().text = "SQUADRONS";
			subButtons[1].SetActive(true);
			subButtons[1].GetComponentInChildren<Text>().text = "SECURITY TEAMS";
			break;
		case "fleet":
			subButtons[0].SetActive(true);
			subButtons[0].GetComponentInChildren<Text>().text = "1ST FLEET";
			subButtons[1].SetActive(true);
			subButtons[1].GetComponentInChildren<Text>().text = "DEFENSIVE PLATFORMS";
			subButtons[2].SetActive(true);
			subButtons[2].GetComponentInChildren<Text>().text = "RECON DRONES";
			break;
		case "finances":
			subButtons[0].SetActive(true);
			subButtons[0].GetComponentInChildren<Text>().text = "PROFIT & LOSS REPORT";
			subButtons[1].SetActive(true);
			subButtons[1].GetComponentInChildren<Text>().text = "CONTRACTS";
			break;
		}
	}//end of PrimaryButtonGotPressed


	public void WhichSecondaryButtonGotPressed(GameObject buttonGameObject)
	{
		buttonNameFromText = buttonGameObject.GetComponentInChildren<Text> ().text;
		
		switch (buttonNameFromText.ToLower())
		{
		default:
			Debug.LogError("Wrong Button Name");
			break;
		case "agents":
			agentsPanel.SetActive(true);
			break;
		case "full report":
			fullReportPanel.SetActive(true);
			break;
		case "counter-\nintelligence":
			counterIntelligencePanel.SetActive(true);
			break;
		case "activity log":
			activityLogPanel.SetActive(true);
			break;
		case "manage flagship":
			manageFlaghsipPanel.SetActive(true);
			break;
		case "requisitions":
			requisitionsPanel.SetActive(true);
			break;
		case "squadrons":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;
		case "security teams":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;
		case "1st fleet":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;
		case "defensive platforms":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;
		case "recon drones":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;
		case "profit & loss report":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;
		case "contracts":
			Subtitles.instance.PostHint(new string[] {"Not Yet Functional"});
			objectInfoWasShown = true;
			break;

		}
		RTSDirector.instance.ChangeGameSpeed (0);
		rtsCamera.canZoom = false;
		rtsCamera.canMove = false;
		TurnOffMenus ();
	}//end of secondaryButtonGotPressed

	public void ShowObjectInfo(GameObject whichObject)
	{
		menusAreShown = true;
		rtsCamera.canZoom = false;
		rtsCamera.canMove = false;
		objectInfoPanel.SetActive (true);
		RTSDirector.instance.ChangeGameSpeed (0);

		objectNameText.text = whichObject.name;
		descriptionText.text = whichObject.GetComponent<RTSObject> ().desciption;
		objectPictureImage.sprite = whichObject.GetComponent<SpriteRenderer> ().sprite;
		objectPictureImage.color = whichObject.GetComponent<SpriteRenderer> ().color;
		whichObject.SendMessage ("GetMyInfo", SendMessageOptions.DontRequireReceiver);

		descriptionScrollbar.value = 1;
		StartCoroutine (CheckScrollbarSize ());
	}
	IEnumerator CheckScrollbarSize()
	{
		yield return new WaitForEndOfFrame ();
		if (descriptionScrollbar.size == 1) 
		{
			descriptionScrollbar.gameObject.SetActive (false);
		}
		else
		{
			descriptionScrollbar.GetComponent<Image> ().enabled = true;
			scrollBarHandle.enabled = true;
		}
	}

	public void HideAllMenuAndInfoScreens()
	{
		menusAreShown = false;
		rtsCamera.canZoom = true;
		rtsCamera.canMove = true;
		objectInfoWasShown = false;
		descriptionScrollbar.gameObject.SetActive (true);
		descriptionScrollbar.GetComponent<Image> ().enabled = false;
		scrollBarHandle.enabled = false;
		valueText.text = "";
		objectInfoPanel.SetActive (false);

		agentsPanel.SetActive (false);
		fullReportPanel.SetActive (false);
		counterIntelligencePanel.SetActive (false);
		activityLogPanel.SetActive (false);
		manageFlaghsipPanel.SetActive (false);
		requisitionsPanel.SetActive (false);

		RTSDirector.instance.ChangeGameSpeed (1);
	}

	public void OpenCloseIncomingMessageMenu(bool trueForOnFalseForOff)
	{
		HideAllMenuAndInfoScreens ();

		incomingMessagePanel.SetActive (trueForOnFalseForOff);
		contextMenu.gameObject.SetActive(false);
		rtsCamera.canZoom = !trueForOnFalseForOff;
		rtsCamera.canMove = !trueForOnFalseForOff;
		menusAreShown = trueForOnFalseForOff;

		if (trueForOnFalseForOff)
			RTSDirector.instance.ChangeGameSpeed (0);
		else
			RTSDirector.instance.ChangeGameSpeed (1);
		
		foreach(Button but in disableWhenMessageIncoming)
		{
			but.interactable = !trueForOnFalseForOff;
		}
	}

	public void SwitchAllGarrisonInfoOnOff(bool setTo)
	{
		garrisonButton.gameObject.SetActive (setTo);
		garrisonCraftPanel.SetActive (setTo);
		garrisonTroopersPanel.SetActive (setTo);
	}


}//Mono

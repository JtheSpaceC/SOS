﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RadialRadioMenu : MonoBehaviour {

	public static RadialRadioMenu instance;

	AudioSource myAudioSource;

	public enum RadialScreens 
	{
		Default,
		OpenAChannel,
		Squadron, AllWingmen, FirstWingman, SecondWingman,
		Support, Extraction
	};
	public RadialScreens currentRadialScreen;

	public List<RadialScreens> screenProgression = new List<RadialScreens>();

	// these are matched in a switch statement in RadialOption
	public string[] Orders = new string[] {"Form Up", "Cover Me", "Fall Back", "Return To Base", "Engage At Will"}; 

	public GameObject radialOptionPrefab;
	public GameObject radialDivideBarPrefab;
	public GameObject radialGuideArrowPrefab;

	public bool canAccessRadialRadio = true;
	bool radioHasOptions = true;

	List<RadialOption> activeRadialOptions = new List<RadialOption>();

	[HideInInspector] public List<AIFighter> selectedWingmen = new List<AIFighter>(); //this will be set from RadialOption each time one is selected

	Canvas radialMenuCanvas;
	public Transform radialMenuCentralPanel;
	public Text headerText; //for instruction like "pick the craft"
	public Text centralText; //for detail on the currently selected option

	[HideInInspector] public bool radialMenuShown = false;
	RadialOption selectedOption;
	GameObject radialGuideArrow;
	Vector2 cursorPos;
	public float cursorsRotation;
	float keyboardLedRotation; //for rotatin on the radial menu with arrow keys
	bool takeDPadInput = true;

	bool shouldToggleEscMenu = false;


	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were two Radial Radio Menus. Deleting one.");
			Destroy(gameObject);
		}

		radialMenuCanvas = GetComponent<Canvas>();
		myAudioSource = GetComponent<AudioSource>();
	}


	void Start()
	{
		if(ClickToPlay.instance.escCanGiveQuitMenu)
			shouldToggleEscMenu = true;
	}


	void Update()
	{
		if(ClickToPlay.instance.paused || !canAccessRadialRadio)
			return;

		//ACTIVATE the Radial menu
		if(!radialMenuShown && 
			(Input.GetKeyDown(KeyCode.Q) || 
				((Input.GetAxis("Orders Vertical")) > 0.5f) && takeDPadInput ) )
		{
			StartCoroutine("DPadInputWait");
			screenProgression.Clear();
			Tools.instance.StopCoroutine("FadeScreen");
			Tools.instance.MoveCanvasToFront(Tools.instance.blackoutCanvas);
			Tools.instance.MoveCanvasToFront(radialMenuCanvas);
			Tools.instance.blackoutPanel.color = Color.Lerp (Color.black, Color.clear, 0.1f);
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", -15f);
			Tools.instance.AlterTimeScale(0.1f);
			PlayerAILogic.instance.TogglePlayerControl(true, false, false, false);
			CameraTactical.instance.canAccessTacticalMap = false;

			currentRadialScreen = RadialScreens.OpenAChannel;
			screenProgression.Add(currentRadialScreen);

			headerText.enabled = true;
			centralText.enabled = true;
			PopulateRadialMenuOptions(currentRadialScreen);
			myAudioSource.Play();
			if(shouldToggleEscMenu)
				ClickToPlay.instance.escCanGiveQuitMenu = false;

			radialMenuShown = true;
		}

		//DEACTIVATE the Radial menu
		else if(radialMenuShown && 
			(Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Orders Vertical")) > 0.5f && takeDPadInput))
		{
			StartCoroutine("DPadInputWait");
			DeactivateRadialMenu ();
		}

		//OPERATE the Radial menu
		if(radialMenuShown && radioHasOptions)
		{
			//if using controller
			if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
			{
				//cursor pos is notional (made up), but the guide arrow object uses it
				cursorPos = new Vector2(Input.GetAxis("Gamepad Left Horizontal"), Input.GetAxis("Gamepad Left Vertical")).normalized;

				if(cursorPos != Vector2.zero)
				{
					radialGuideArrow.gameObject.SetActive(true);

					cursorsRotation = Vector2.Angle(Vector2.up, cursorPos);

					if(cursorPos.x < 0)
						cursorsRotation = 360 - cursorsRotation;

					radialGuideArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -cursorsRotation));
					CalculateZone();
				}
				else if(selectedOption)
				{
					selectedOption.myImage.color = Color.white;
					selectedOption = null;
					centralText.text = "";
				}
				else
					radialGuideArrow.gameObject.SetActive(false);
				
			}
			//if using keyboard
			else
			{
				if(Input.GetKey(KeyCode.LeftArrow))
					keyboardLedRotation -= 6;
				if(Input.GetKey(KeyCode.RightArrow))
					keyboardLedRotation += 6;

				if(keyboardLedRotation < 0)
					keyboardLedRotation += 360;
				keyboardLedRotation = keyboardLedRotation % 360;
				cursorsRotation = keyboardLedRotation;

				radialGuideArrow.SetActive(true);
				radialGuideArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -keyboardLedRotation));
				CalculateZone();
			}

			//for SELECTING
			if(selectedOption && (Input.GetButtonDown("FirePrimary")))
			{
				currentRadialScreen = selectedOption.myRadialScreen;
				screenProgression.Add(currentRadialScreen);

				selectedOption.RunMySelection();
				ClearRadialMenu();
				if(selectedOption.containsFinalCommand)
					DeactivateRadialMenu();
				else
					PopulateRadialMenuOptions(currentRadialScreen);
			}
		}
		//outside the above loop necessary so you can go back from a blank screen (like no wingmen available)
		if(radialMenuShown)
		{
			//for GO BACK
			if(Input.GetButtonDown("Dodge"))
			{
				if(currentRadialScreen == RadialScreens.OpenAChannel)
				{
					DeactivateRadialMenu();
				}
				else
				{
					screenProgression.RemoveAt(screenProgression.Count - 1); //remove the last screen
					currentRadialScreen = screenProgression[screenProgression.Count - 1]; //move to the new last screen on the list (shoudl be previous)
					ClearRadialMenu();
					PopulateRadialMenuOptions(currentRadialScreen);
				}
			}
		}
	}//end of UPDATE


	void PopulateRadialMenuOptions(RadialScreens currentScreen)
	{
		int radialOptions = 0;

		if(currentRadialScreen == RadialScreens.OpenAChannel)
		{
			radialOptions = 2; //TODO: Add channel for nearby ships
			headerText.text = "Open a Channel to..";
		}
		else if(currentRadialScreen == RadialScreens.Squadron)
		{
			if(PlayerAILogic.instance.squadLeaderScript.activeWingmen.Count == 0)
			{
				headerText.text = "Channel Empty";
				centralText.text = "You have no active wingmen";
				radioHasOptions = false;
				return;
			}
			else
			{
				headerText.text = "..Select Wingmen..";
				radialOptions = PlayerAILogic.instance.squadLeaderScript.activeWingmen.Count;
				if(radialOptions > 1)
					radialOptions += 1; //for an 'all' option
			}
		}
		else if(currentRadialScreen == RadialScreens.Support)
		{
			//TODO: Detect how many options we have
			radialOptions = 1;
		}
		else if(currentScreen == RadialScreens.FirstWingman || currentScreen == RadialScreens.SecondWingman
			|| currentScreen == RadialScreens.AllWingmen)
		{
			radialOptions = Orders.Length;
			if(currentScreen == RadialScreens.AllWingmen)
				headerText.text = "..to All Wingmen..";
			else
				headerText.text = "..to " + selectedWingmen[0].name + "..";
		}

		radioHasOptions = true;
		activeRadialOptions.Clear();
		
		float degreesEach = 360f/radialOptions;
		float rotation = 0;

		//throw up the options
		for(int i = 0; i < radialOptions; i++)
		{
			GameObject newOption = Instantiate(radialOptionPrefab) as GameObject;
			newOption.transform.SetParent(radialMenuCentralPanel);
			newOption.transform.localPosition = Vector3.zero;
			newOption.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 360 - rotation));
			newOption.transform.localScale = Vector3.one;
			RadialOption newScript = newOption.GetComponent<RadialOption>();

			newScript.enabled = true;
			activeRadialOptions.Add(newScript);

			newScript.GetRealRotation();
			newScript.maxRotation = (newScript.realRotation + degreesEach/2) % 360;
			newScript.minRotation = (newScript.realRotation - degreesEach/2) % 360;
			if(newScript.minRotation < 0)
				newScript.minRotation += 360;

			rotation += degreesEach;
		}
		rotation = degreesEach/2;

		AssignCommandsToEachOption();

		//throw up dividing bars
		if(radialOptions > 1)
		{
			for(int i = 0; i < radialOptions; i++)
			{
				GameObject newDivider = Instantiate(radialDivideBarPrefab) as GameObject;
				newDivider.transform.SetParent(radialMenuCentralPanel);
				newDivider.transform.localPosition = Vector3.zero;
				newDivider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
				newDivider.transform.localScale = Vector3.one;

				rotation += degreesEach;
			}
		}

		//make the guide arrow
		radialGuideArrow = Instantiate(radialGuideArrowPrefab) as GameObject;
		radialGuideArrow.transform.SetParent(radialMenuCentralPanel);
		radialGuideArrow.transform.localPosition = Vector3.zero;
		radialGuideArrow.transform.localScale = Vector3.one;

		keyboardLedRotation = 0;
		radialGuideArrowPrefab.SetActive(true);

		if(shouldToggleEscMenu)
			ClickToPlay.instance.escCanGiveQuitMenu = false;

	}//end of PopulateRadialMenuOptions()


	void ClearRadialMenu()
	{
		//TODO: Stop deleting and start re-using
		while(radialMenuCentralPanel.childCount > 0)
		{
			DestroyImmediate(radialMenuCentralPanel.GetChild(0).gameObject);
		}
	}

	public void DeactivateRadialMenu ()
	{
		Tools.instance.MoveCanvasToRear (Tools.instance.blackoutCanvas);
		Tools.instance.MoveCanvasToRear (radialMenuCanvas);
		Tools.instance.blackoutPanel.color = Color.clear;
		AudioMasterScript.instance.masterMixer.SetFloat ("Master vol", 0f);
		Tools.instance.AlterTimeScale (1f);
		bool[] bools = PlayerAILogic.instance.previousPlayerControlBools;
		PlayerAILogic.instance.TogglePlayerControl (bools[0], bools[1], bools[2], bools[3]);
		CameraTactical.instance.canAccessTacticalMap = true;
		headerText.enabled = false;
		centralText.enabled = false;
		ClearRadialMenu ();
		if(shouldToggleEscMenu)
			ClickToPlay.instance.escCanGiveQuitMenu = true;

		radialMenuShown = false;
	}

	void CalculateZone()
	{
		//set the old option white. New option might be the same or different, and we'll highlight it when found below
		if(selectedOption)
			selectedOption.myImage.color = Color.white;

		//which zone is the cursor in? go through all zones to check if current cursorRotation is between min and max
		for(int i = 0; i < activeRadialOptions.Count; i++)
		{
			//if it's the first one (12 o'clock, straddling 360), cursor rotation won't be greater than the minRotation, so it's a special case
			if(i == 0 && !Mathf.Approximately(cursorsRotation, 180f))
			{
				if(cursorsRotation > activeRadialOptions[i].minRotation || cursorsRotation <= activeRadialOptions[i].maxRotation)
				{
					selectedOption = activeRadialOptions[i];
				}
			}
			else if(cursorsRotation > activeRadialOptions[i].minRotation && cursorsRotation <= activeRadialOptions[i].maxRotation)
			{
				selectedOption = activeRadialOptions[i];
			}
		}

		if(selectedOption != null)
		{
			if(selectedOption.myImage != null)
				selectedOption.myImage.color = Color.red;
			
			centralText.text = selectedOption.displayText;
		}
	}


	IEnumerator DPadInputWait()
	{
		takeDPadInput = false;
		yield return new WaitForSecondsRealtime(0.4f);
		takeDPadInput = true;
	}

	void AssignCommandsToEachOption()
	{
		if(currentRadialScreen == RadialScreens.OpenAChannel)
		{
			activeRadialOptions[0].displayText = "Squadron";
			activeRadialOptions[0].myRadialScreen = RadialScreens.Squadron;
			activeRadialOptions[1].displayText = "Support";
			activeRadialOptions[1].myRadialScreen = RadialScreens.Support;
		}
		else if(currentRadialScreen == RadialScreens.Squadron)
		{
			if(activeRadialOptions.Count == 1) //if there's only the one wingman
			{
				activeRadialOptions[0].displayText = PlayerAILogic.instance.squadLeaderScript.activeWingmen[0].name;
				activeRadialOptions[0].myRadialScreen = RadialScreens.FirstWingman;
				return;
			}

			//otherwise
			for(int i = 0; i < activeRadialOptions.Count; i++)  
			{
				if(i == 0)
				{
					activeRadialOptions[i].displayText = "All Squad Members";
					activeRadialOptions[i].myRadialScreen = RadialScreens.AllWingmen;
				}
				else if(i == 1)
				{
					activeRadialOptions[i].displayText = PlayerAILogic.instance.squadLeaderScript.activeWingmen[1].name;
					activeRadialOptions[i].myRadialScreen = RadialScreens.SecondWingman;
				}
				else if(i == 2)
				{
					activeRadialOptions[i].displayText = PlayerAILogic.instance.squadLeaderScript.activeWingmen[0].name;
					activeRadialOptions[i].myRadialScreen = RadialScreens.FirstWingman;
				}
			}
			return;
		}
		else if(currentRadialScreen == RadialScreens.FirstWingman || currentRadialScreen == RadialScreens.SecondWingman
			|| currentRadialScreen == RadialScreens.AllWingmen)
		{
			for(int i = 0; i < activeRadialOptions.Count; i++)  
			{
				activeRadialOptions[i].displayText = Orders[i];
				activeRadialOptions[i].containsFinalCommand = true;
			}
		}
		else if(currentRadialScreen == RadialScreens.Support)
		{
			activeRadialOptions[0].displayText = "Extraction";
			activeRadialOptions[0].containsFinalCommand = true;
		}
	}

}


using AssemblyCSharp;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterPool : MonoBehaviour {

	public static string encryption = ".es?encrypt=true&password=asswordp"; //for EasySave2. If changing, also change '.es' in PopulateExportPoolList()

	public static CharacterPool instance;

	//NameGenerator ng;
	public Names namesSO;

	/*[HideInInspector] */ public CharacterPoolEntry selectedCharacter;
	[HideInInspector] public CharacterPoolGroupEntry selectedCharacterGroup;
	public GameObject characterPoolPanel;
	public GameObject characterCreationPanel;
	public ButtonSelectAuto backOutOfCharacterPoolScreenButton;
	public ButtonSelectAuto backOutOfCharacterGroupImportScreenButton;
	public GameObject characterBioEditPanel;
	public GameObject poolImportExportEditPanel;
	public GameObject poolExportConfirmationPanel;
	public GameObject poolDeleteConfirmationPanel;
	public GameObject addToGroupConfirmationPanel;
	public GameObject characterRemoveConfirmationPanel;
	public GameObject cpcCreateNewPoolButton;
	public GameObject cpcImportEntireCollectionButton;
	public Text cpcSubHeaderText;

	public enum ImportOrExport {Import, Export};
	[HideInInspector] public ImportOrExport importOrExport;
	public enum CurrentTask {EditingCharacter, ImportExport};
	[HideInInspector] public CurrentTask currentTask;

	public GameObject characterNameEditPanel;
	public Text nameHeaderText;

	public Text characterEditScreenHeaderText;

	public Text firstNameEntryText;
	public Text lastNameEntryText;
	public Text callsignEntryText;
	public Text genderEntryText;

	public CharacterPoolEntry[] allCharacters;
	public List<CharacterPoolEntry> selectedCharacters;

	public Button deselectAllButton;
	public Button deleteSelectedButton;
	public Button exportSelectedButton;
	public Dropdown charPoolUsageDropdown;

	public GameObject poolEntryPrefab;
	public GameObject poolGroupEntryPrefab;
	public GameObject poolCharacterImportEntryPrefab;
	public Transform parentForNewEntries;
	public Transform parentForNewGroupEntries;

	public string[] saveDataOutput; //for debugging in Inspector
	public string allIDs;
	public string [] allIDsArray;

	[Header("Numbers on Buttons for Selecting Appearance")]
	public Text selectedGenderText;
	public Text selectedBodyText;
	public Text selectedSkinColourText;
	public Text selectedEyesText;
	public Text selectedEyesPropText;
	public Text selectedNoseText;
	public Text selectedFacialHairText;
	public Text selectedHairText;
	public Text selectedHairColourText;
	public Text selectedFacialFeatureText;
	public Text selectedHelmetText;
	public Text selectedSpacesuitColourText;

	Character avatar;
	public GameObject avatarOutputForCharacterCreationScreen;

	public string[] seedStringArray;

	[TextArea()]
	public string[] testBox;

	string currentBio = "";


	IEnumerator PlayAudio(int howMany)
	{
		int i = 0;
		while(i < howMany)
		{
			GetComponent<AudioSource>().Play();
			i++;
			yield return new WaitForSeconds(0.2f);
		}
	}
	void CheckAvatarOutput()
	{
		if(avatar && avatar.avatarOutput != null)
		{
			if(GetComponent<AudioSource>().enabled)
				StartCoroutine(PlayAudio(2));
		}
		else
		{
			if(GetComponent<AudioSource>().enabled)
			{
				StartCoroutine("PlayAudio", 6);
				Subtitles.instance.PostHint(new string[] {avatar.avatarOutput.name});
			}
		}

	}


	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		}
		else
		{
			Debug.LogError("There were two Character Pools. Deleting one");
			DestroyImmediate(this.gameObject);
		}

		SetUpCharacterPoolUsageDropdown();

		avatar = FindObjectOfType<Character>();

		CheckAvatarOutput();

		DestroyChildEntries ();
	}
		

	public void ActivateCharacterPoolPanel()
	{
		CloseAllCharacterRelatedWindows();
		characterPoolPanel.SetActive(true);
		backOutOfCharacterPoolScreenButton.enabled = true;
		PopulateActiveCharacterPoolList();
	}


	void PopulateActiveCharacterPoolList ()
	{
		//this gets rid of the old character entries on the list as it populates new ones based on the save file each time. Prevents duplicates
		DestroyChildEntries ();

		//Load All Characters. Get from a list of saved, active characters. Create a list
		if (ES2.Exists ("allCharacterIDs" + encryption)) 
		{
			allIDs = ES2.Load<string> ("allCharacterIDs" + encryption);
		}
		else 
		{
			Debug.Log ("allCharacterIDs file did NOT exist. NO characters to load.");
		}

		//to see in Inspector (for debugging)
		allIDsArray = allIDs.Split (new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);

		//For each in the list, go through and..
		for (int i = 0; i < allIDsArray.Length; i++) 
		{
			LoadCharacterToPoolList (allIDsArray [i]);
		}

		allCharacters = FindObjectsOfType<CharacterPoolEntry>();

		UpdateSelection();
	}

	public void PopulateCharactersInImportList(CharacterPoolGroupEntry cpge) //these are characters on a potential import list, not the Active List
	{
		//this gets rid of the old character entries on the list as it populates new ones based on the save file each time. Prevents duplicates
		DestroyChildEntries ();

		//Load a representative list of character names from the saved data of each line in the Group

		for(int i = 1; i < cpge.savedData.Length; i++) //skip the first one, which is just the name
		{
			allIDs = cpge.savedData[i];

			//next line is just for viewing in Inspector
			allIDsArray = allIDs.Split (new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);

			GameObject newImportPoolEntry = Instantiate(poolCharacterImportEntryPrefab) as GameObject;
			CharacterPoolEntry characterScript = newImportPoolEntry.GetComponent<CharacterPoolEntry>();
			newImportPoolEntry.transform.SetParent(parentForNewGroupEntries);

			GiveCharacterPoolEntryScriptItsInfo(allIDs, characterScript);

			newImportPoolEntry.GetComponentInChildren<Text>().text = 
				characterScript.firstName + " \""+ characterScript.callsign + "\" " + characterScript.lastName;
		}
	}


	void PopulateImportOrExportPoolList()
	{
		//this gets rid of the old  entries on the list as it populates new ones based on the save file each time. Prevents duplicates
		DestroyChildEntries ();

		if(importOrExport == ImportOrExport.Import)
		{
			cpcCreateNewPoolButton.SetActive(false);
			cpcSubHeaderText.text = "Import a Character from an existing Collection";
		}

		if(System.IO.Directory.Exists(Application.persistentDataPath + "/CharacterPool"))
		{
			foreach(string file in System.IO.Directory.GetFiles(Application.persistentDataPath + "/CharacterPool"))
			{
				string[] info = file.Split(new String[]{"\\", ".es"}, StringSplitOptions.None);
				testBox = info;

				//Load All Character Lists.
				if (ES2.Exists ("CharacterPool/" + info[1] + encryption)) 
				{
					string[] groupInfo =
						ES2.Load<string> ("CharacterPool/" + info[1] + encryption).Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);
					testBox = groupInfo;

					GameObject newEntry = Instantiate(poolGroupEntryPrefab) as GameObject;
					newEntry.transform.parent = parentForNewGroupEntries;
					CharacterPoolGroupEntry newEntryScript = newEntry.GetComponent<CharacterPoolGroupEntry>();
					newEntryScript.savedData = groupInfo;
					newEntryScript.collectionName = groupInfo[0];
					newEntryScript.nameText.text = groupInfo[0];
				}
				else 
				{
					Debug.Log ("File DOESN'T exist. " + file);
				}
			}
		}
	}


	public void UpdateSelection()
	{
		selectedCharacters.Clear();

		print("AllCharacters Length: " + allCharacters.Length);
		for(int i = 0; i < allCharacters.Length; i++)
		{
			if(allCharacters[i].GetComponentInChildren<Toggle>().isOn)
				selectedCharacters.Add(allCharacters[i]);
		}

		if(selectedCharacters.Count != 0)
		{
			deselectAllButton.interactable = true;
			deleteSelectedButton.interactable = true;
			exportSelectedButton.interactable = true;
		}
		else
		{
			deselectAllButton.interactable = false;
			deleteSelectedButton.interactable = false;
			exportSelectedButton.interactable = false;
		}
	}


	public void SelectAllCharacters(bool trueOrFalse)
	{
		selectedCharacters.Clear();

		for(int i = 0; i < allCharacters.Length; i++)
		{
			allCharacters[i].GetComponentInChildren<Toggle>().isOn = trueOrFalse;

			if(trueOrFalse)
				selectedCharacters.Add(allCharacters[i]);
		}

		UpdateSelection();
	}

		
	public void CreateNewCharacter()
	{
		GameObject go = new GameObject("tempGO");
		selectedCharacter = go.AddComponent<CharacterPoolEntry>();

		avatar.GenerateRandomNewAppearance();
		if(avatar.gender == Character.Gender.Female)
			selectedFacialHairText.transform.parent.gameObject.SetActive(false);
		else
			selectedFacialHairText.transform.parent.gameObject.SetActive(true);		

		if(selectedCharacter == null)
			Debug.LogError("Feck");

		selectedCharacter.firstName = 
			avatar.gender == Character.Gender.Male? 
			namesSO.maleNames[UnityEngine.Random.Range(0, namesSO.maleNames.Count)] : 
			namesSO.femaleNames[UnityEngine.Random.Range(0, namesSO.femaleNames.Count)];
		selectedCharacter.lastName = namesSO.lastNames[UnityEngine.Random.Range(0, namesSO.lastNames.Count)];
		selectedCharacter.callsign = namesSO.callsigns[UnityEngine.Random.Range(0, namesSO.callsigns.Count)];
		selectedCharacter.appearanceSeed = avatar.appearanceSeed;
		currentBio = "";

		ActivateCharacterEditScreen(selectedCharacter, false);

		Destroy(go);
	}


	public void ActivateCharacterEditScreen(CharacterPoolEntry selected, bool thisIsASavedCharacter)
	{
		currentTask = CurrentTask.EditingCharacter;

		selectedCharacter = selected;
		SetCorrectNumbersOnEditButtons();

		avatar.avatarOutput.SetActive(true);

		//for viewing in Inspector
		avatar.appearanceSeed = selectedCharacter.appearanceSeed;

		char[] splitBy = new char[]{','};
		string[] splitAppearanceSeed = avatar.appearanceSeed.Split(splitBy, System.StringSplitOptions.RemoveEmptyEntries);

		if(thisIsASavedCharacter)
			avatar.GenerateAppearanceBySeed(splitAppearanceSeed);

		characterPoolPanel.SetActive(false);
		backOutOfCharacterPoolScreenButton.enabled = false;
		characterCreationPanel.SetActive(true);

		characterEditScreenHeaderText.text = selectedCharacter.firstName + " \"" + selectedCharacter.callsign + "\" " + selectedCharacter.lastName;
	}

	void SetCorrectNumbersOnEditButtons()
	{
		firstNameEntryText.text = "First Name: "+ selectedCharacter.firstName;
		lastNameEntryText.text = "Last Name: " + selectedCharacter.lastName;
		callsignEntryText.text = "Callsign: " + selectedCharacter.callsign;
		genderEntryText.text = avatar.gender == Character.Gender.Male? "0" : "1";

		//APPEARANCE_SEED:
		//FOR SEED (string): ORDER IS: 
		//Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, SpacesuitColour

		seedStringArray = selectedCharacter.appearanceSeed.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);	

		selectedGenderText.text = seedStringArray[0].ToString();
		selectedBodyText.text = seedStringArray[1].ToString();
		selectedSkinColourText.text = seedStringArray[2].ToString();
		selectedNoseText.text = seedStringArray[3].ToString();
		selectedEyesText.text = seedStringArray[4].ToString();
		selectedHairText.text = seedStringArray[5].ToString();
		selectedFacialHairText.text = seedStringArray[6].ToString();
		selectedHairColourText.text = seedStringArray[7].ToString();
		selectedEyesPropText.text = seedStringArray[8].ToString();
		selectedFacialFeatureText.text = seedStringArray[9].ToString();
		selectedHelmetText.text = seedStringArray[10].ToString();
		selectedSpacesuitColourText.text = seedStringArray[11].ToString();
	}

	void SpaceSuitShouldBeOn(bool shouldBeOn)
	{
		if(shouldBeOn && !avatar.inSpace)
			ToggleSpaceSuit();
		else if(!shouldBeOn && avatar.inSpace)
			ToggleSpaceSuit();
	}

	public void ToggleSpaceSuit()
	{
		avatar.inSpace = !avatar.inSpace;
		avatar.spaceSuit.enabled = avatar.inSpace;
		avatar.helmet.enabled = avatar.inSpace;
		avatar.hair.enabled = !avatar.inSpace;
		avatar.clothes.enabled = !avatar.inSpace;
	}

	#region Name & Bio Editing
	public void ActivateBioEditPanel(InputField input)
	{
		characterBioEditPanel.SetActive(true);
		//replace placeholder text with existing bio if any
		if(selectedCharacter != null && selectedCharacter.characterBio != "")
		{
			input.text = selectedCharacter.characterBio;
			currentBio = selectedCharacter.characterBio;
		}
			
		selectedCharacter.startingBioText = input.text;
		characterBioEditPanel.GetComponentInChildren<InputField>().ActivateInputField();
	}


	public void ActivateNameEditPanel(string whichName)
	{
		nameHeaderText.text = whichName;
		characterNameEditPanel.SetActive(true);
		characterNameEditPanel.GetComponentInChildren<InputField>().text = "";
		characterNameEditPanel.GetComponentInChildren<InputField>().ActivateInputField();
	}

	public void ActivateImportPanel()
	{
		CloseAllCharacterRelatedWindows();
		selectedCharacters.Clear();
		poolImportExportEditPanel.SetActive(true);
		importOrExport = ImportOrExport.Import;
		PopulateImportOrExportPoolList();
	}

	public void ActivateExportEditPanel()
	{
		CloseAllCharacterRelatedWindows();
		importOrExport = ImportOrExport.Export;
		poolImportExportEditPanel.SetActive(true);
		PopulateImportOrExportPoolList();
	}

	public void ActivateExportConfirmationPanel()
	{
		poolExportConfirmationPanel.gameObject.SetActive(true);
		poolExportConfirmationPanel.GetComponentInChildren<InputField>().text = "";
		poolExportConfirmationPanel.GetComponentInChildren<InputField>().ActivateInputField();
	}


	public void SetNewName(InputField input)
	{
		switch(nameHeaderText.text)
		{
		default: Debug.LogError("Name error! " + nameHeaderText.text);
			break;

		case "First Name":
			selectedCharacter.firstName = input.text;
			firstNameEntryText.text = nameHeaderText.text + ": " + selectedCharacter.firstName;
			break;

		case "Last Name":
			selectedCharacter.lastName = input.text;
			lastNameEntryText.text = nameHeaderText.text + ": " + selectedCharacter.lastName;
			break;

		case "Callsign":
			selectedCharacter.callsign = input.text;
			input.text = "\"" + input.text + "\"";
			callsignEntryText.text = nameHeaderText.text + ": " + selectedCharacter.callsign;
			break;
		}
			
		characterNameEditPanel.SetActive(false);
		characterEditScreenHeaderText.text = selectedCharacter.firstName + " \"" + selectedCharacter.callsign + "\" " + selectedCharacter.lastName;
	}


	public void SetNewBio(InputField input)
	{
		selectedCharacter.characterBio = input.text;
		currentBio = input.text;
		characterBioEditPanel.SetActive(false);
	}

	public void CancelNewBio(InputField input)
	{
		if(selectedCharacter != null)
		{
			input.text = selectedCharacter.startingBioText;
		}
		else 
			input.text = currentBio;
		
		characterBioEditPanel.SetActive(false);
	}


	public void CloseAllCharacterRelatedWindows()
	{
		characterPoolPanel.SetActive(false);
		backOutOfCharacterPoolScreenButton.enabled = false;
		characterCreationPanel.SetActive(false);
		characterNameEditPanel.SetActive(false);
		characterBioEditPanel.SetActive(false);
		poolImportExportEditPanel.SetActive(false);
		poolExportConfirmationPanel.SetActive(false);
		poolDeleteConfirmationPanel.SetActive(false);
		addToGroupConfirmationPanel.SetActive(false);
		characterRemoveConfirmationPanel.SetActive(false);
		cpcCreateNewPoolButton.SetActive(true);
		cpcImportEntireCollectionButton.SetActive(false);
		cpcSubHeaderText.text = "Character Collections";

		try{
			avatar.avatarOutput.SetActive(false);
			selectedCharacter = null;
			selectedCharacterGroup = null;		
		}
		catch{
			CheckAvatarOutput();
		}
  	}
	#endregion

	string CollectInfo()
	{
		string stringToReturn = 
		"ID:" + selectedCharacter.characterID + "FN:" + selectedCharacter.firstName +
		"LN:" + selectedCharacter.lastName + "CS:" + selectedCharacter.callsign +
		"BIO:" + selectedCharacter.characterBio +
		"APP:" + selectedCharacter.appearanceSeed;

		return stringToReturn;
	}

	public void SaveCharacter(bool returnToMain)
	{
		//check if character exists already (if they have an ID) and generate an ID if it's blank

		if(selectedCharacter.characterID == "")
		{
			selectedCharacter.characterID = GenerateCharacterID();
			Debug.Log("Generating new CharacterID: " + selectedCharacter.characterID);
		}

		if(currentTask == CurrentTask.EditingCharacter) //if we're editing a new character
		{
			//converts the char array from the on-screen avatar into a single string

			selectedCharacter.appearanceSeed = null;
			foreach(string seedFragment in seedStringArray)
			{
				selectedCharacter.appearanceSeed += seedFragment + ','; 
			}
		}
		else if(currentTask == CurrentTask.ImportExport)
		{
			//don't edit the appearance seed.
		}
		else Debug.LogError("Unforseen Circumstance");

		//collect info
		string saveData = CollectInfo();

		//for debugging in Inspector
		string[] parameters = new string[]{"ID:","FN:","LN:","CS:", "BIO:", "APP:"};
		saveDataOutput = saveData.Split(parameters, System.StringSplitOptions.None);
			
		//save it with all character info
		ES2.Save(saveData, selectedCharacter.characterID + encryption);

		//save a new list of all character IDs

		allIDs = selectedCharacter.characterID + ',';

		if(ES2.Exists("allCharacterIDs" + encryption))
		{
			string oldSaves = ES2.Load<string>("allCharacterIDs" + encryption);

			if(oldSaves.Contains(selectedCharacter.characterID))
			{
				allIDs = oldSaves;
				Debug.Log("Current characterID was already in saves");
			}
			else
			{
				allIDs += oldSaves;
				Debug.Log("Current characterID was NEW. Adding to saves.");
			}
		}
		else
		{
			Debug.Log("allCharacterIDs file did NOT exist");
		}

		ES2.Save(allIDs, "allCharacterIDs" + encryption);

		//to see in Inspector (for debugging)
		allIDsArray = allIDs.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);

		if(returnToMain)
			ActivateCharacterPoolPanel();

	}	//end of SaveCharacter()


	public void ImportEntirePool()
	{
		allCharacters = FindObjectsOfType<CharacterPoolEntry>();

		for(int i = 0; i < allCharacters.Length; i++)
		{
			allCharacters[i].ImportThisCharacter(false);
		}

		ActivateCharacterPoolPanel();
	}

	public void ExportSelection(bool isNew)
	{
		string exportSelectionName = "";
		string exportField = "";

		if(isNew)
		{
			exportSelectionName = poolExportConfirmationPanel.transform.FindChild("InputField/Text").GetComponent<Text>().text;
			exportField = exportSelectionName + "\n";
		}
		else
		{
			exportSelectionName = selectedCharacterGroup.collectionName;
			exportField = exportSelectionName + "\n";
			for(int i = 1; i < selectedCharacterGroup.savedData.Length; i++)
			{
				exportField += selectedCharacterGroup.savedData[i] + "\n";
			}
		}

		foreach(CharacterPoolEntry character in selectedCharacters)
		{
			selectedCharacter = character;
			string exportCharacterString = CollectInfo();

			exportField += exportCharacterString + "\n";
		}

		ES2.Save(exportField, "CharacterPool/" + exportSelectionName + encryption);

		selectedCharacters.Clear();
		ActivateCharacterPoolPanel();
	}


	public string GenerateCharacterID()
	{
		string toReturn = System.DateTime.Now.ToBinary().ToString() + (allCharacters.Length + 1);
		char[] allChars = toReturn.ToCharArray();
		toReturn = "";

		for(int i = 1; i < allChars.Length; i++)
			toReturn += allChars[i];

		return toReturn;
	}


	void LoadCharacterToPoolList(string charID)
	{
		if(charID == "")
		{
			Debug.Log("Character ID was empty. Returning.");
			return;
		}
		else if(!ES2.Exists(charID + encryption))
		{
			Debug.Log("No character info found to Load. Returning.");
			return;
		}

		GameObject newPoolEntry = Instantiate(poolEntryPrefab) as GameObject;
		CharacterPoolEntry characterScript = newPoolEntry.GetComponent<CharacterPoolEntry>();
		newPoolEntry.transform.SetParent(parentForNewEntries);

		string characterInfo = ES2.Load<string>(charID + encryption);
		GiveCharacterPoolEntryScriptItsInfo(characterInfo, characterScript);

		newPoolEntry.GetComponentInChildren<Text>().text = 
			characterScript.firstName + " \""+ characterScript.callsign + "\" " + characterScript.lastName;
	}

	void GiveCharacterPoolEntryScriptItsInfo(string characterInfo, CharacterPoolEntry cpeScript)
	{
		string[] parameters = new string[]{"ID:","FN:","LN:","CS:", "BIO:", "APP:"};
		cpeScript.savedData = characterInfo.Split(parameters, System.StringSplitOptions.None);

		cpeScript.characterID = cpeScript.savedData[1];
		cpeScript.firstName = cpeScript.savedData[2];
		cpeScript.lastName = cpeScript.savedData[3];
		cpeScript.callsign = cpeScript.savedData[4];
		cpeScript.characterBio = cpeScript.savedData[5];
		cpeScript.appearanceSeed = cpeScript.savedData[6];
	}


	public void DeleteSelected()
	{
		string removedIDs = "";

		for(int i = 0; i < selectedCharacters.Count; i++)
		{
			if(ES2.Exists(selectedCharacters[i].characterID + encryption))
			{
				ES2.Delete(selectedCharacters[i].characterID + encryption);
				print("Removing");
				removedIDs += selectedCharacters[i].characterID +',';
			}
			else
				print("Couldn't find");
		}

		//SAVE NEW LIST OF ALL IDs
		//first, get the original list

		if (ES2.Exists ("allCharacterIDs" + encryption)) 
		{
			allIDs = ES2.Load<string> ("allCharacterIDs" + encryption);
		}
		else 
		{
			Debug.Log ("allCharacterIDs file did NOT exist. NO characters to load.");
			allIDs = "";
		}

		//to see in Inspector (for debugging)
		allIDsArray = allIDs.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);

		//next make a new string to save, without any of the IDs we just deleted
		string newAllIDs = "";

		for(int i = 0; i < allIDsArray.Length; i++)
		{
			if(!removedIDs.Contains(allIDsArray[i]))
			{
				newAllIDs += allIDsArray[i] + ',';
			}
		}
		ES2.Save(newAllIDs, "allCharacterIDs" + encryption);

		//repopulate the list so we can see who's left
		PopulateActiveCharacterPoolList();

	}//end of DeleteSelected()


	public void DeleteSelectedGroup()
	{
		if(ES2.Exists("CharacterPool/" + selectedCharacterGroup.nameText.text + encryption))
		{
			ES2.Delete("CharacterPool/" + selectedCharacterGroup.nameText.text + encryption);
		}
		else
		{
			Debug.LogError("Couldn't find selected Character Group.");
		}

		poolDeleteConfirmationPanel.SetActive(false);

		ActivateCharacterPoolPanel();
	}


	public void OpenCharacterPoolFolder()
	{
		try{
		System.Diagnostics.Process.Start(Application.persistentDataPath + "/CharacterPool");
		}
		catch{
			System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/CharacterPool");
			System.Diagnostics.Process.Start(Application.persistentDataPath + "/CharacterPool");
		}
	}


	void DestroyChildEntries ()
	{
		while (parentForNewEntries.childCount > 0) 
		{
			DestroyImmediate (parentForNewEntries.GetChild (0).gameObject);
		}

		while (parentForNewGroupEntries.childCount > 0) 
		{
			DestroyImmediate (parentForNewGroupEntries.GetChild (0).gameObject);
		}
	}

	void SetUpCharacterPoolUsageDropdown()
	{
		string setting = PlayerPrefsManager.GetCharacterPoolUsageKey();
		if(setting == "Random & Character Pool")
			charPoolUsageDropdown.value = 0;
		else if(setting == "Character Pool Only")
			charPoolUsageDropdown.value = 1;
		else if(setting == "Random Only")
			charPoolUsageDropdown.value = 2;
		else
			Debug.LogError("Player Prefs Error");
	}
	public void SetCharacterPoolUsageSettings()
	{
		PlayerPrefsManager.SetCharacterPoolUsageKey(charPoolUsageDropdown.transform.FindChild("Label").GetComponent<Text>().text);
	}


	#region Avatar Characteristics

	//APPEARANCE_SEED:
	//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, SpacesuitColour

	public void NextGender() //changing this is touger as it requires changing possible eyes, hair, facial hair, etc
	{
		if(avatar.gender == Character.Gender.Male)
		{
			avatar.gender = Character.Gender.Female;
			avatar.myAppearance = avatar.femaleAppearances;
			selectedCharacter.firstName = namesSO.femaleNames[UnityEngine.Random.Range(0, namesSO.femaleNames.Count)];				
			selectedGenderText.text = "1";
			avatar.eyeLids.sprite = avatar.myAppearance.eyes[0];
			selectedEyesText.text = "0";
			avatar.facialHair.sprite = null;
			selectedFacialHairText.text = "0";
			selectedFacialHairText.transform.parent.gameObject.SetActive(false);
			avatar.hair.sprite = avatar.myAppearance.hair[0];
			selectedHairText.text = "0";
		}
		else 
		{
			avatar.gender = Character.Gender.Male;
			avatar.myAppearance = avatar.maleAppearances;
			selectedCharacter.firstName = namesSO.maleNames[UnityEngine.Random.Range(0, namesSO.maleNames.Count)];
			selectedGenderText.text = "0";
			avatar.eyeLids.sprite = avatar.myAppearance.eyes[0];
			selectedEyesText.text = "0";
			avatar.facialHair.sprite = avatar.myAppearance.facialHair[0];
			selectedFacialHairText.text = "0";
			selectedFacialHairText.transform.parent.gameObject.SetActive(true);
			avatar.hair.sprite = avatar.myAppearance.hair[0];
			avatar.body.sprite = avatar.myAppearance.baseBody[Int32.Parse(selectedBodyText.text)];
		}
		firstNameEntryText.text = "First Name: " + selectedCharacter.firstName;
		characterEditScreenHeaderText.text = selectedCharacter.firstName + " \"" + selectedCharacter.callsign + "\" " + selectedCharacter.lastName;
		avatar.eyeSet[0] = avatar.eyeLids.sprite;
		avatar.eyeSet[1] = avatar.eyeWhites.sprite;
		avatar.eyeSet[2] = avatar.eyeIrises.sprite;
		avatar.eyeSet[3] = avatar.eyesBlinking.sprite;
		seedStringArray[0] = selectedGenderText.text + ',';
	}

	public void NextBodyType(int i)
	{
		int arrayPosition = Int32.Parse(selectedBodyText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.baseBody.Length -1;
		else if(arrayPosition >= avatar.myAppearance.baseBody.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.body.sprite = avatar.myAppearance.baseBody[arrayPosition];

		selectedBodyText.text = arrayPosition.ToString();

		seedStringArray[1] = selectedBodyText.text + ',';
	}

	public void NextSkinColour(int i)
	{
		if(avatar.body == null)
		{
			StartCoroutine("PlayAudio", 4);
		}

		int arrayPosition = Int32.Parse(selectedSkinColourText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.skinTones.Length -1;
		else if(arrayPosition >= avatar.myAppearance.skinTones.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.body.color = avatar.myAppearance.skinTones[arrayPosition];
		avatar.AdjustFacialFeatureColour();
		selectedSkinColourText.text = arrayPosition.ToString();

		seedStringArray[2] = selectedSkinColourText.text + ',';
	}

	public void NextNose(int i)
	{
		int arrayPosition = Int32.Parse(selectedNoseText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.noses.Length -1;
		else if(arrayPosition >= avatar.myAppearance.noses.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.nose.sprite = avatar.myAppearance.noses[arrayPosition];
		selectedNoseText.text = arrayPosition.ToString();

		seedStringArray[3] = selectedNoseText.text + ',';
	}

	public void NextEyes(int i)
	{
		int arrayPosition = Int32.Parse(selectedEyesText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.eyes.Length -1;
		else if(arrayPosition >= avatar.myAppearance.eyes.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.eyeLids.sprite = avatar.myAppearance.eyes[arrayPosition];

		//then set
		selectedEyesText.text = arrayPosition.ToString();
		avatar.eyeSet[0] = avatar.eyeLids.sprite;
		avatar.eyeSet[1] = avatar.eyeWhites.sprite;
		avatar.eyeSet[2] = avatar.eyeIrises.sprite;
		avatar.eyeSet[3] = avatar.eyesBlinking.sprite;
		seedStringArray[4] = selectedEyesText.text + ',';
	}

	public void NextHair(int i)
	{
		SpaceSuitShouldBeOn(false);

		int arrayPosition = Int32.Parse(selectedHairText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.hair.Length -1;
		else if(arrayPosition >= avatar.myAppearance.hair.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.hair.sprite = avatar.myAppearance.hair[arrayPosition];


		//then set
		selectedHairText.text = arrayPosition.ToString();

		seedStringArray[5] = selectedHairText.text + ',';
	}

	public void NextFacialHair(int i)
	{
		SpaceSuitShouldBeOn(false);

		int arrayPosition = Int32.Parse(selectedFacialHairText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.facialHair.Length -1;
		else if(arrayPosition >= avatar.myAppearance.facialHair.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.facialHair.sprite = avatar.myAppearance.facialHair[arrayPosition];
		selectedFacialHairText.text = arrayPosition.ToString();

		seedStringArray[6] = selectedFacialHairText.text + ',';
	}

	public void NextHairColour(int i)
	{
		SpaceSuitShouldBeOn(false);

		int arrayPosition = Int32.Parse(selectedHairColourText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.hairColours.Length -1;
		else if(arrayPosition >= avatar.myAppearance.hairColours.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.facialHair.color = avatar.myAppearance.hairColours[arrayPosition];
		avatar.hair.color = avatar.myAppearance.hairColours[arrayPosition];
		avatar.eyebrows.color = avatar.myAppearance.hairColours[arrayPosition];
		selectedHairColourText.text = arrayPosition.ToString();

		seedStringArray[7] = selectedHairColourText.text + ',';
	}

	public void NextEyesProp(int i)
	{
		int arrayPosition = Int32.Parse(selectedEyesPropText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.eyesProp.Length;
		else if(arrayPosition > avatar.myAppearance.eyesProp.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		if(arrayPosition == 0)
			avatar.eyesProp.sprite = null;
		else
			avatar.eyesProp.sprite = avatar.myAppearance.eyesProp[arrayPosition-1];
		selectedEyesPropText.text = (arrayPosition).ToString();

		seedStringArray[8] = selectedEyesPropText.text + ',';
	}

	public void NextFacialFeature(int i)
	{
		int arrayPosition = Int32.Parse(selectedFacialFeatureText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.facialFeatures1.Length -1;
		else if(arrayPosition >= avatar.myAppearance.facialFeatures1.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.facialFeatures1.sprite = avatar.myAppearance.facialFeatures1[arrayPosition];
		avatar.AdjustFacialFeatureColour();
		selectedFacialFeatureText.text = arrayPosition.ToString();

		seedStringArray[9] = selectedFacialFeatureText.text + ',';
	}
	//TODO: same as above for Features2 (and rename above to '1')
	//TODO: two for scars also

	public void NextHelemt(int i)
	{
		SpaceSuitShouldBeOn(true);

		int arrayPosition = Int32.Parse(selectedHelmetText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.helmets.Length -1;
		else if(arrayPosition >= avatar.myAppearance.helmets.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.helmet.sprite = avatar.myAppearance.helmets[arrayPosition];
		selectedHelmetText.text = arrayPosition.ToString();

		seedStringArray[10] = selectedHelmetText.text + ',';
	}

	public void NextSpacesuitColour(int i)
	{
		SpaceSuitShouldBeOn(true);

		int arrayPosition = Int32.Parse(selectedSpacesuitColourText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.myAppearance.spaceSuitColours.Length -1;
		else if(arrayPosition >= avatar.myAppearance.spaceSuitColours.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.spaceSuit.color = avatar.myAppearance.spaceSuitColours[arrayPosition];
		avatar.helmet.color = avatar.myAppearance.spaceSuitColours[arrayPosition];
		selectedSpacesuitColourText.text = arrayPosition.ToString();

		seedStringArray[11] = selectedSpacesuitColourText.text + ',';
	}


	#endregion
}

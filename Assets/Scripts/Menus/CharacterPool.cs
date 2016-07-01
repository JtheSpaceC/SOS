using AssemblyCSharp;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CharacterPool : MonoBehaviour {

	string encryption = ".es?encrypt=true&password=asswordp"; //for EasySave2. If changing, also change '.es' in PopulateExportPoolList()

	public static CharacterPool instance;

	NameGenerator ng;

	[HideInInspector] public CharacterPoolEntry selectedCharacter;
	[HideInInspector] public CharacterPoolGroupEntry selectedCharacterGroup;
	public GameObject characterPoolPanel;
	public GameObject characterCreationPanel;
	public GameObject characterBioEditPanel;
	public GameObject poolImportExportEditPanel;
	public GameObject poolExportConfirmationPanel;
	public GameObject poolDeleteConfirmationPanel;
	public GameObject addToGroupConfirmationPanel;
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

	public char[] seedCharArray;

	[TextArea()]
	public string[] testBox;


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

		avatar = FindObjectOfType<Character>();
		ng = NameGenerator.Instance;

		DestroyChildEntries ();
	}
		

	public void ActivateCharacterPoolPanel()
	{
		CloseAllCharacterRelatedWindows();
		characterPoolPanel.SetActive(true);
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
		selectedCharacter = new CharacterPoolEntry();

		avatar.GenerateRandomNewAppearance();
		if(avatar.gender == Character.Gender.Female)
			selectedFacialHairText.transform.parent.gameObject.SetActive(false);
		else
			selectedFacialHairText.transform.parent.gameObject.SetActive(true);		

		selectedCharacter.firstName = ng.getRandomFirstName(avatar.gender.ToString().ToCharArray()[0]);
		selectedCharacter.lastName = ng.getRandomLastName();
		selectedCharacter.callsign = ng.getRandomCallsign();
		selectedCharacter.appearanceSeed = avatar.appearanceSeed;

		ActivateCharacterEditScreen(selectedCharacter, false);
	}


	public void ActivateCharacterEditScreen(CharacterPoolEntry selected, bool thisIsASavedCharacter)
	{
		currentTask = CurrentTask.EditingCharacter;

		selectedCharacter = selected;
		SetCorrectNumbersOnEditButtons();

		avatar.avatarOutput.SetActive(true);

		//for viewing in Inspector
		avatar.appearanceSeed = selectedCharacter.appearanceSeed;

		if(thisIsASavedCharacter)
			avatar.GenerateAppearanceBySeed(avatar.appearanceSeed.ToCharArray());

		characterPoolPanel.SetActive(false);
		characterCreationPanel.SetActive(true);

		characterEditScreenHeaderText.text = selectedCharacter.firstName + " \"" + selectedCharacter.callsign + "\" " + selectedCharacter.lastName;
	}

	void SetCorrectNumbersOnEditButtons()
	{
		firstNameEntryText.text = "First Name: "+ selectedCharacter.firstName;
		lastNameEntryText.text = "Last Name: " + selectedCharacter.lastName;
		callsignEntryText.text = "Callsign: " + selectedCharacter.callsign;
		genderEntryText.text = avatar.gender == Character.Gender.Male? "0" : "1";

		//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, SpacesuitColour
		seedCharArray = selectedCharacter.appearanceSeed.ToCharArray();

		selectedGenderText.text = seedCharArray[0].ToString();
		selectedBodyText.text = seedCharArray[1].ToString();
		selectedSkinColourText.text = seedCharArray[2].ToString();
		selectedNoseText.text = seedCharArray[3].ToString();
		selectedEyesText.text = seedCharArray[4].ToString();
		selectedHairText.text = seedCharArray[5].ToString();
		selectedFacialHairText.text = seedCharArray[6].ToString();
		selectedHairColourText.text = seedCharArray[7].ToString();
		selectedEyesPropText.text = seedCharArray[8].ToString();
		selectedFacialFeatureText.text = seedCharArray[9].ToString();
		selectedHelmetText.text = seedCharArray[10].ToString();
		selectedSpacesuitColourText.text = seedCharArray[11].ToString();
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
		characterBioEditPanel.SetActive(false);
	}

	public void CancelNewBio(InputField input)
	{
		if(selectedCharacter != null)
			input.text = selectedCharacter.startingBioText;
		characterBioEditPanel.SetActive(false);
	}


	public void CloseAllCharacterRelatedWindows()
	{
		characterPoolPanel.SetActive(false);
		characterCreationPanel.SetActive(false);
		characterNameEditPanel.SetActive(false);
		characterBioEditPanel.SetActive(false);
		poolImportExportEditPanel.SetActive(false);
		poolExportConfirmationPanel.SetActive(false);
		poolDeleteConfirmationPanel.SetActive(false);
		addToGroupConfirmationPanel.SetActive(false);
		cpcCreateNewPoolButton.SetActive(true);
		cpcImportEntireCollectionButton.SetActive(false);
		cpcSubHeaderText.text = "Character Collections";
		avatar.avatarOutput.SetActive(false);
		selectedCharacter = null;
		selectedCharacterGroup = null;
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
			selectedCharacter.appearanceSeed = new string(seedCharArray); 
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


	#region Avatar Characteristics

	//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, SpacesuitColour

	public void NextGender() //changing this is touger as it requires changing possible eyes, hair, facial hair, etc
	{
		if(avatar.gender == Character.Gender.Male)
		{
			avatar.gender = Character.Gender.Female;
			selectedGenderText.text = "1";
			avatar.eyes.sprite = avatar.appearances.eyesFemale[0];
			selectedEyesText.text = "0";
			avatar.facialHair.sprite = null;
			selectedFacialHairText.text = "0";
			selectedFacialHairText.transform.parent.gameObject.SetActive(false);
			avatar.hair.sprite = avatar.appearances.hairFemale[0];
			selectedHairText.text = "0";
		}
		else 
		{
			avatar.gender = Character.Gender.Male;
			selectedCharacter.firstName = ng.getRandomFirstName(avatar.gender.ToString().ToCharArray()[0]);
			selectedGenderText.text = "0";
			avatar.eyes.sprite = avatar.appearances.eyesMale[0];
			selectedEyesText.text = "0";
			avatar.facialHair.sprite = avatar.appearances.facialHair[0];
			selectedFacialHairText.text = "0";
			selectedFacialHairText.transform.parent.gameObject.SetActive(true);
			avatar.hair.sprite = avatar.appearances.hairMale[0];
			avatar.body.sprite = avatar.appearances.baseBody[Int32.Parse(selectedBodyText.text)];
		}
		selectedCharacter.firstName = ng.getRandomFirstName(avatar.gender.ToString().ToCharArray()[0]);
		firstNameEntryText.text = "First Name: " + selectedCharacter.firstName;
		characterEditScreenHeaderText.text = selectedCharacter.firstName + " \"" + selectedCharacter.callsign + "\" " + selectedCharacter.lastName;
		avatar.originalEyes = avatar.eyes.sprite;
		seedCharArray[0] = selectedGenderText.text[0];
	}

	public void NextBodyType(int i)
	{
		int arrayPosition = Int32.Parse(selectedBodyText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.baseBody.Length -1;
		else if(arrayPosition >= avatar.appearances.baseBody.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.body.sprite = avatar.appearances.baseBody[arrayPosition];

		selectedBodyText.text = arrayPosition.ToString();

		seedCharArray[1] = selectedBodyText.text[0];
	}

	public void NextSkinColour(int i)
	{
		int arrayPosition = Int32.Parse(selectedSkinColourText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.skinTones.Length -1;
		else if(arrayPosition >= avatar.appearances.skinTones.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.body.color = avatar.appearances.skinTones[arrayPosition];
		avatar.AdjustFacialFeatureColour();
		selectedSkinColourText.text = arrayPosition.ToString();

		seedCharArray[2] = selectedSkinColourText.text[0];
	}

	public void NextNose(int i)
	{
		int arrayPosition = Int32.Parse(selectedNoseText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.noses.Length -1;
		else if(arrayPosition >= avatar.appearances.noses.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.nose.sprite = avatar.appearances.noses[arrayPosition];
		selectedNoseText.text = arrayPosition.ToString();

		seedCharArray[3] = selectedNoseText.text[0];
	}

	public void NextEyes(int i)
	{
		int arrayPosition = Int32.Parse(selectedEyesText.text);
		arrayPosition += i;

		if(genderEntryText.text == "0")
		{
			if(arrayPosition < 0) //check if it's less than 0
				arrayPosition = avatar.appearances.eyesMale.Length -1;
			else if(arrayPosition >= avatar.appearances.eyesMale.Length) //check if it's greater than length
				arrayPosition = 0;

			//then set
			avatar.eyes.sprite = avatar.appearances.eyesMale[arrayPosition];
		}
		else if(genderEntryText.text == "1")
		{
			if(arrayPosition < 0) //check if it's less than 0
				arrayPosition = avatar.appearances.eyesFemale.Length -1;
			else if(arrayPosition >= avatar.appearances.eyesFemale.Length) //check if it's greater than length
				arrayPosition = 0;

			//then set
			avatar.eyes.sprite = avatar.appearances.eyesFemale[arrayPosition];
		}

		//then set
		selectedEyesText.text = arrayPosition.ToString();
		avatar.originalEyes = avatar.eyes.sprite;

		seedCharArray[4] = selectedEyesText.text[0];
	}

	public void NextHair(int i)
	{
		SpaceSuitShouldBeOn(false);

		int arrayPosition = Int32.Parse(selectedHairText.text);
		arrayPosition += i;

		if(genderEntryText.text == "0")
		{
			if(arrayPosition < 0) //check if it's less than 0
				arrayPosition = avatar.appearances.hairMale.Length -1;
			else if(arrayPosition >= avatar.appearances.hairMale.Length) //check if it's greater than length
				arrayPosition = 0;

			//then set
			avatar.hair.sprite = avatar.appearances.hairMale[arrayPosition];
		}
		else if(genderEntryText.text == "1")
		{
			if(arrayPosition < 0) //check if it's less than 0
				arrayPosition = avatar.appearances.hairFemale.Length -1;
			else if(arrayPosition >= avatar.appearances.hairFemale.Length) //check if it's greater than length
				arrayPosition = 0;

			//then set
			avatar.hair.sprite = avatar.appearances.hairFemale[arrayPosition];
		}

		//then set
		selectedHairText.text = arrayPosition.ToString();

		seedCharArray[5] = selectedHairText.text[0];
	}

	public void NextFacialHair(int i)
	{
		SpaceSuitShouldBeOn(false);

		int arrayPosition = Int32.Parse(selectedFacialHairText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.facialHair.Length -1;
		else if(arrayPosition >= avatar.appearances.facialHair.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.facialHair.sprite = avatar.appearances.facialHair[arrayPosition];
		selectedFacialHairText.text = arrayPosition.ToString();

		seedCharArray[6] = selectedFacialHairText.text[0];
	}

	public void NextHairColour(int i)
	{
		SpaceSuitShouldBeOn(false);

		int arrayPosition = Int32.Parse(selectedHairColourText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.hairColours.Length -1;
		else if(arrayPosition >= avatar.appearances.hairColours.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.facialHair.color = avatar.appearances.hairColours[arrayPosition];
		avatar.hair.color = avatar.appearances.hairColours[arrayPosition];
		selectedHairColourText.text = arrayPosition.ToString();

		seedCharArray[7] = selectedHairColourText.text[0];
	}

	public void NextEyesProp(int i)
	{
		int arrayPosition = Int32.Parse(selectedEyesPropText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.eyesProp.Length -1;
		else if(arrayPosition >= avatar.appearances.eyesProp.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.eyesProp.sprite = avatar.appearances.eyesProp[arrayPosition];
		selectedEyesPropText.text = arrayPosition.ToString();

		seedCharArray[8] = selectedEyesPropText.text[0];
	}

	public void NextFacialFeature(int i)
	{
		int arrayPosition = Int32.Parse(selectedFacialFeatureText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.facialFeatures.Length -1;
		else if(arrayPosition >= avatar.appearances.facialFeatures.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.facialFeature.sprite = avatar.appearances.facialFeatures[arrayPosition];
		avatar.AdjustFacialFeatureColour();
		selectedFacialFeatureText.text = arrayPosition.ToString();

		seedCharArray[9] = selectedFacialFeatureText.text[0];
	}

	public void NextHelemt(int i)
	{
		SpaceSuitShouldBeOn(true);

		int arrayPosition = Int32.Parse(selectedHelmetText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.helmets.Length -1;
		else if(arrayPosition >= avatar.appearances.helmets.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.helmet.sprite = avatar.appearances.helmets[arrayPosition];
		selectedHelmetText.text = arrayPosition.ToString();

		seedCharArray[10] = selectedHelmetText.text[0];
	}

	public void NextSpacesuitColour(int i)
	{
		SpaceSuitShouldBeOn(true);

		int arrayPosition = Int32.Parse(selectedSpacesuitColourText.text);
		arrayPosition += i;

		if(arrayPosition < 0) //check if it's less than 0
			arrayPosition = avatar.appearances.spaceSuitColours.Length -1;
		else if(arrayPosition >= avatar.appearances.spaceSuitColours.Length) //check if it's greater than length
			arrayPosition = 0;

		//then set
		avatar.spaceSuit.color = avatar.appearances.spaceSuitColours[arrayPosition];
		avatar.helmet.color = avatar.appearances.spaceSuitColours[arrayPosition];
		selectedSpacesuitColourText.text = arrayPosition.ToString();

		seedCharArray[11] = selectedSpacesuitColourText.text[0];
	}


	#endregion
}

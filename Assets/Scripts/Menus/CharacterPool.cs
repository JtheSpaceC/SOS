using AssemblyCSharp;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterPool : MonoBehaviour {

	public static CharacterPool instance;

	NameGenerator ng;

	[HideInInspector] public CharacterPoolEntry selectedCharacter;
	public GameObject characterPoolPanel;
	public GameObject characterCreationPanel;
	public GameObject characterBioEditPanel;

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

	public GameObject poolEntryPrefab;
	public Transform parentForNewEntries;

	public string[] saveDataOutput; //for debugging in Inspector
	public string allIDs;
	public string [] allIDsArray;




	Character avatar;

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
		

	public void PopulateCharacterPoolList ()
	{
		//this gets rid of the old character entries on the list as it populates new ones based on the save file each time. Prevents duplicates
		DestroyChildEntries ();

		//Load All Characters. Get from a list of saved, active characters. Create a list
		if (ES2.Exists ("allCharacterIDs.es?encrypt=true&password=asswordp")) 
		{
			allIDs = ES2.Load<string> ("allCharacterIDs.es?encrypt=true&password=asswordp");
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
		}
		else
		{
			deselectAllButton.interactable = false;
			deleteSelectedButton.interactable = false;
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

		selectedCharacter.firstName = ng.getRandomFirstName(avatar.gender.ToString().ToCharArray()[0]);
		selectedCharacter.lastName = ng.getRandomLastName();
		selectedCharacter.callsign = ng.getRandomCallsign();
		//TODO: Generate the whole new appearance


		ActivateCharacterEditScreen(selectedCharacter);
	}


	public void ActivateCharacterEditScreen(CharacterPoolEntry selected)
	{
		selectedCharacter = selected;

		avatar.avatarOutput.SetActive(true);

		firstNameEntryText.text = "First Name: "+ selectedCharacter.firstName;
		lastNameEntryText.text = "Last Name: " + selectedCharacter.lastName;
		callsignEntryText.text = "Callsign: " + selectedCharacter.callsign;
		genderEntryText.text = avatar.gender == Character.Gender.Male? "0" : "1";

		characterPoolPanel.SetActive(false);
		characterCreationPanel.SetActive(true);

		characterEditScreenHeaderText.text = selectedCharacter.firstName + " \"" + selectedCharacter.callsign + "\" " + selectedCharacter.lastName;

		//set up Back Button
	}


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
		ActivateCharacterEditScreen(selectedCharacter);
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
		avatar.avatarOutput.SetActive(false);
  	}


	public void SaveCharacter()
	{
		//check if character exists already (if they have an ID) and generate an ID if it's blank

		if(selectedCharacter.characterID == "")
		{
			selectedCharacter.characterID = GenerateCharacterID();
			Debug.Log("Generating new CharacterID: " + selectedCharacter.characterID);
		}

		//collect info
		string saveData = "ID:" + selectedCharacter.characterID + "FN:" + selectedCharacter.firstName +
			"LN:" + selectedCharacter.lastName + "CS:" + selectedCharacter.callsign +
			"BIO:" + selectedCharacter.characterBio;
		//TODO: More here and below

		//for debugging in Inspector
		string[] parameters = new string[]{"ID:","FN:","LN:","CS:", "BIO:"};
		saveDataOutput = saveData.Split(parameters, System.StringSplitOptions.None);
			
		//save it with all character info
		ES2.Save(saveData, selectedCharacter.characterID + ".es?encrypt=true&password=asswordp");

		//save a new list of all character IDs

		allIDs = selectedCharacter.characterID + ',';

		if(ES2.Exists("allCharacterIDs.es?encrypt=true&password=asswordp"))
		{
			string oldSaves = ES2.Load<string>("allCharacterIDs.es?encrypt=true&password=asswordp");

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
		

		ES2.Save(allIDs, "allCharacterIDs.es?encrypt=true&password=asswordp");

		//to see in Inspector (for debugging)
		allIDsArray = allIDs.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);

	}	//end of SaveCharacter()

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
		else if(!ES2.Exists(charID + ".es?encrypt=true&password=asswordp"))
		{
			Debug.Log("No character info found to Load. Returning.");
			return;
		}

		GameObject newPoolEntry = Instantiate(poolEntryPrefab) as GameObject;
		CharacterPoolEntry characterScript = newPoolEntry.GetComponent<CharacterPoolEntry>();
		newPoolEntry.transform.SetParent(parentForNewEntries);


		string characterInfo = ES2.Load<string>(charID + ".es?encrypt=true&password=asswordp");
		string[] parameters = new string[]{"ID:","FN:","LN:","CS:", "BIO:"}; //TODO: more params
		characterScript.savedData = characterInfo.Split(parameters, System.StringSplitOptions.None);

		characterScript.characterID = characterScript.savedData[1];
		characterScript.firstName = characterScript.savedData[2];
		characterScript.lastName = characterScript.savedData[3];
		characterScript.callsign = characterScript.savedData[4];
		characterScript.characterBio = characterScript.savedData[5];

		newPoolEntry.GetComponentInChildren<Text>().text = 
			characterScript.firstName + " \""+ characterScript.callsign + "\" " + characterScript.lastName;
	}


	public void DeleteSelected()
	{
		string removedIDs = "";

		for(int i = 0; i < selectedCharacters.Count; i++)
		{
			if(ES2.Exists(selectedCharacters[i].characterID + ".es?encrypt=true&password=asswordp"))
			{
				ES2.Delete(selectedCharacters[i].characterID + ".es?encrypt=true&password=asswordp");
				print("Deleting");
				removedIDs += selectedCharacters[i].characterID +',';
			}
			else
				print("Couldn't find");
		}

		//SAVE NEW LIST OF ALL IDs
		//first, get the original list

		if (ES2.Exists ("allCharacterIDs.es?encrypt=true&password=asswordp")) 
		{
			allIDs = ES2.Load<string> ("allCharacterIDs.es?encrypt=true&password=asswordp");
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
		ES2.Save(newAllIDs, "allCharacterIDs.es?encrypt=true&password=asswordp");

		//repopulate the list so we can see who's left
		PopulateCharacterPoolList();

	}//end of DeleteSelected()


	void DestroyChildEntries ()
	{
		while (parentForNewEntries.childCount > 0) 
		{
			DestroyImmediate (parentForNewEntries.GetChild (0).gameObject);
		}
	}

}

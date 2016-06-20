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

	public GameObject poolEntryPrefab;

	public string[] saveDataOutput;
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


		PopulateCharactersList();
	}

	void OnEnable()
	{
		//Load All Characters. Get from a list of saved, active characters. Create a list
		//For each in the list, go through and..
		LoadCharacterToPoolList();
	}

	public void PopulateCharactersList()
	{
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
			deselectAllButton.interactable = true;
		else
			deselectAllButton.interactable = false;
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
	}
		
	public void ActivateCharacterEditScreen(CharacterPoolEntry selected)
	{
		selectedCharacter = selected;

		if(selectedCharacter.firstName == "<blank>")
		{
			//TODO: generate a whole new character
			avatar.GenerateRandomNewAppearance();

			selectedCharacter.firstName = ng.getRandomFirstName(avatar.gender.ToString().ToCharArray()[0]);
			selectedCharacter.lastName = ng.getRandomLastName();
			selectedCharacter.callsign = ng.getRandomCallsign();
		}

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
		if(selectedCharacter.characterBio != "")
		{
			input.text = selectedCharacter.characterBio;
		}
			
		selectedCharacter.startingBioText = input.text;	
	}

	public void SetNewBio(InputField input)
	{
		selectedCharacter.characterBio = input.text;
	}

	void ActivateNameEditPanel(string whichName)
	{
		nameHeaderText.text = whichName;
		characterNameEditPanel.SetActive(true);
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

	public void CancelNewBio(InputField input)
	{
		input.text = selectedCharacter.startingBioText;
	}

	public void CloseAllCharacterRelatedWindows()
	{
		characterPoolPanel.SetActive(false);
		characterCreationPanel.SetActive(false);
		characterNameEditPanel.SetActive(false);
		characterBioEditPanel.SetActive(false);
		avatar.avatarOutput.SetActive(false);
	}

	public void SetSelectedCharacterToNull()
	{
		if(selectedCharacter != null)
			selectedCharacter.firstName = "<blank>";
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

		string[] parameters = new string[]{"ID:","FN:","LN:","CS:", "BIO:"};
		saveDataOutput = saveData.Split(parameters, System.StringSplitOptions.None);
			
		//save it with all character info
		ES2.Save(saveData, selectedCharacter.characterID + ".es?encrypt=true&password=asswordp");

		//save a new list of all character IDs

		allIDs = selectedCharacter.characterID + ","; //TODO: Prevent duplicates if the string is already in there

		if(ES2.Exists("allCharacterIDs.es?encrypt=true&password=asswordp"))
		{
			Debug.Log("allCharacterIDs file did exist");

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

	void LoadCharacterToPoolList()
	{
		
	}

}

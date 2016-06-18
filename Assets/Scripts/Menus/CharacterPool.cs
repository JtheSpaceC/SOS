using AssemblyCSharp;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterPool : MonoBehaviour {

	NameGenerator ng;

	CharacterPoolEntry selectedCharacter;
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

	Button deselectAllButton;


	Character avatar;

	void Awake()
	{
		avatar = FindObjectOfType<Character>();
		ng = NameGenerator.Instance;

		deselectAllButton = GameObject.Find("Deselect All Button").GetComponent<Button>();

		PopulateCharactersList();
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

}

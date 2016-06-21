using UnityEngine;

public class CharacterPoolEntry : MonoBehaviour {

	public string characterID = "";
	public string firstName;
	public string lastName;
	public string callsign;

	public string characterBio;
	[HideInInspector] public string startingBioText;

	public string[] savedData;

	public int gender;
	public int body;
	public int skinColour;
	public int nose;
	public int eyes;
	public int hair;
	public int facialHair;
	public int hairColour;
	public int eyesProp;


	public void EditCharacter()
	{
		CharacterPool.instance.ActivateCharacterEditScreen(this);
	}

	public void UpdatePoolListSelection()
	{
		CharacterPool.instance.UpdateSelection();
	}

}

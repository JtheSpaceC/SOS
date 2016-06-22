using UnityEngine;

public class CharacterPoolEntry : MonoBehaviour {

	public string characterID = "";
	public string firstName;
	public string lastName;
	public string callsign;

	public string characterBio;
	[HideInInspector] public string startingBioText;
	public string appearanceSeed;

	public string[] savedData;



	public void EditCharacter()
	{
		CharacterPool.instance.ActivateCharacterEditScreen(this, true);
	}

	public void UpdatePoolListSelection()
	{
		CharacterPool.instance.UpdateSelection();
	}

}

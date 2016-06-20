using UnityEngine;
using System.Collections;

public class CharacterPoolEntry : MonoBehaviour {

	public string characterID = "";
	public string firstName;
	public string lastName;
	public string callsign;

	public string characterBio;
	[HideInInspector] public string startingBioText;


	public void EditCharacter()
	{
		CharacterPool.instance.ActivateCharacterEditScreen(this);
	}


}

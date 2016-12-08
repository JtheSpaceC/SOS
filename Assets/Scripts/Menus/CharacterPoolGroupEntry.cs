using UnityEngine;
using UnityEngine.UI;

public class CharacterPoolGroupEntry : MonoBehaviour {

	public Text nameText;

	public string collectionName = "";
	public string[] savedData;


	public void DeleteThisGroup()
	{
		CharacterPool.instance.selectedCharacterGroup = this;
		CharacterPool.instance.poolDeleteConfirmationPanel.SetActive(true);
		CharacterPool.instance.backOutOfCharacterGroupImportScreenButton.enabled = false;
	}

	public void ReEnableBackOutButton()
	{
		CharacterPool.instance.backOutOfCharacterGroupImportScreenButton.enabled = true;
	}

	public void AddSelectedCharactersToGroup()
	{
		CharacterPool.instance.selectedCharacterGroup = this;

		if(CharacterPool.instance.importOrExport == CharacterPool.ImportOrExport.Export)
		{
			CharacterPool.instance.addToGroupConfirmationPanel.SetActive(true);
		}
		else if(CharacterPool.instance.importOrExport == CharacterPool.ImportOrExport.Import)
		{
			CharacterPool.instance.cpcImportEntireCollectionButton.SetActive(true);
			CharacterPool.instance.PopulateCharactersInImportList(this);
		}
			
	}
}

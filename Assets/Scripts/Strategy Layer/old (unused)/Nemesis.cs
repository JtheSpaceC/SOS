using UnityEngine;
using System.Collections;

public class Nemesis : Personnel {

	[TextArea(10,20)]
	public string bio;

	void Start () 
	{
		nameText.text = personName;
	}
	
	public void SelectNemesis()
	{
		foreach(GameObject button in MenuNemesis.instance.nemesisScreenButtons)
		{
			button.SetActive(true);
		}
		MenuNemesis.instance.selectedNemesis = this.gameObject;
		MenuNemesis.instance.myTextScrollbar.value = 1;
		GetNemesisDescription ();
	}

	void GetNemesisDescription()
	{
		//TODO: get actual info
		MenuNemesis.instance.descriptionText.text = bio;
	//	StartCoroutine (CheckScrollbarSize (MenuNemesis.instance.myTextScrollbar, MenuNemesis.instance.textScrollBarHandle));
	}
}

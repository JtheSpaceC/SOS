using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeBaseManager : MonoBehaviour {

	public static HomeBaseManager instance;

	public GameObject hangar;
	public GameObject bar;
	public GameObject briefing;

	public GameObject blackoutPanel;

	public GameObject choicePanel;
	public Text choiceText;
	public GameObject elevatorPanel;


	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("HomeBaseManager was not null. Destroying one.");
			Destroy(this.gameObject);
		}
	}


	public void DisplayChoiceText(string choiceToDisplay)
	{
		choicePanel.SetActive(true);
		choiceText.text = choiceToDisplay;
	}

	public void DeactivateChoiceText()
	{
		choicePanel.SetActive(false);
	}

	public void ActivateElevator()
	{
		choicePanel.SetActive(false);
		blackoutPanel.SetActive(true);
		elevatorPanel.SetActive(true);
	}

	public void GoToRoom(GameObject newRoom)
	{
		hangar.SetActive(false);
		bar.SetActive(false);
		briefing.SetActive(false);

		elevatorPanel.SetActive(false);

		//change this to a fade
		blackoutPanel.SetActive(false);

		newRoom.SetActive(true);
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Spy : Personnel {

	public enum status {Available, AttemptingInfiltration, OnAssignment, Captured, Dead, AttemptingExfiltration};
	public status myStatus;
	public float lastReport;
	public string lastLocation;
	public int rank = 1;
	public int missionsCompleted = 0;
	public int missionsFailed = 0;
	public int missionsAborted = 0;

	[Header("Attributes")]
	public bool alwaysKnowTheExit = false;
	public bool masterOfDisguise = false;
	public bool propertyDestruction = false;
	public bool resilient = false;
	public bool routine = false;
	public bool opportuneMoment = false;
	public bool deflectBlame = false;
	public bool bestOfPals = false;
	public bool seductive = false;

	int chosenAttribute;
	string descriptionReport;


	void Start () 
	{
		RandomNewAttribute ();
	}


	void RandomNewAttribute()
	{
		chosenAttribute = Random.Range (0, 9);

		switch(chosenAttribute)
		{
			default:
				Debug.LogError("Wrong attribute chosen");
				break;

			case 0:
			if(alwaysKnowTheExit)
				RandomNewAttribute();
			else 
				alwaysKnowTheExit = true;
			break;

		case 1:
			if(masterOfDisguise)
				RandomNewAttribute();
			else 
				masterOfDisguise = true;
			break;

		case 2:
			if(propertyDestruction)
				RandomNewAttribute();
			else 
				propertyDestruction = true;
			break;

		case 3:
			if(resilient)
				RandomNewAttribute();
			else 
				resilient = true;
			break;

		case 4:
			if(routine)
				RandomNewAttribute();
			else 
				routine = true;
			break;

		case 5:
			if(opportuneMoment)
				RandomNewAttribute();
			else 
				opportuneMoment = true;
			break;

		case 6:
			if(deflectBlame)
				RandomNewAttribute();
			else 
				deflectBlame = true;
			break;

		case 7:
			if(bestOfPals)
				RandomNewAttribute();
			else 
				bestOfPals = true;
			break;

		case 8:
			if(seductive)
				RandomNewAttribute();
			else 
				seductive = true;
			break;
		}

	}//end of RandomNewAttribute

	public void SelectAgent()
	{
		foreach (GameObject button in MenuAgents.instance.agentScreenButtons)
			button.SetActive (true);

		MenuAgents.instance.selectedAgent = this.gameObject;
		MenuAgents.instance.myTextScrollbar.value = 1;
		GetAgentDescription ();
	}

	void GetAgentDescription()
	{
		descriptionReport = "Name: " + personName + "\n" +
			"Gender: " + gender + "\n" +
			"Rank: " + rank + "\n\n" +
			"Last Reported In: Day " + lastReport.ToString("n1") + "\n" +
			"Last Known Location: " + lastLocation + "\n\n" +
			"Misisons Completed: " + missionsCompleted + "\n" + 
			"Missions Failed: " + "\n" + 
			"Missions Aborted: " + "\n\n" + 

			"Notable Skills: \n";
		if (alwaysKnowTheExit)
			descriptionReport += "Always Know The Exit\n";
		if (masterOfDisguise)
			descriptionReport += "Master Of Disguise\n";
		if (propertyDestruction)
			descriptionReport += "Property Destruction\n";
		if (resilient)
			descriptionReport += "Resilient\n";
		if (routine)
			descriptionReport += "Routine\n";
		if (opportuneMoment)
			descriptionReport += "Opportune Moment\n";
		if (deflectBlame)
			descriptionReport += "Deflect Blame\n";
		if (bestOfPals)
			descriptionReport += "Best of Pals\n";
		if (seductive)
			descriptionReport += "Seductive\n";

		MenuAgents.instance.descriptionText.text = descriptionReport;
		//StartCoroutine (CheckScrollbarSize (MenuAgents.instance.myTextScrollbar, MenuAgents.instance.textScrollBarHandle));
	}
}//Mono

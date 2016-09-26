﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialOption : MonoBehaviour {

	public Image myImage;
	public float realRotation;

	//these two are for 'owning' a zone of the cursor's rotation
	public float minRotation;
	public float maxRotation;

	public string displayText = "This option";
	public RadialRadioMenu.CurrentRadialScreen myRadialScreen;

	public bool containsFinalCommand = false;


	void Start()
	{
		myImage = GetComponent<Image>();
	}

	public void GetRealRotation()
	{
		//this just corrects rotation into a clockwise 360 that's easier to think about
		realRotation = (360 - transform.localRotation.eulerAngles.z) % 360; 
		if(realRotation == 360)
		{
			realRotation -= 360;
		}
	}

	public void RunMySelection()
	{
		if(myRadialScreen == RadialRadioMenu.CurrentRadialScreen.AllWingmen)
		{
			RadialRadioMenu.instance.selectedWingmen.Clear();
			foreach(GameObject fighter in PlayerAILogic.instance.squadLeaderScript.activeWingmen)
			{
				RadialRadioMenu.instance.selectedWingmen.Add(fighter.GetComponent<AIFighter>());
			}
		}
		else if(myRadialScreen == RadialRadioMenu.CurrentRadialScreen.FirstWingman)
		{
			RadialRadioMenu.instance.selectedWingmen.Clear();
			RadialRadioMenu.instance.selectedWingmen.Add(
				PlayerAILogic.instance.squadLeaderScript.activeWingmen[0].GetComponent<AIFighter>());
		}
		else if(myRadialScreen == RadialRadioMenu.CurrentRadialScreen.SecondWingman)
		{
			RadialRadioMenu.instance.selectedWingmen.Clear();
			RadialRadioMenu.instance.selectedWingmen.Add(
				PlayerAILogic.instance.squadLeaderScript.activeWingmen[1].GetComponent<AIFighter>());
		}
		else if(containsFinalCommand)
		{
			SwitchCommands();
		}
	}

	void SwitchCommands()
	{
		switch(displayText)
		{
			default: Debug.LogError("Something went wrong with Radial Orders: " + displayText);
			break;

		case "Form Up":
			foreach(AIFighter wingman in RadialRadioMenu.instance.selectedWingmen)
			{
				PlayerAILogic.instance.squadLeaderScript.FormUp(wingman);
			}
			break;

		case "Cover Me":
			foreach(AIFighter wingman in RadialRadioMenu.instance.selectedWingmen)
			{
				PlayerAILogic.instance.squadLeaderScript.CoverMe(wingman);
			}
			break;

		case "Fall Back":
			Subtitles.instance.PostHint(new string[]{"FALL BACK BEHAVIOUR NOT IMPLEMENTED YET"});
			break;

		case "Return To Base":
			foreach(AIFighter wingman in RadialRadioMenu.instance.selectedWingmen)
			{
				PlayerAILogic.instance.squadLeaderScript.ReturnToBase(wingman);
			}
			break;

		case "Engage At Will":
			foreach(AIFighter wingman in RadialRadioMenu.instance.selectedWingmen)
			{
				PlayerAILogic.instance.squadLeaderScript.EngageAtWill(wingman);
			}
			break;

		}
	}

}
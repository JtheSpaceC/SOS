using UnityEngine;
using AssemblyCSharp;
using System.Collections.Generic;

[System.Serializable]
public class Names : ScriptableObject {

	public List<string> maleNames;
	public List<string> femaleNames;
	public List<string> lastNames;
	public List<string> callsigns;
	public List<string> callsignsMaleOnly;
	public List<string> callsignsFemaleOnly;


	[ContextMenu("Sort")]
	public void SortAlphabetical()
	{
		RemoveEmpties(maleNames);
		RemoveEmpties(femaleNames);
		RemoveEmpties(lastNames);
		RemoveEmpties(callsigns);
		RemoveEmpties(callsignsMaleOnly);
		RemoveEmpties(callsignsFemaleOnly);

		maleNames.Sort();
		femaleNames.Sort();
		lastNames.Sort();
		callsigns.Sort();
		callsignsMaleOnly.Sort();
		callsignsFemaleOnly.Sort();

		RemoveDuplicates(maleNames, false);
		RemoveDuplicates(femaleNames, false);
		RemoveDuplicates(lastNames, false);
		RemoveDuplicates(callsigns, true);
		RemoveDuplicates(callsignsMaleOnly, false);
		RemoveDuplicates(callsignsFemaleOnly, false);
	}

	void RemoveEmpties(List<string> whichList)
	{
		for(int i = whichList.Count -1; i >= 0; i--)
		{
			if(whichList[i] == "")
				whichList.RemoveAt(i);
		}
	}

	void RemoveDuplicates(List<string> whichList, bool isMainCallsignList)
	{
		for(int i = whichList.Count -1; i > 0; i--)
		{
			if(whichList[i-1] == whichList[i])
			{
				Debug.Log("Removing Duplicate Name: " + whichList[i]);
				whichList.RemoveAt(i);
			}

			if(isMainCallsignList)
			{
				if(callsignsMaleOnly.Contains(whichList[i]) || callsignsFemaleOnly.Contains(whichList[i]))
				{
					Debug.Log("Removing Gender-Specific Callsign from Main Callsigns: " + whichList[i]);
					whichList.RemoveAt(i);
				}
			}
		}
	}

	/*
	[ContextMenu("Import")]
	public void Import()
	{
		string[] males = NameGenerator.Instance.GetAllMaleNames();

		foreach(string male in males)
		{
			if(male.Length > 0)
				maleNames.Add(male);
		}

		string[] females = NameGenerator.Instance.GetAllFemaleNames();

		foreach(string female in females)
		{
			if(female.Length > 0)
				femaleNames.Add(female);
		}

		string[] surnames = NameGenerator.Instance.GetAllLastNames();

		foreach(string surname in surnames)
		{
			if(surname.Length > 0)
				lastNames.Add(surname);
		}

		string[] callSigns = NameGenerator.Instance.GetAllCallsigns();

		foreach(string callsign in callSigns)
		{
			if(callsign.Length > 0)
				callsigns.Add(callsign);
		}
	}*/

}

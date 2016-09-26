﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public static class KevinsCustomEditor : object {


	[MenuItem("Project/Player Prefs/Clear Player Prefs")]
	public static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	[MenuItem("Project/Checklist Before Public Build")]
	public static void PrintBuildChecklist()
	{
		string checklist = 
			"Version number in self-play scene.." +
			"Uncheck 'r' for Restart in all scenes.." +
			"Uncheck Director Screenshot mode.." +
			"Add all scenes to Build Settings.." +
			"Set Analytics.." +
			"Set Self-Play reset timer.." +
			"Check AI Healths.." +
			"Turn off Manual Spawns.." +
			"Set Canvas Resolutions to 1920 x 1080, from 1580 x 889";
		
		string[] checklistItems = checklist.Split(new string[]{"."}, System.StringSplitOptions.RemoveEmptyEntries);
		foreach(string item in checklistItems)
		{
			Debug.Log("<b><color=brown>" + item + "</color></b>", null);
		}
	}
}
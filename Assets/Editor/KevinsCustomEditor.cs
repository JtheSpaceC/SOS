using UnityEngine;
using UnityEditor;
using System.Collections;

public static class KevinsCustomEditor : object {


	[MenuItem("SOS Crow's Nest/Player Prefs/Clear Player Prefs")]
	public static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	[MenuItem("SOS Crow's Nest/Checklist Before Public Build")]
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
			"Set Canvas Resolutions to 1920 x 1080, from 1580 x 889.." +
			"Turn off Asteroid Combat ability.." +
			"Remove Listener from Menu Scene.." +
			"Turn Music back on.." +
			"Enable/Disable website link & Character Pool(+AudioSource).." +
			"Check that self-play level has automatic camera.." +
			"Set Quit Behaviour (application or menu)";
		
		string[] checklistItems = checklist.Split(new string[]{"."}, System.StringSplitOptions.RemoveEmptyEntries);
		foreach(string item in checklistItems)
		{
			Debug.Log("<b><color=brown>" + item + "</color></b>", null);
		}
	}
}

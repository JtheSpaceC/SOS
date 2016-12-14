using UnityEngine;
using UnityEditor;

public static class KevinsCustomEditor : object {
	

	[MenuItem("SOS Crow's Nest/Player Prefs/Clear Player Prefs")]
	public static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	[MenuItem("Tools/Show Prefab Changes")]
	public static void ShowPrefabChanges() 
	{
		PropertyModification[] changes = PrefabUtility.GetPropertyModifications(Selection.activeGameObject);

		if(changes == null)
		{
			Debug.Log("NO CHANGES");
			return;
		}

		foreach(PropertyModification change in changes)
		{
			if(!change.propertyPath.Contains("m_LocalRotation") && !change.propertyPath.Contains("m_LocalPosition")
				&& !change.propertyPath.Contains("m_RootOrder") && !change.propertyPath.Contains("m_Anchor"))
				Debug.Log("CHANGE: " + change.target.name + " " + change.propertyPath + " " + change.value);
		}
	}

	[MenuItem("SOS Crow's Nest/Checklist Before Public Build")]
	public static void PrintBuildChecklist()
	{
		string checklist = 
			"Version number in self-play scene.." +
			"Uncheck 'r' for Restart in all scenes.." +
			"Uncheck Director Screenshot mode.." +
			"Convention mode on InputManager.." +
			"Add all scenes to Build Settings.." +
			"Set Analytics.." +
			"Set Self-Play reset timer.." +
			"Check AI Healths.." +
			"Turn off Manual Spawns.." +
			"Set Canvas Resolutions to 1920 x 1080, from 1580 x 889.." +
			"Turn off Asteroid Combat ability.." +
			"Remove Listener from Menu Scene.." +
			"Turn Music back on.." +
			"Enable/Disable Character Pool(+AudioSource).." +
			"Check that self-play level has automatic camera.." +
			"Set Quit Behaviour (application or menu).." +
			"Website & Feedback links on/off.." +
			"Set Correct Checkpoints (first) in scenes that have them." +
			"Check \"Leave Feedback\" buttons array in Tools in demo level.";
		
		string[] checklistItems = checklist.Split(new string[]{"."}, System.StringSplitOptions.RemoveEmptyEntries);
		foreach(string item in checklistItems)
		{
			Debug.Log("<b><color=brown>" + item + "</color></b>", null);
		}
	}
}
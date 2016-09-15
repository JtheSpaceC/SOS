using UnityEngine;
using UnityEditor;
using System.Collections;

public static class KevinsCustomEditor : object {


	[MenuItem("Project/Clear Player Prefs")]
	public static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Names))]
public class CustomPilotNamesInspector : Editor {

/*	Names myNames;

	void OnEnable()
	{
		myNames = (Names)target;
	}*/

	public override void OnInspectorGUI()
	{
	/*	string[] options = new string[]{"Male Names", "Female Names", "Last Names", "Callsigns"};

		if(GUILayout.Button("Male Names"))
			ShowList(myNames.maleNames.ToArray());
		if(GUILayout.Button("Female Names"))
			ShowList(myNames.femaleNames.ToArray());
		if(GUILayout.Button("Last Names"))
			ShowList(myNames.lastNames.ToArray());
		if(GUILayout.Button("Callsigns"))
			ShowList(myNames.callsigns.ToArray());	

		EditorGUILayout.Popup("Test" ,0, options);*/

		DrawDefaultInspector();
	}

	void ShowList(string[] namesList)
	{
		for(int i = 0; i < namesList.Length; i++)
		{
			namesList[i] = GUILayout.TextField(namesList[i]);
		}
		base.OnInspectorGUI();
	}
}

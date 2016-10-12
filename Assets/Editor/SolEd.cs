using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class SolEd : EditorWindow {

	string unitName = "New Unit";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;
	string myString;

	public ShipStats shipStats;


	//Show The Window possible
	[MenuItem("SOS Crow's Nest/Sol Ed")]
	public static void  ShowWindow () 
	{
		EditorWindow.GetWindow(typeof(SolEd));
	}

	//actual Window Code
	void OnGUI()
	{
		shipStats = (ShipStats)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/ShipStatsHolder.asset", typeof(ShipStats));

		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		unitName = EditorGUILayout.TextField ("Text Field", unitName);

		groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		myBool = EditorGUILayout.Toggle ("Toggle", myBool);
		myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
		EditorGUILayout.EndToggleGroup ();

		if(GUILayout.Button("Create Unit"))
		{
			Debug.Log(shipStats.allFighters[0].level);
		}

		//ShowList(allNames[0]);
	}

	void ShowList(List<string> namesList)
	{
		GUILayout.Space(5);

		for(int i = 0; i < namesList.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();

			namesList[i] = EditorGUILayout.TextField(namesList[i], GUILayout.Width(250));

			if(GUILayout.Button("Delete", GUILayout.Width(50)))
			{
				return; // not necessary but eliminates calls for rest of this frame
			}

			EditorGUILayout.EndHorizontal();
		}

		//base.OnInspectorGUI();
	}

}


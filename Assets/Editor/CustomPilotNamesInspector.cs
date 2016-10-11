using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Names))]
public class CustomPilotNamesInspector : Editor {

	Names myNames;
	int index = 0;
	List<List<string>> allNames = new List<List<string>>();
	Vector2 scrollPos;


	void OnEnable()
	{
		myNames = (Names)target;
		allNames = new List<List<string>>{myNames.maleNames, myNames.femaleNames, myNames.lastNames, myNames.callsigns};
		myNames.SortAlphabetical();
	}

	void OnDisable()
	{
		myNames.SortAlphabetical();
	}


	public override void OnInspectorGUI()
	{
		string[] options = new string[]{"Male Names", "Female Names", "Last Names", "Callsigns"};

		if(GUILayout.Button("Tidy All Lists"))
		{
			myNames.SortAlphabetical();
			scrollPos = Vector2.zero;
		}
		
		GUILayout.BeginVertical("box");	
		GUILayout.Space(5);
			
			index = EditorGUILayout.Popup("Names" , index, options);
			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Names in List:" + allNames[index].Count);
				if(GUILayout.Button("Add Name"))
					AddName();	
		
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

		GUILayout.EndVertical();
		GUILayout.Space(5);

		ShowList(allNames[index]);	

		//DrawDefaultInspector();
	}

	void ShowList(List<string> namesList)
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.Space(5);

		for(int i = 0; i < namesList.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();

			namesList[i] = EditorGUILayout.DelayedTextField(namesList[i], GUILayout.Width(250));

			if(GUILayout.Button("Delete", GUILayout.Width(50)))
			{
				DeleteName(i);
				return; // not necessary but eliminates calls for rest of this frame
			}

			EditorGUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();
		GUILayout.Space(5);
		//base.OnInspectorGUI();
	}

	void AddName()
	{
		allNames[index].Add("");
		scrollPos = new Vector2(0, Mathf.Infinity);
	}

	void DeleteName(int i)
	{
		allNames[index].RemoveAt(i);
	}
}

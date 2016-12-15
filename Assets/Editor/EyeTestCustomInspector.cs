using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EyeTests))]
public class EyeTestCustomInspector : Editor {

	EyeTests script;

	void OnEnable()
	{
		script = (EyeTests)target;
	}

	public override void OnInspectorGUI()
	{
		if(GUILayout.Button("Next Eyes"))
		{
			
		}

		GUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Upper Left"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.upperLeft;
			if(GUILayout.Button("Up"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.up;
			if(GUILayout.Button("Upper Right"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.upperRight;			
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Left"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.left;
			if(GUILayout.Button("Centre"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.neutral;
			if(GUILayout.Button("Right"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.right;	
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Lower Left"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.lowerLeft;
			if(GUILayout.Button("Down"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.down;
			if(GUILayout.Button("Lower Right"))
				script.myCharacter.eyeballs.localPosition = script.myCharacter.lowerRight;	
		}
		GUILayout.EndHorizontal();

	}
}

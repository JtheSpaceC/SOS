using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EyeTests))]
public class EyeTestCustomInspector : Editor {

	EyeTests script;
	Character avatar;
	int currentEyeSet = 0;

	void OnEnable()
	{
		script = (EyeTests)target;
		avatar = script.myCharacter;
	}

	public override void OnInspectorGUI()
	{
		Object[] eyeThingsToUndo = new Object[]
		{avatar.eyeballs, avatar.eyeLids, avatar.eyeWhites, avatar.eyeShine, avatar.eyeIrises, avatar.eyesBlinking};
		Undo.RecordObjects(eyeThingsToUndo, "Changes to Avatar Eyes");

		GUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Previous Eyes"))
			{
				currentEyeSet--;
				ChangeEyes ();
			}
			if(GUILayout.Button("Next Eyes"))
			{
				currentEyeSet++;
				ChangeEyes ();
			}
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Current Eye Set: " + currentEyeSet, EditorStyles.helpBox);


		GUILayout.BeginVertical("box");	
		{
			EditorGUILayout.LabelField("View Positions.", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Upper Left"))
					avatar.eyeballs.localPosition = avatar.upperLeft;
				if(GUILayout.Button("Up"))
					avatar.eyeballs.localPosition = avatar.up;
				if(GUILayout.Button("Upper Right"))
					avatar.eyeballs.localPosition = avatar.upperRight;			
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Left"))
					avatar.eyeballs.localPosition = avatar.left;
				if(GUILayout.Button("Centre"))
					avatar.eyeballs.localPosition = avatar.neutral;
				if(GUILayout.Button("Right"))
					avatar.eyeballs.localPosition = avatar.right;	
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Lower Left"))
					avatar.eyeballs.localPosition = avatar.lowerLeft;
				if(GUILayout.Button("Down"))
					avatar.eyeballs.localPosition = avatar.down;
				if(GUILayout.Button("Lower Right"))
					avatar.eyeballs.localPosition = avatar.lowerRight;	
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();


		GUILayout.Space(10);

		GUILayout.BeginVertical("box");	
		{
			EditorGUILayout.LabelField("CAUTION: Use only to set!", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Sets whichever button is chosen below to be the current eye position.");

			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Set Upper Left"))
					avatar.upperLeft = avatar.eyeballs.localPosition;
				if(GUILayout.Button("Set Up"))
					avatar.up = avatar.eyeballs.localPosition;
				if(GUILayout.Button("Set Upper Right"))
					avatar.upperRight = avatar.eyeballs.localPosition;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Set Left"))
					avatar.left = avatar.eyeballs.localPosition;
				if(GUILayout.Button("Set Centre"))
					avatar.neutral = avatar.eyeballs.localPosition;
				if(GUILayout.Button("Set Right"))
					avatar.right = avatar.eyeballs.localPosition;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Set Lower Left"))
					avatar.lowerLeft = avatar.eyeballs.localPosition;
				if(GUILayout.Button("Set Down"))
					avatar.down = avatar.eyeballs.localPosition;
				if(GUILayout.Button("Set Lower Right"))
					avatar.lowerRight = avatar.eyeballs.localPosition;
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		//saves Data
		EditorUtility.SetDirty(avatar);
	}

	void ChangeEyes ()
	{
		if (currentEyeSet < 0)
			//check if it's less than 0
			currentEyeSet = (avatar.myAppearance.eyes.Length / 5) - 1;
		else
			if (currentEyeSet >= avatar.myAppearance.eyes.Length / 5)
				//check if it's greater than length
				currentEyeSet = 0;
		//then set
		avatar.eyeLids.sprite = avatar.myAppearance.eyes [currentEyeSet * 5];
		avatar.eyeWhites.sprite = avatar.myAppearance.eyes [(currentEyeSet * 5) + 1];
		avatar.eyeIrises.sprite = avatar.myAppearance.eyes [(currentEyeSet * 5) + 2];
		avatar.eyeShine.sprite = avatar.myAppearance.eyes [(currentEyeSet * 5) + 3];
		avatar.eyesBlinking.sprite = avatar.myAppearance.eyes [(currentEyeSet * 5) + 4];
	}
}

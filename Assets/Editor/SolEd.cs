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

	public int toolbarInt = 0;
	public string[] toolbarStrings = new string[]{"Fighters", "Bombers", "Support", "Capital", "Pilots"};
	Vector2 scrollPos = Vector2.zero;
	Rect windowWidth;
	float headerWidth = 131f;
	int numOfColumns = 7;

	//Show The Window possible
	[MenuItem("SOS Crow's Nest/Sol Ed")]
	public static void  ShowWindow () 
	{
		EditorWindow.GetWindow(typeof(SolEd));
	}

	void OnFocus()
	{
		Debug.Log("OnFocus at " + System.DateTime.UtcNow);
		shipStats = (ShipStats)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/ShipStatsHolder.asset", typeof(ShipStats));
		shipStats.arrowFighters.Clear();
		shipStats.mantisFighters.Clear();

		foreach(Fighter fighter in shipStats.allFighters)
		{
			if(fighter.myShipType == Fighter.ShipType.Arrow)
			{
				shipStats.arrowFighters.Add(fighter);
			}
			else if(fighter.myShipType == Fighter.ShipType.Mantis)
			{
				shipStats.mantisFighters.Add(fighter);
			}
		}
	}

	//actual Window Code
	void OnGUI()
	{
		shipStats = (ShipStats)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/ShipStatsHolder.asset", typeof(ShipStats));

		GUILayout.Space(10);
		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

		if(toolbarInt == 0) //SHIPS
		{
			GUILayout.Space(5);

			//HEADERS for fields
			GUILayout.BeginHorizontal("box");
			{				
				GUILayout.Label("Fighter Type", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Level", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Max Health", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Starting Awareness", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Max Awareness", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Snap Focus", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Awareness Recharge", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
			}
			EditorGUILayout.EndHorizontal();

			//scroll bar for editing section
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				EditorGUILayout.BeginVertical("box");
				{
					for(int i = 0; i < shipStats.allFighters.Count; i++)
					{
						GUILayout.Space(5);
						windowWidth = EditorGUILayout.BeginHorizontal();
						{
							shipStats.allFighters[i].myShipType = (Fighter.ShipType)EditorGUILayout.EnumPopup(shipStats.allFighters[i].myShipType);
							shipStats.allFighters[i].level = EditorGUILayout.IntField(shipStats.allFighters[i].level);
							shipStats.allFighters[i].maxHealth = EditorGUILayout.IntField(shipStats.allFighters[i].maxHealth);
							shipStats.allFighters[i].startingAwareness = EditorGUILayout.IntField(shipStats.allFighters[i].startingAwareness);
							shipStats.allFighters[i].maxAwareness = EditorGUILayout.IntField(shipStats.allFighters[i].maxAwareness);
							shipStats.allFighters[i].snapFocus = EditorGUILayout.IntField(shipStats.allFighters[i].snapFocus);
							shipStats.allFighters[i].awarenessRecharge = EditorGUILayout.FloatField(shipStats.allFighters[i].awarenessRecharge);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();

			//SAMPLE CODE
			GUILayout.Space(30);

			unitName = EditorGUILayout.TextField ("Text Field", unitName);

			groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
			EditorGUILayout.EndToggleGroup ();

			if(GUILayout.Button("Create Unit"))
			{
				Debug.Log(shipStats.allFighters[0].level);
			}
		}
		else //PILOTS AND OTHER
		{
			EditorGUILayout.LabelField("Nothing available for this option. Hurry up and finish making it!");
		}

		headerWidth = (windowWidth.width - 25)/numOfColumns;
	}


}


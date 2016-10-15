using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class SolEd : EditorWindow {

	string unitName = "New Unit";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;
	string myString;

	static Vector2 s_WindowsMinSize = Vector2.one * 300.0f;

	public ShipStats shipStats;

	public int toolbarInt = 0;
	public string[] toolbarStrings = new string[]{"Fighters", "Bombers", "Support", "Capital", "Pilots"};
	Vector2 scrollPos = Vector2.zero;
	Rect fighterInfoWindow = new Rect(Vector2.zero, s_WindowsMinSize);
	float headerWidth = 131f;
	int numOfColumns;

	//Show The Window possible
	[MenuItem("SOS Crow's Nest/Sol Ed")]
	public static void  ShowWindow () 
	{
		EditorWindow.GetWindow(typeof(SolEd));
	}

	void OnFocus()
	{
		shipStats = (ShipStats)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/ShipStatsHolder.asset", typeof(ShipStats));
		numOfColumns = 7;
		/*foreach(Fighter fighter in shipStats.allFighters)
		{
			if(fighter.myShipType == Fighter.ShipType.Arrow)
			{
				shipStats.arrowFighters.Add(fighter);
			}
			else if(fighter.myShipType == Fighter.ShipType.Mantis)
			{
				shipStats.mantisFighters.Add(fighter);
			}
		}*/
	}

	//actual Window Code
	void OnGUI()
	{
		shipStats = (ShipStats)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/ShipStatsHolder.asset", typeof(ShipStats));

		GUILayout.Space(10);
		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

		#region FIGHTERS
		if(toolbarInt == 0) //SHIPS
		{
			GUILayout.Space(5);

			//HEADERS for fields
			GUILayout.BeginHorizontal("box");
			{
				GUILayout.Label("Level", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Max Health", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Starting Awareness", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Max Awareness", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Snap Focus", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Awareness Recharge", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));

				GUILayout.Label("Special Ship Code", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
			}

			EditorGUILayout.EndHorizontal();

			//scroll bar for editing section
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				ListLayout("Arrow", shipStats.arrowFighters);
				ListLayout("Mantis", shipStats.mantisFighters);
			}
			EditorGUILayout.EndScrollView();

			//SAMPLE CODE
			GUILayout.Space(30);

			groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
			EditorGUILayout.EndToggleGroup();
		}
		#endregion


		#region Pilots And Other
		else //PILOTS AND OTHER
		{
			EditorGUILayout.LabelField("Nothing available for this option. Hurry up and finish making it!");
		}
		#endregion

		if(fighterInfoWindow.width != 0)
		{
			headerWidth = (fighterInfoWindow.width - 25)/numOfColumns;
		}
	}

	void ListLayout(string heading, List<Fighter> whichList)
	{		

		EditorGUILayout.BeginVertical("box");
		{
			GUILayout.Label(heading, EditorStyles.boldLabel, GUILayout.Width(headerWidth));

			for(int i = 0; i < whichList.Count; i++)
			{
				fighterInfoWindow = EditorGUILayout.BeginHorizontal();
				{
					//whichList[i].myShipType = (Fighter.ShipType)EditorGUILayout.EnumPopup(whichList[i].myShipType);
					whichList[i].level = i+1; 
					EditorGUILayout.LabelField("Level: " + whichList[i].level, GUILayout.Width(headerWidth));
					whichList[i].maxHealth = EditorGUILayout.IntField(whichList[i].maxHealth);
					whichList[i].startingAwareness = EditorGUILayout.IntField(whichList[i].startingAwareness);
					whichList[i].maxAwareness = EditorGUILayout.IntField(whichList[i].maxAwareness);
					whichList[i].snapFocus = EditorGUILayout.IntField(whichList[i].snapFocus);
					whichList[i].awarenessRecharge = EditorGUILayout.FloatField(whichList[i].awarenessRecharge);

					whichList[i].specialShip = EditorGUILayout.TextField(whichList[i].specialShip);
				}
				EditorGUILayout.EndHorizontal();
			}	
			EditorGUILayout.BeginHorizontal(); //Add/Remove buttons
			{
				if(GUILayout.Button("Add", GUILayout.Width(headerWidth/2)))
				{
					whichList.Add(new Fighter());
				}
				if(GUILayout.Button("Remove", GUILayout.Width(headerWidth/2)))
				{
					if(whichList.Count > 0)
						whichList.Remove(whichList[whichList.Count-1]);
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		GUILayout.Space(5);
	}//end of List Layout
}


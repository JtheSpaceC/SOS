using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class SolEd : EditorWindow {

	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;
	string myString;

	static Vector2 s_WindowsMinSize = Vector2.one * 300.0f;

	public ShipStats shipStats;
	public Icons icons;

	public int toolbarInt = 0;
	public string[] toolbarStrings = new string[]{"Fighters", "Bombers", "Support", "Capital", "Pilots"};
	Vector2 scrollPos = Vector2.zero;
	Rect fighterInfoWindow = new Rect(Vector2.zero, s_WindowsMinSize);
	Rect copyAndDeleteButtonsSpace = new Rect(Vector2.zero, s_WindowsMinSize);
	float headerWidth = 131f;
	float copyAndDeleteButtonsHeaderWidth = 90f;
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
		icons = (Icons)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/Icons.asset", typeof(Icons));

		numOfColumns = 12; //excludes the delete & copy column
	}

	//actual Window Code
	void OnGUI()
	{
		shipStats = (ShipStats)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/ShipStatsHolder.asset", typeof(ShipStats));
		icons = (Icons)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scriptable Objects/Icons.asset", typeof(Icons));

		GUILayout.Space(10);
		toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

		#region FIGHTERS
		if(toolbarInt == 0) //SHIPS
		{
			GUILayout.Space(5);

			//HEADERS for fields
			GUILayout.BeginHorizontal("box");
			{
				GUILayout.Label("Delete & Copy", EditorStyles.wordWrappedLabel, GUILayout.Width(copyAndDeleteButtonsHeaderWidth));
				GUILayout.Label("Level", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Max Health", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Starting S.A.", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Max S.A.", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Snap Focus", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("S.A. Recharge", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));

				GUILayout.Label("Dodge Front", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Dodge Side", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Dodge Rear", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Missile Mult.", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));
				GUILayout.Label("Asteroid Mult. ", EditorStyles.wordWrappedLabel, GUILayout.Width(headerWidth));

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
			headerWidth = (fighterInfoWindow.width - 45)/numOfColumns;
		}
		if(copyAndDeleteButtonsSpace.width != 0)
		{
			copyAndDeleteButtonsHeaderWidth = copyAndDeleteButtonsSpace.width;
		}
	}

	void ListLayout(string heading, List<Fighter> whichList)
	{		

		EditorGUILayout.BeginVertical("box");
		{
			GUILayout.Label(heading, EditorStyles.boldLabel, GUILayout.Width(headerWidth));

			for(int i = 0; i < whichList.Count; i++)
			{
				EditorGUILayout.BeginHorizontal(); //this joins the Horizontals for copy/del buttons & other fighter stats
				{
					copyAndDeleteButtonsSpace = EditorGUILayout.BeginHorizontal();
					{
						//DELETE BUTTON
						if(GUILayout.Button("X"))
						{
							whichList.RemoveAt(i);
							return;
						}
						//COPY BUTTON
						if(GUILayout.Button("C"))
						{
							Fighter copiedFighter = Fighter.CopyFighter(whichList[i]);
							whichList.Insert(i+1, copiedFighter);
							return;
						}
					}
					EditorGUILayout.EndHorizontal();

					fighterInfoWindow = EditorGUILayout.BeginHorizontal();
					{
						//ALL STATS
						whichList[i].level = i+1; 
						if(whichList[i].specialShip == "")
							EditorGUILayout.LabelField("Level: " + whichList[i].level, GUILayout.Width(headerWidth));
						else
							EditorGUILayout.LabelField("Special: ", GUILayout.Width(headerWidth));					
						whichList[i].maxHealth = EditorGUILayout.IntField(whichList[i].maxHealth);
						whichList[i].startingAwareness = EditorGUILayout.IntField(whichList[i].startingAwareness);
						whichList[i].maxAwareness = EditorGUILayout.IntField(whichList[i].maxAwareness);
						whichList[i].snapFocus = EditorGUILayout.IntField(whichList[i].snapFocus);
						whichList[i].awarenessRecharge = EditorGUILayout.FloatField(whichList[i].awarenessRecharge);

						whichList[i].dodgeSkillFront = EditorGUILayout.FloatField(whichList[i].dodgeSkillFront);
						whichList[i].dodgeSkillSide = EditorGUILayout.FloatField(whichList[i].dodgeSkillSide);
						whichList[i].dodgeSkillRear = EditorGUILayout.FloatField(whichList[i].dodgeSkillRear);
						whichList[i].missileMultiplier = EditorGUILayout.FloatField(whichList[i].missileMultiplier);
						whichList[i].asteroidMultiplier = EditorGUILayout.FloatField(whichList[i].asteroidMultiplier);

						whichList[i].specialShip = EditorGUILayout.TextField(whichList[i].specialShip);
					}
					EditorGUILayout.EndHorizontal();
				}
			EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal(); //Add/Remove buttons
			{
				if(GUILayout.Button("Add", GUILayout.Width(headerWidth)))
				{
					whichList.Add(new Fighter());
				}
				if(GUILayout.Button("Remove", GUILayout.Width(headerWidth)))
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
		
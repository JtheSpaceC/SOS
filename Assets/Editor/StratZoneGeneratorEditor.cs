using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StratZoneGenerator))]
public class StratZoneGeneratorEditor : Editor {

	StratZoneGenerator stratZoneGenScript;

	void OnEnable()
	{
		stratZoneGenScript = (StratZoneGenerator)target;
		if(stratZoneGenScript.zoneParent == null && GameObject.Find("Zone"))
		{
			stratZoneGenScript.zoneParent = GameObject.Find("Zone");
		}			
	}
	
	public override void OnInspectorGUI()
	{
		Undo.RecordObject(stratZoneGenScript, "Strategy Zone Script Changes");

		if(GUILayout.Button("Generate New Zone"))
		{
			stratZoneGenScript.ClearZone();

			stratZoneGenScript.GenerateNewZone();
		}

		if(GUILayout.Button("Clear Zone"))
		{
			stratZoneGenScript.ClearZone();
		}

		GUILayout.Label("Min Points of Interest");
		stratZoneGenScript.minPointsOfInterest = 
			EditorGUILayout.IntSlider(stratZoneGenScript.minPointsOfInterest, 1, 20);
		
		GUILayout.Label("Max Points of Interest");
		stratZoneGenScript.maxPointsOfInterest = 
			EditorGUILayout.IntSlider(stratZoneGenScript.maxPointsOfInterest, 1, 20);
		
		GUILayout.Label("Min Connections per POI");
		stratZoneGenScript.minConnections = 
			EditorGUILayout.IntSlider(stratZoneGenScript.minConnections, 1, 5);

		GUILayout.Label("Max Connections per POI");
		stratZoneGenScript.maxConnections = 
			EditorGUILayout.IntSlider(stratZoneGenScript.maxConnections, 1, 5);

		//saves Data
		EditorUtility.SetDirty(stratZoneGenScript);
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpreadsheetGetter : MonoBehaviour {

	public static SpreadsheetGetter instance;

	//will output info here for debugging
	[TextArea(4, 10)]
	public string info;
	public ShipInfo publiclyVisibleShipInfo;

	[System.Serializable]
	public struct ShipInfo
	{
		public string shipType; 
		public int level; 
		public int maxHealth;
		public int startingAwareness;
		public int maxAwareness;
		public int snapFocus;
		public float awarenessRecharge;
	}

	private static ShipInfo shipInfo;

	void Start()
	{
		instance = this;
		SpreadsheetGetter.GetShipInfo();
	}

	[MenuItem("Project/Import Ship Info")]
	public static void GetShipInfo() //from Google Sheets
	{
		CloudConnectorCore.processedResponseCallback.AddListener(SpreadsheetGetter.ParseData);
		CloudConnectorCore.GetObjectsByField("FighterStats", "Level", "7", true);
		//CloudConnectorCore.GetTable("FighterStats", true);
	}

	// Parse data received from the cloud.
	public static void ParseData(CloudConnectorCore.QueryType query, List<string> objTypeNames, List<string> jsonData)
	{
		for (int i = 0; i < objTypeNames.Count; i++)
		{
			Debug.Log("Data type/table: " + objTypeNames[i]);
		}

		// First check the type of answer.
		if (query == CloudConnectorCore.QueryType.getObjects)
		{
			// Check if the type is correct.
			if (string.Compare(objTypeNames[0], "FighterStats") == 0)
			{
				// Parse from json to the desired object type.
				ShipInfo[] shipInfos = GSFUJsonHelper.JsonArray<ShipInfo>(jsonData[0]);

				shipInfo = shipInfos[0];

				SpreadsheetGetter.instance.info = "jsonData (" + jsonData.Count +")\n\n";
				SpreadsheetGetter.instance.info += jsonData[0] +"\n";
				SpreadsheetGetter.instance.info += shipInfo.shipType + "\n";
				SpreadsheetGetter.instance.info += shipInfo.startingAwareness + "\n";
				SpreadsheetGetter.instance.info += shipInfo.maxHealth + "\n";

				SpreadsheetGetter.instance.publiclyVisibleShipInfo = shipInfo;
			}
		}

		// First check the type of answer.
		else if (query == CloudConnectorCore.QueryType.getTable)
		{
			SpreadsheetGetter.instance.info += "\n" + "jsonData (" + jsonData.Count + ")\n";
			foreach(string data in jsonData)
			{
				SpreadsheetGetter.instance.info += data + "\n\n";
			}
//
//			// Check if the type is correct.
//			if (string.Compare(objTypeNames[0], tableName) == 0)
//			{
//				// Parse from json to the desired object type.
//				ShipInfo[] players = GSFUJsonHelper.JsonArray<ShipInfo>(jsonData[0]);
//
//				string logMsg = "<color=yellow>" + players.Length.ToString() + " objects retrieved from the cloud and parsed:</color>";
//				for (int i = 0; i < players.Length; i++)
//				{
//					logMsg += "\n" +
//						"<color=blue>Name: " + players[i].name + "</color>\n" +
//						"Level: " + players[i].level + "\n" +
//						"Health: " + players[i].health + "\n" +
//						"Role: " + players[i].role + "\n";				
//				}
//				Debug.Log(logMsg);
//			}
		}

		// First check the type of answer.
		else if (query == CloudConnectorCore.QueryType.getAllTables)
		{
			print("GET ALL TABLES");

//			// Just dump all content to the console, sorted by table name.
//			string logMsg = "<color=yellow>All data tables retrieved from the cloud.\n</color>";
//			for (int i = 0; i < objTypeNames.Count; i++)
//			{
//				logMsg += "<color=blue>Table Name: " + objTypeNames[i] + "</color>\n"
//					+ jsonData[i] + "\n";
//			}
//			Debug.Log(logMsg);
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ActivityLog : MonoBehaviour {

	public static ActivityLog instance;
	public Text displayText;
	string wholeLog = "";

	List<LogEntry> logEntryList = new List<LogEntry>();


	void Awake () 
	{
		if (instance == null) 
		{
			instance = this;
		}
		else 
		{
			Debug.LogError("Duplicate instances " + gameObject.name);
			Destroy (gameObject);
		}
		CreateRandomLogEntries (40);
	}

	void CreateRandomLogEntries(int howMany)
	{
		for (int i = 0; i < howMany; i++)
		{
			float date = Random.Range(i* RTSDirector.instance.gameDay, ((i+1))*RTSDirector.instance.gameDay)/ howMany;

			LogEntry scratchLogEntry = new LogEntry();

			scratchLogEntry.date = date;
			//scratchLogEntry.location = whatever;
			scratchLogEntry.who = scratchLogEntry.whoArray[Random.Range(0, scratchLogEntry.whoArray.Length)] ;
			scratchLogEntry.didWhat = scratchLogEntry.didWhatArray[Random.Range(0, scratchLogEntry.didWhatArray.Length)];

			scratchLogEntry.whatSubject = scratchLogEntry.whatSubjectArray[Random.Range(0, scratchLogEntry.whatSubjectArray.Length)];
			if(scratchLogEntry.whatSubject == "floating debris")
				scratchLogEntry.doingWhat = "";
			else
				scratchLogEntry.doingWhat = scratchLogEntry.doingWhatArray[Random.Range(0, scratchLogEntry.doingWhatArray.Length)];

			scratchLogEntry.where = scratchLogEntry.whereArray[Random.Range(0, scratchLogEntry.whereArray.Length)];

			scratchLogEntry.message = "Day " + scratchLogEntry.date.ToString("n1") + ": "+
				scratchLogEntry.who+ " "+
				scratchLogEntry.didWhat + " "+ 
				scratchLogEntry.whatSubject	+ " "+
				scratchLogEntry.doingWhat + " "+
				scratchLogEntry.where + ".\n\n";

			logEntryList.Add(scratchLogEntry);
		}
	}

	void OnEnable()
	{
		DisplayLogEntries ();
	}

	void DisplayLogEntries()
	{
		for(int i = 0; i < logEntryList.Count; i++)
		{
			wholeLog = logEntryList[i].message + wholeLog;
		}

		displayText.text = wholeLog;
	}

	public void CreateLogEntry(Vector2 where, string incomingMessage)
	{
		LogEntry scratchLogEntry = new LogEntry();
		
		scratchLogEntry.date = RTSDirector.instance.gameDay;
		scratchLogEntry.location = where;
		scratchLogEntry.message = "Day: " + scratchLogEntry.date.ToString ("n1") + ": " +
			incomingMessage;

		logEntryList.Add(scratchLogEntry);
		Subtitles.instance.PostHint(new string[] {logEntryList[logEntryList.Count-1].message});
	}

}//Mono

public class LogEntry
{
	public float date;
	public Vector2 location;
	public string who;
	public string didWhat;
	public string whatSubject;
	public string doingWhat;
	public string where;

	public string message;

	public string[] whoArray = new string[] {"Civilan traffic", "An independent prospecting company", "A government scout"};
	public string[] didWhatArray = new string[] {"reported seeing", "reported"};
	public string[] whatSubjectArray = new string[] {"a strange object on their sensors", "a large military ship", "floating debris", 
		"what looked like a group of fighter craft", "what might have been an enemy scout", "enemy fighters", "a small warship"};
	public string[] doingWhatArray = new string[] {"moving quickly", "lingering", "doing a sensor sweep", "heading out-system",
		"heading in-system"};
	public string[] whereArray = new string[] {"near Planet A", "near Moon A1", "near Planet B", "near Planet C", "near Moon C1",
		"near Mining Station 1", "near Mining Station 2", "near part of the Ookran Asteroid Field"};
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionSetup : MonoBehaviour {

	public static MissionSetup instance;

	[HideInInspector] public bool inMission = false;

	public enum MissionType {Patrol, Raid, Battle}
	public MissionType missionType;

	public enum InsertionType {AlreadyPresent, WarpIn, LeaveHangar, RestInPosition, NotPresent} //not present might be used for cutscenes or MainMenu
	public InsertionType insertionType;

	public MissionUnitInfo playerCraft;
	public List<MissionUnitInfo> warpingCraft = new List<MissionUnitInfo>(); //if we're warping in, we'll have to attach to this. Player's group first, any friendlies after

	[Tooltip("The ship/station whose hangar the player will exit at mission start.")]
	public MissionUnitInfo hangarCraft; //if flying out of a hangar, we'll need this. It should normally be one of the craft from the PMC craft list
	public List<MissionUnitInfo> playerSquad = new List<MissionUnitInfo>();
	public List<MissionUnitInfo> pmcCraft = new List<MissionUnitInfo>(); //don't include squad mates, just other support ships
	public List<MissionUnitInfo> enemyCraft = new List<MissionUnitInfo>();
	public List<MissionUnitInfo> civilianCraft = new List<MissionUnitInfo>();

	public GameObject backgroundForArea;

	[Header("Prefabs for every ship")]
	public GameObject arrowPlayer;
	public GameObject arrowAI;
	public GameObject transportAI;
	public GameObject shuttleAI;
	public GameObject stormwallAI;
	public GameObject pmcHomeShip;


	void Awake()
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			Destroy(gameObject);
			Debug.Log("There were two MissionSetup scripts. Destroying one.");
		}
	}

	public void ValidateChoices()
	{
		//make sure if we're leaving hangar, there's a hangar to leave, etc

		if(insertionType == InsertionType.WarpIn && warpingCraft[0] == null && playerCraft != null)
		{
			Debug.Log("Setting Insertion Type to AlreadyPresent. There was no warping craft listed.");
			insertionType = InsertionType.AlreadyPresent;
		}
		else if(insertionType == InsertionType.LeaveHangar && hangarCraft == null && playerCraft != null)
		{
			Debug.Log("Setting Insertion Type to AlreadyPresent. There was no hangar craft listed.");
			insertionType = InsertionType.AlreadyPresent;
		}
		else if(insertionType == InsertionType.RestInPosition && (warpingCraft == null || hangarCraft == null) && playerCraft != null)
		{
			Debug.Log("Setting Insertion Type to AlreadyPresent. There was nothing to RestIn.");
			insertionType = InsertionType.AlreadyPresent;
		}
		else if(playerCraft == null)
		{
			Debug.Log("Setting Insertion Type to NotPresent. No Player set.");
			insertionType = InsertionType.NotPresent;		
		}

		inMission = true;

	}//end of ValidateChoices

	void Update()
	{
		if(inMission)
		{
			if(Input.GetKeyDown(KeyCode.X))
			{
				ClickToPlay.instance.LoadScene("rts_CAG walkabout");
			}
		}
	}


}//end of script


public class MissionUnitInfo{

	public enum WhichSide {PMC, Enemy, Civilian}
	public WhichSide whichSide;

	public GameObject shipType;

	public bool activeAtStart = true;

	public enum PrimaryWeapon{DualCannon}
	public PrimaryWeapon primaryWeapon;

	public string appearance;
	public string skill;
	public string firstName;
	public string lastName;
	public string callSign;

	public Transform dockedWith;

}
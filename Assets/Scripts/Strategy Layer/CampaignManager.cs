using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CampaignManager : MonoBehaviour {

	public static CampaignManager instance;

	public GameObject sectorsContainer;
	public Sector[] allSectors;
	[HideInInspector] public Sector activeSector;

	public List<Mission> availableMissions;

	string infoString;

	public Text missionsListText;

	
	void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were two CampaignManagers. Destroying one.");
			Destroy(this.gameObject);
		}

		availableMissions = new List<Mission>();
		allSectors = sectorsContainer.GetComponentsInChildren<Sector>();
	}

	public void ToggleHoloTableInfo()
	{
		CAGManager.instance.contextualText.text = "";

		if(!CAGManager.instance.map.activeSelf)
		{
			CAGManager.instance.map.SetActive(true);
			CAGManager.instance.mapInfoPanel.SetActive(false);
		}
		else
		{
			CAGManager.instance.map.SetActive(false);
			CAGManager.instance.mapInfoPanel.SetActive(true);
		}
	}

	public void PopulateMapInfoPanelForSector()
	{
		CAGManager.instance.reconButton.gameObject.SetActive(true);
		CAGManager.instance.reconButton.interactable = true;

		infoString = "";

		infoString += "Last Scouted: ";
		if(activeSector.dayScouted == -1)			
			infoString += "N/A" + "\n";
		else
			infoString += activeSector.dayScouted + "\n";

		if(activeSector.baseType == Sector.BaseType.None && !activeSector.enemyFleetPresent)
			infoString += "This sector is believed to be void of any pirate or civilian activity.\n";
		else if(activeSector.baseType == Sector.BaseType.Civilian)
			infoString += "There is a civilian presence in this sector.\n";
		else if(activeSector.baseType == Sector.BaseType.Enemy)
			infoString += "A pirate base is known to be in this area.\n";

		if(activeSector.enemyFleetPresent)
			infoString += "A pirate fleet is suspected to be operating in this area.\n";		
	
		CheckForExistingMissions();

		CAGManager.instance.mapInfoHeaderText.text = activeSector.name;
		CAGManager.instance.mapInfoBodyText.text = infoString;
	}

	public void PopulateMapInfoPanelForSpyDrone(Transform satellite)
	{
		CAGManager.instance.reconButton.gameObject.SetActive(false);

		infoString = "This stealthed spy drone sits in empty space near " + satellite.transform.parent.parent.name + " passively monitoring " +
			"the entire sector.\n" +
			"Obstruction from planetary bodies and from asteroids, as well as the need not to use active sensors, prevents this drone " +
			"from gathering detailed information about the sub-sectors that it monitors.\n" +
			"It will, however, detect any large ships entering or leaving the Sector. " +
			"Apart from providing valuable intelligence, the presence of these drones is reassuring for the civilian population.";

		CAGManager.instance.mapInfoHeaderText.text = "Spy Drone";
		CAGManager.instance.mapInfoBodyText.text = infoString;
	}

	void CheckForExistingMissions()
	{
		if(availableMissions.Count == 0)
			return;
		
		for(int i = 0; i < availableMissions.Count; i++)
		{
			if(availableMissions[i].missionSector == activeSector)
			{
				infoString += "\nSCHEDULED MISSIONS: \n" + 
					StaticTools.SplitCamelCase(availableMissions[i].missionType.ToString()) + "\n";
				
				if(availableMissions[i].missionType == Mission.MissionType.Recon)
					CAGManager.instance.reconButton.interactable = false;
			}
		}
	}

	public void RequestScheduleReconMission()
	{
		//TODO: CheckForCraftAvailabilityEtc()

		ScheduleReconMission();
		CAGManager.instance.reconButton.interactable = false;
	}
	void ScheduleReconMission()
	{
		Mission newMission = new Mission();
		newMission.missionType = Mission.MissionType.Recon;
		newMission.missionSector = activeSector;
		newMission.dayCalled = CAGManager.instance.gameDay;

		availableMissions.Add(newMission);
		missionsListText.text += "\n" 
			+ StaticTools.SplitCamelCase(newMission.missionType.ToString())
			+ " " + newMission.missionSector.name;
	}
}

public class Mission{
	
	public Sector missionSector;
	public enum MissionType{Recon, StealthRecon, BaseAssault, CivilianDefence, SearchAndDestroy, Capture, Rescue};
	public MissionType missionType;
	public int dayCalled;
	public int dayDue;
	public bool dueDateKnown = false;
}


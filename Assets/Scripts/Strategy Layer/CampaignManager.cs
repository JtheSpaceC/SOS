using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CampaignManager : MonoBehaviour {

	public static CampaignManager instance;

	public GameObject sectorsContainer;
	public Sector[] allSectors;
	[HideInInspector] public Sector activeSector;

	public List<Mission> availableMissions;

	public GameObject map;
	public GameObject mapInfoPanel;
	public Text mapInfoHeaderText;
	public Text mapInfoBodyText;
	public Button reconButton;
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

		if(!map.activeSelf)
		{
			map.SetActive(true);
			mapInfoPanel.SetActive(false);
		}
		else
		{
			map.SetActive(false);
			mapInfoPanel.SetActive(true);
		}
	}

	public void PopulateMapInfoPanelForSector()
	{
		reconButton.gameObject.SetActive(true);
		reconButton.interactable = true;

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

		mapInfoHeaderText.text = activeSector.name;
		mapInfoBodyText.text = infoString;
	}

	public void PopulateMapInfoPanelForSatellite(Transform satellite)
	{
		reconButton.gameObject.SetActive(false);

		infoString = "This stealthed spy drone sits in empty space near " + satellite.transform.parent.parent.name + " passively monitoring " +
			"the entire sector.\n" +
			"Obstruction from planetary bodies and from asteroids, as well as the need not to use active sensors, prevents this drone " +
			"from gathering detailed information about the sub-sectors that it monitors.\n" +
			"It will, however, detect any large ships entering or leaving the Sector at large. " +
			"Apart from providing valuable intelligence, the presence of these drones is reassuring for the civilian population.";

		mapInfoHeaderText.text = "Spy Satellite";
		mapInfoBodyText.text = infoString;
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
					reconButton.interactable = false;
			}
		}
	}

	public void RequestScheduleReconMission()
	{
		//TODO: CheckForCraftAvailabilityEtc()

		ScheduleReconMission();
		reconButton.interactable = false;
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
}


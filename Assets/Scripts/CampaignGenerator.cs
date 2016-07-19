using UnityEngine;
using System.Collections;

public class CampaignGenerator : MonoBehaviour {


	public void GenerateNewTour()
	{
		TourOfDuty tod = new TourOfDuty();
	}
}

public class TourOfDuty
{
	public enum TourType {DefendStation, RecaptureStation, SweepAndClear, Neutralise};
	public TourType tourType;

	Character nemesis;
	public enum nemesisObjective {DestroyPMCCarrier, CaptureBase, Escape, DevelopSuperweapon};

	public enum StrongholdType {AsteroidBase, Warship, BaseAndWarship};
	public StrongholdType strongholdType;

	public int forceStrength = 500;
	public int techLevel = 1;
	public int skillLevel = 2;
}

using UnityEngine;
using System.Collections;

public class Sector: HoloMapObject{


	public enum MySectorGroup {Capital, Alpha, Beta, Gamma, Delta};
	public MySectorGroup mySectorGroup;

	public enum BaseType {None, Civilian, Enemy};
	public BaseType baseType;

	public bool enemyFleetPresent = false;
	public int dayScouted = -1;


	void Awake()
	{
		AwakeBaseClass();

		if(baseType == BaseType.Civilian)
			myRenderer.color = Color.blue;
		else if(baseType == BaseType.Enemy || enemyFleetPresent)
			myRenderer.color = Color.red;
	}

	public string OutPutMyStats()
	{
		return "";	
	}

	public void OnMouseDown()
	{
		CampaignManager.instance.activeSector = this;
	}
}
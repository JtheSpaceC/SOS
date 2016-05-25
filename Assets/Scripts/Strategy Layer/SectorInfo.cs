using UnityEngine;
using System.Collections;

public class SectorInfo : MonoBehaviour {

	public Sector[] allSectors;
	
	void LoadInfo()
	{
		for(int i = 0; i < allSectors.Length; i++)
		{
			
		}
	}
}

public class Sector{

	public bool explored = false;
	public bool enemyFleetPresent = false;
	public bool enemyBasePresent = false;
	public bool civilianBasePresent = false;

	public string OutPutMyStats()
	{
		return "";	
	}
}
using UnityEngine;
using System.Collections;

public class CampaignManager : MonoBehaviour {

	public GameObject sectorsContainer;
	 public Sector[] allSectors;
	
	void Awake()
	{
		allSectors = sectorsContainer.GetComponentsInChildren<Sector>();
	}
}


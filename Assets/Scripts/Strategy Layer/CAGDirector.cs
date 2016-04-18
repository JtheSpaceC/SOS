using UnityEngine;
using System.Collections;

public class CAGDirector : MonoBehaviour {

	public static CAGDirector instance;

	public int gameDay = 1;
	[HideInInspector] public bool wearingClothes = false;


	public void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were two CAGDirectors. Destroying one.");
			Destroy(gameObject);
		}
	}


	public void ActivateNextRoom()
	{
		
	}
}

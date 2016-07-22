using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionSetup : MonoBehaviour {

	public enum InsertionType {AlreadyPresent, WarpIn, LeaveHangar}
	public InsertionType insertionType;

	public GameObject playerCraft;
	public List<Transform> warpingCraft; //if we're warping in, we'll have to attach to this. Player's group first, any friendlies after
	public Transform hangarCraft; //if flying out of a hangar, we'll need this
	public List<GameObject> playerSquad;
	public List<GameObject> enemyCraft;
	public List<GameObject> civilianCraft;

	public GameObject backgroundForArea;


	public void ValidateChoices()
	{
		//make sure if we're leaving hangar, there's a hangar to leave, etc
	}
}

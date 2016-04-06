using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquadronLeader : MonoBehaviour {

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;

	bool isPlayerSquad;

	[HideInInspector] public List<GameObject> allWingmen;
	[Tooltip("Put wingmen here to handle the group setup. NB: put in order 0 - 11")]
	public List<GameObject> activeWingmen;
	public List<GameObject> deadWingmen;
	public List<GameObject> EVWingmen;
	public List<GameObject> capturedWingmen;
	
	public GameObject mate01;
	public GameObject mate02;
	public GameObject mate03;
	public GameObject mate04;
	public GameObject mate05;
	public GameObject mate06;
	public GameObject mate07;
	public GameObject mate08;
	public GameObject mate09;
	public GameObject mate10;
	public GameObject mate11;
	public GameObject mate12;

	[HideInInspector] public Animator mate01RadarAnimator;
	[HideInInspector] public Animator mate02RadarAnimator;
	[HideInInspector] public Animator mate03RadarAnimator;
	[HideInInspector] public Animator mate04RadarAnimator;
	[HideInInspector] public Animator mate05RadarAnimator;
	[HideInInspector] public Animator mate06RadarAnimator;
	[HideInInspector] public Animator mate07RadarAnimator;
	[HideInInspector] public Animator mate08RadarAnimator;
	[HideInInspector] public Animator mate09RadarAnimator;
	[HideInInspector] public Animator mate10RadarAnimator;
	[HideInInspector] public Animator mate11RadarAnimator;
	[HideInInspector] public Animator mate12RadarAnimator;

	public GameObject wingmanPosLeft;
	public GameObject wingmanPosRight;
	[HideInInspector] public GameObject SecondFlightPos;
	[HideInInspector] public GameObject ThirdFlightPos;
	[HideInInspector] public GameObject FourthFlightPos;
	
	public enum Orders {FormUp, EngageAtWill, CoverMe, Disengage, Extraction};
	public Orders firstFlightOrders;
	//TODO: orders for other flights

	public float toolTipDuration = 4;

	
	void Start()
	{
		//this check fixes wingmen errors that prefabs are causing across different test scenes
		List<GameObject> tempWingmenList = new List<GameObject>();
		for(int i = 0; i < activeWingmen.Count; i++)
		{
			if(activeWingmen[i] != null)
				if(activeWingmen[i].activeSelf)
					tempWingmenList.Add(activeWingmen[i]);
		}
		activeWingmen.Clear ();
		activeWingmen = tempWingmenList;
		//finished check

		foreach (GameObject go in activeWingmen) {
			allWingmen.Add (go);
		}
		foreach (GameObject go in deadWingmen) {
			allWingmen.Add (go);
		}
		foreach (GameObject go in EVWingmen) {
			allWingmen.Add (go);
		}
		foreach (GameObject go in capturedWingmen) {
			allWingmen.Add (go);
		}
		
		if(transform.parent.parent.tag == "PlayerFighter")
		{
			isPlayerSquad = true;
			GameObject.Find("RADIO layout group").GetComponent<RadioCommands>().enabled = true;
		}
		else
			GetComponentInParent<AIFighter>().flightLeader = transform.parent.parent.gameObject;

		GenerateWingmanFormationPositions ();
		ReAssignWingmen ();

		//Invoke(firstFlightSquadOrders.ToString(), 0.01f);
	}

	public void ReAssignWingmen()
	{
		try{
		mate01 = transform.parent.parent.gameObject;
		if(transform.parent.parent.tag != "PlayerFighter")
		{
			mate01.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.one;
		}

		mate02 = activeWingmen [0];
		mate02.GetComponent<AIFighter>().flightLeader = mate01;
		mate02.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.two;

		mate03 = activeWingmen [1];
		mate03.GetComponent<AIFighter>().flightLeader = mate01;
		mate03.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.three;

		mate04 = activeWingmen [2];
		mate04.GetComponent<AIFighter>().flightLeader = mate01;
		mate05 = activeWingmen [3];
		mate05.GetComponent<AIFighter>().flightLeader = mate01;
		mate06 = activeWingmen [4];
		mate06.GetComponent<AIFighter>().flightLeader = mate01;
		mate07 = activeWingmen [5];
		mate07.GetComponent<AIFighter>().flightLeader = mate01;
		mate08 = activeWingmen [6];
		mate08.GetComponent<AIFighter>().flightLeader = mate01;
		mate09 = activeWingmen [7];
		mate09.GetComponent<AIFighter>().flightLeader = mate01;
		mate10 = activeWingmen [8];
		mate10.GetComponent<AIFighter>().flightLeader = mate01;
		mate11 = activeWingmen [9];
		mate11.GetComponent<AIFighter>().flightLeader = mate01;
		mate12 = activeWingmen [10];
		mate12.GetComponent<AIFighter>().flightLeader = mate01;
		}catch{};

		try{
		mate01RadarAnimator = mate01.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate02RadarAnimator = mate02.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate03RadarAnimator = mate03.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate04RadarAnimator = mate04.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate05RadarAnimator = mate05.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate06RadarAnimator = mate06.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate07RadarAnimator = mate07.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate08RadarAnimator = mate08.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate09RadarAnimator = mate09.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate10RadarAnimator = mate10.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate11RadarAnimator = mate11.transform.FindChild("RadarSig").GetComponent<Animator> ();
		mate12RadarAnimator = mate12.transform.FindChild("RadarSig").GetComponent<Animator> ();
		}catch{
		}

		for(int i = 0; i < activeWingmen.Count; i++)
		{
			if(activeWingmen[i] != null)
			{
				AIFighter aiScript = activeWingmen[i].GetComponent<AIFighter>();
				aiScript.SetSquadReferences();
				if(isPlayerSquad)
					aiScript.myCharacterAvatarScript.SetUpAvatar(aiScript.mySquadUnitNumber);
			}
		}
	}
	public void GenerateWingmanFormationPositions()
	{
		wingmanPosLeft = new GameObject();
		wingmanPosLeft.transform.parent = this.transform;
		wingmanPosLeft.transform.position = this.transform.position + (transform.up * -1) + (transform.right * -2.5f);
		wingmanPosLeft.tag = "FormationPosition";
		wingmanPosLeft.name = "Wingman Pos Left";

		wingmanPosRight = new GameObject();
		wingmanPosRight.transform.parent = this.transform;
		wingmanPosRight.transform.position = this.transform.position + (transform.up * -1) + (transform.right * 2.5f);
		wingmanPosRight.tag = "FormationPosition";
		wingmanPosRight.name = "Wingman Pos Right";

		SecondFlightPos = new GameObject();
		SecondFlightPos.transform.parent = this.transform;
		SecondFlightPos.transform.position = this.transform.position + (transform.up * -3) + (transform.right * -7.5f);
		SecondFlightPos.tag = "FormationPosition";
		SecondFlightPos.name = "2nd Flight Pos";

		ThirdFlightPos = new GameObject();
		ThirdFlightPos.transform.parent = this.transform;
		ThirdFlightPos.transform.position = this.transform.position + (transform.up * -3) + (transform.right * 7.5f);
		ThirdFlightPos.tag = "FormationPosition";
		ThirdFlightPos.name = "3rd Flight Pos";

		FourthFlightPos = new GameObject();
		FourthFlightPos.transform.parent = this.transform;
		FourthFlightPos.transform.position = this.transform.position + (transform.up * -6);
		FourthFlightPos.tag = "FormationPosition";
		FourthFlightPos.name = "4th Flight Pos";
	}

	public void CheckActiveMateStatus()
	{
		if(mate02 == null || !mate02.activeSelf || mate02.GetComponent<HealthFighter>().dead)
			mate02 = null;
		if( mate03 == null || !mate03.activeSelf || mate03.GetComponent<HealthFighter>().dead)
			mate03 = null;
		if( mate04 == null || !mate04.activeSelf || mate04.GetComponent<HealthFighter>().dead)
			mate04 = null;
		if( mate05 == null || !mate05.activeSelf || mate05.GetComponent<HealthFighter>().dead)
			mate05 = null;
		if( mate06 == null || !mate06.activeSelf || mate06.GetComponent<HealthFighter>().dead)
			mate06 = null;
		if( mate07 == null || !mate07.activeSelf || mate07.GetComponent<HealthFighter>().dead)
			mate07 = null;
		if( mate08 == null || !mate08.activeSelf || mate08.GetComponent<HealthFighter>().dead)
			mate08 = null;
		if( mate09 == null || !mate09.activeSelf || mate09.GetComponent<HealthFighter>().dead)
			mate09 = null;
		if( mate10 == null || !mate10.activeSelf || mate10.GetComponent<HealthFighter>().dead)
			mate10 = null;
		if( mate11 == null || !mate11.activeSelf || mate11.GetComponent<HealthFighter>().dead)
			mate11 = null;
		if( mate12 == null || !mate12.activeSelf || mate12.GetComponent<HealthFighter>().dead)
			mate12 = null;
	}

	public void FormUp()
	{
		CheckActiveMateStatus ();

		firstFlightOrders = Orders.FormUp;

		if (mate02 != null) {
			mate02.GetComponent<AIFighter> ().ChangeToNewState (mate02.GetComponent<AIFighter> ().formUpStates, new float[]{1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate02RadarAnimator.SetTrigger("Flashing");
				mate02.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		if (mate03 != null) {
			mate03.GetComponent<AIFighter> ().ChangeToNewState (mate03.GetComponent<AIFighter> ().formUpStates, new float[]{1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate03RadarAnimator.SetTrigger("Flashing");
				mate03.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		string[] acknowledgments = new string[] {"Roger that. Forming up!"};
		AcknowledgeOrderIfWingmenAlive (acknowledgments);
	}

	public void EngageAtWill()
	{
		CheckActiveMateStatus ();

		if (firstFlightOrders == Orders.Extraction)
			return;

		firstFlightOrders = Orders.EngageAtWill;

		if(mate02 != null)
		{
			mate02.GetComponent<AIFighter> ().ChangeToNewState (mate02.GetComponent<AIFighter> ().normalStates, new float[]{1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate02RadarAnimator.SetTrigger("Flashing");
				mate02.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		if(mate03 != null)
		{
			mate03.GetComponent<AIFighter> ().ChangeToNewState (mate03.GetComponent<AIFighter> ().normalStates, new float[]{1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate03RadarAnimator.SetTrigger("Flashing");
				mate03.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		
		string[] acknowledgments = new string[] {"Affirmative. Searching for targets!",	"Affirmative. Going hunting!"};
		AcknowledgeOrderIfWingmenAlive (acknowledgments);
	}

	public void CoverMe()
	{
		CheckActiveMateStatus ();

		firstFlightOrders = Orders.CoverMe;

		if(mate02)
		{
			mate02.GetComponent<AIFighter> ().ChangeToNewState (mate02.GetComponent<AIFighter> ().coverMeStates, new float[]{1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate02RadarAnimator.SetTrigger("Flashing");
				mate02.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		if (mate03) 
		{
			mate03.GetComponent<AIFighter> ().ChangeToNewState (mate03.GetComponent<AIFighter> ().coverMeStates, new float[]{1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate03RadarAnimator.SetTrigger("Flashing");
				mate03.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}

		string[] acknowledgments = new string[] {"Acknowledged. Got your back!"};
		AcknowledgeOrderIfWingmenAlive (acknowledgments);
	}

	public void Disengage()
	{
		CheckActiveMateStatus ();

		firstFlightOrders = Orders.Disengage;
		
		if(mate02)
		{
			mate02.GetComponent<AIFighter> ().ChangeToNewState (mate02.GetComponent<AIFighter> ().retreatStates, new float[]{1, 1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate02RadarAnimator.SetTrigger("Flashing");
				mate02.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		if(mate03)
		{
			mate03.GetComponent<AIFighter> ().ChangeToNewState (mate03.GetComponent<AIFighter> ().retreatStates, new float[]{1, 1});
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate03RadarAnimator.SetTrigger("Flashing");
				mate03.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}

		string[] acknowledgments = new string[] {"Roger. Breaking off!"};
		AcknowledgeOrderIfWingmenAlive (acknowledgments);
	}


	public void ReportIn()
	{
		CheckActiveMateStatus ();		

		if(mate02)
		{
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate02RadarAnimator.SetTrigger("Flashing");
				mate02.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
		if(mate03)
		{
			if(transform.parent.parent.gameObject.layer == LayerMask.NameToLayer("PMCFighters"))
			{
				mate03RadarAnimator.SetTrigger("Flashing");
				mate03.SendMessage("HUDPointerOn", toolTipDuration);
			}
		}
	}


	void AcknowledgeOrderIfWingmenAlive(string[] possibleReplies)
	{
		if(whichSide == WhichSide.Ally)
		{
			if(mate01 == null || !mate01.activeSelf || mate01.GetComponent<HealthFighter>().dead)
				return;
			
			if ((!mate02 || mate02.GetComponent<HealthFighter>().dead) && (!mate03 || mate03.GetComponent<HealthFighter>().dead))
			{
				Subtitles.instance.PostSubtitle (new string[]{"Wingmates are down. You're on your own, " +transform.parent.parent.name+ "."});
			}
			else
			{
				Subtitles.instance.PostSubtitle (possibleReplies);
				foreach(GameObject wingman in activeWingmen)
				{
					wingman.GetComponent<AIFighter>().myCharacterAvatarScript.StartCoroutine("Speaking");
				}
				_battleEventManager.instance.CallWingmanAcknowledgeOrder();
			}
		}
	}

	void OrderReceivedFeedback()
	{

	}

}//Mono

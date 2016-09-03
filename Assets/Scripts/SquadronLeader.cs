using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquadronLeader : MonoBehaviour {

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;

	[HideInInspector] public string squadName;

	bool isPlayerSquad;

	[HideInInspector] public List<GameObject> allWingmen;
	[Tooltip("Put wingmen here to handle the group setup. NB: put in order 0 - 11")]
	public List<GameObject> activeWingmen;
	public List<GameObject> deadWingmen;
	public List<GameObject> retreatingWingmen;
	public List<GameObject> retrievedWingmen;
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

	public GameObject wingmanPos2;
	public GameObject wingmanPos3;
	public GameObject wingmanPos4;
	public GameObject wingmanPos5;
	public GameObject wingmanPos6;
	public GameObject wingmanPos7;
	public GameObject wingmanPos8;
	public GameObject wingmanPos9;
	public GameObject wingmanPos10;
	public GameObject wingmanPos11;
	public GameObject wingmanPos12;
	[HideInInspector] public GameObject SecondFlightPos;
	[HideInInspector] public GameObject ThirdFlightPos;
	[HideInInspector] public GameObject FourthFlightPos;
	
	public enum Orders {FormUp, EngageAtWill, CoverMe, Disengage, Extraction};
	public Orders firstFlightOrders;
	//TODO: orders for other flights

	public float toolTipDuration = 4;

	
	void Start()
	{
		SetUp();
	}

	public void SetUp()
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

		GenerateWingmanFormationPositions ();
		ReassignWingmen ();

		AddStarsToCallsign();

		//Invoke(firstFlightSquadOrders.ToString(), 0.01f);
	}


	public void ReassignWingmen()
	{
		if(transform.parent.parent.tag == "PlayerFighter")
		{
			isPlayerSquad = true;
			GameObject.Find("RADIO layout group").GetComponent<RadioCommands>().enabled = true;
		}
		else
			transform.parent.parent.GetComponent<AIFighter>().flightLeader = transform.parent.parent.gameObject;
		
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
			mate04.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.four;

			mate05 = activeWingmen [3];
			mate05.GetComponent<AIFighter>().flightLeader = mate01;
			mate05.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.five;

			mate06 = activeWingmen [4];
			mate06.GetComponent<AIFighter>().flightLeader = mate01;
			mate06.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.six;

			mate07 = activeWingmen [5];
			mate07.GetComponent<AIFighter>().flightLeader = mate01;
			mate07.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.seven;

			mate08 = activeWingmen [6];
			mate08.GetComponent<AIFighter>().flightLeader = mate01;
			mate08.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.eight;

			mate09 = activeWingmen [7];
			mate09.GetComponent<AIFighter>().flightLeader = mate01;
			mate09.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.nine;

			mate10 = activeWingmen [8];
			mate10.GetComponent<AIFighter>().flightLeader = mate01;
			mate10.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.ten;

			mate11 = activeWingmen [9];
			mate11.GetComponent<AIFighter>().flightLeader = mate01;
			mate11.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.eleven;

			mate12 = activeWingmen [10];
			mate12.GetComponent<AIFighter>().flightLeader = mate01;
			mate12.GetComponent<AIFighter>().squadronMembership = AIFighter.SquadronMembership.twelve;

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

		mate01.GetComponent<AIFighter>().SetSquadReferences();

		for(int i = 0; i < activeWingmen.Count; i++)
		{
			if(activeWingmen[i] != null)
			{
				AIFighter aiScript = activeWingmen[i].GetComponent<AIFighter>();
				aiScript.SetSquadReferences();
				if(isPlayerSquad)
				{
					aiScript.myCharacterAvatarScript.SetUpAvatar(aiScript.mySquadUnitNumber);
				}
			}
		}
	}
	public void GenerateWingmanFormationPositions()
	{
		//there is no Pos 1

		wingmanPos2 = new GameObject();
		wingmanPos2.transform.parent = this.transform;
		wingmanPos2.transform.position = this.transform.position + (transform.up * -1) + (transform.right * -2.5f);
		wingmanPos2.tag = "FormationPosition";
		wingmanPos2.name = "Wingman Pos 2";

		wingmanPos3 = new GameObject();
		wingmanPos3.transform.parent = this.transform;
		wingmanPos3.transform.position = this.transform.position + (transform.up * -1) + (transform.right * 2.5f);
		wingmanPos3.tag = "FormationPosition";
		wingmanPos3.name = "Wingman Pos 3";

		wingmanPos4 = new GameObject();
		wingmanPos4.transform.parent = this.transform;
		wingmanPos4.transform.position = this.transform.position + (transform.up * -2) + (transform.right * -5f);
		wingmanPos4.tag = "FormationPosition";
		wingmanPos4.name = "Wingman Pos 4";

		wingmanPos5 = new GameObject();
		wingmanPos5.transform.parent = this.transform;
		wingmanPos5.transform.position = this.transform.position + (transform.up * -2) + (transform.right * 5f);
		wingmanPos5.tag = "FormationPosition";
		wingmanPos5.name = "Wingman Pos 5";

		wingmanPos6 = new GameObject();
		wingmanPos6.transform.parent = this.transform;
		wingmanPos6.transform.position = this.transform.position + (transform.up * -2.5f) + (transform.right * 0);
		wingmanPos6.tag = "FormationPosition";
		wingmanPos6.name = "Wingman Pos 6";

		wingmanPos7 = new GameObject();
		wingmanPos7.transform.parent = this.transform;
		wingmanPos7.transform.position = this.transform.position + (transform.up * -3.5f) + (transform.right * -3.75f);
		wingmanPos7.tag = "FormationPosition";
		wingmanPos7.name = "Wingman Pos 7";

		wingmanPos8 = new GameObject();
		wingmanPos8.transform.parent = this.transform;
		wingmanPos8.transform.position = this.transform.position + (transform.up * -3.5f) + (transform.right * 3.75f);
		wingmanPos8.tag = "FormationPosition";
		wingmanPos8.name = "Wingman Pos 8";

		wingmanPos9 = new GameObject();
		wingmanPos9.transform.parent = this.transform;
		wingmanPos9.transform.position = this.transform.position + (transform.up * -4) + (transform.right * -6.25f);
		wingmanPos9.tag = "FormationPosition";
		wingmanPos9.name = "Wingman Pos 9";

		wingmanPos10 = new GameObject();
		wingmanPos10.transform.parent = this.transform;
		wingmanPos10.transform.position = this.transform.position + (transform.up * -4) + (transform.right * 6.25f);
		wingmanPos10.tag = "FormationPosition";
		wingmanPos10.name = "Wingman Pos 10";

		wingmanPos11 = new GameObject();
		wingmanPos11.transform.parent = this.transform;
		wingmanPos11.transform.position = this.transform.position + (transform.up * -5) + (transform.right * -2.5f);
		wingmanPos11.tag = "FormationPosition";
		wingmanPos11.name = "Wingman Pos 11";

		wingmanPos12 = new GameObject();
		wingmanPos12.transform.parent = this.transform;
		wingmanPos12.transform.position = this.transform.position + (transform.up * -5) + (transform.right * 2.5f);
		wingmanPos12.tag = "FormationPosition";
		wingmanPos12.name = "Wingman Pos 12";

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
		if( mate02 == null || !mate02.activeSelf || mate02.GetComponent<HealthFighter>().dead)
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
		if(whichSide == WhichSide.Ally && isPlayerSquad)
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
		//TODO: Can't remember..
	}


	public void AssignNewLeader(bool leaderIsDead)
	{
		//if leader is still alive but everyone else is dead
		if(!leaderIsDead && activeWingmen.Count == 0)
		{
			FindMeANewLeader(mate01);
			return;
		}
		else if(!leaderIsDead && activeWingmen.Count >= 1) //leader alive with 1 or more wingmen left
			return; //don't make changes

		//if there are no other available wingmen, do nothing. Leader was the last survivor and is now dead/retreating
		if(leaderIsDead && activeWingmen.Count == 0)
		{
			mate01.GetComponent<AIFighter>().myCommander.mySquadrons.Remove(this);
			return;
		}
		//if only one ship left, first check if the Commander would rather assign you to a different squad
		else if(leaderIsDead && activeWingmen.Count == 1)
		{
			FindMeANewLeader(activeWingmen[0]);
		}
		else if(leaderIsDead && activeWingmen.Count > 1)
		{
			//otherwise assign a new leader, the first in the active wingmen
			MakeExistingWingmanIntoThisSquadsLeader();
		}
	}

	void FindMeANewLeader(GameObject who)
	{
		SquadronLeader potentialNewLeader = who.GetComponent<AIFighter>().myCommander.JoinUnderstrengthSquad(this);
		if(potentialNewLeader != null)
		{
			mate01.GetComponent<AIFighter>().myCommander.mySquadrons.Remove(this);

			potentialNewLeader.activeWingmen.Add(who);
			potentialNewLeader.allWingmen.Add(who);
			potentialNewLeader.ReassignWingmen();
			potentialNewLeader.AddStarsToCallsign();
			who.GetComponent<AIFighter>().ChangeToNewState(new AIFighter.StateMachine[]{AIFighter.StateMachine.Covering}, new float[]{1});

			Debug.Log(who.name + " was added to " + potentialNewLeader.squadName);
			return;
		}
		else if(who != mate01)
			MakeExistingWingmanIntoThisSquadsLeader();
	}
	void MakeExistingWingmanIntoThisSquadsLeader()
	{
		GameObject newLeader = activeWingmen[0];
		activeWingmen.Remove(newLeader);
		this.transform.SetParent(newLeader.transform.FindChild ("Abilities"));
		this.transform.localPosition = Vector3.zero;
		ReassignWingmen();
		AddStarsToCallsign();
		newLeader.GetComponent<AIFighter>().ChangeToNewState(newLeader.GetComponent<AIFighter>().normalStates, new float[]{1});

		Debug.Log(newLeader.name + " became new squad leader of " + squadName + " Squadron");
	}

	public void AddStarsToCallsign()
	{
		if(transform.parent.GetComponentInParent<AIFighter>() && whichSide == WhichSide.Ally)
		{
			string name = GetComponentInParent<AIFighter>().nameHUDText.text;
			if(name.ToCharArray()[0] != '*')
			{
				GetComponentInParent<AIFighter>().nameHUDText.text = "* " + GetComponentInParent<AIFighter>().nameHUDText.text + " *";
			}
		}
	}

}//Mono

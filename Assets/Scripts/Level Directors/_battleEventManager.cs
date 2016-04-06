using UnityEngine;
using System.Collections;

public class _battleEventManager : MonoBehaviour {	

	public static _battleEventManager instance;

	public delegate void FirstWingmanClash();
	public static event FirstWingmanClash wingmanFirstClash;
	[HideInInspector] public bool firstClashCalled = false;
	[HideInInspector] public bool playerHasOneHitKills = false;

	public delegate void WingmanGotKill();
	public static event WingmanGotKill wingmanGotAKill;

	public delegate void WingmanInTrouble();
	public static event WingmanInTrouble wingmanInTrouble;

	public delegate void WingmanBack();
	public static event WingmanBack wingmanBack;

	public delegate void WingmanAcknowledgeOrder();
	public static event WingmanAcknowledgeOrder wingmanAcknowledgeOrder;
	
	public delegate void WingmanDied();
	public static event WingmanDied wingmanDied;

	public delegate void PickupOnTheWay();
	public static event PickupOnTheWay pickupOnTheWay;

	public delegate void PickupNegative();
	public static event PickupNegative pickupNegative;

	public delegate void PlayerLeaving();
	public static event PlayerLeaving playerLeaving;


	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		}
		else
		{
			Debug.LogError("There were two BattleEventManagers. Deleting one");
			Destroy(this.gameObject);
		}
	}

	public void CallFirstWingmanClash()
	{
		if (!firstClashCalled) {
			wingmanFirstClash ();
			firstClashCalled = true;
		}
	}
	public void CallWingmanGotAKill()
	{
		wingmanGotAKill ();
	}
	public void CallWingmanInTrouble()
	{
		wingmanInTrouble ();
	}
	public void CallWingmanBack()
	{
		wingmanBack ();
	}
	public void CallWingmanAcknowledgeOrder()
	{
		wingmanAcknowledgeOrder ();
	}
	public void CallWingmanDied()
	{
		wingmanDied ();
	}
	public void CallPickupOnTheWay()
	{
		pickupOnTheWay ();
	}
	public void CallPickupNegative()
	{
		pickupNegative ();
	}
	public void CallPlayerLeaving()
	{
		playerLeaving ();
	}


}//Mono

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PMCMisisonSupports : MonoBehaviour {

	public static PMCMisisonSupports instance;
	AICommander pmcCommander;

	public List<string> availableSupports;
	public List<string> backup;
	public int availableFighterTransports = 3;
	public int availableRetrievalShuttles = 3;

	public bool carrierAvailable = false;
	public bool carrierDeployed = false;

	[Header("Prefabs")]
	public GameObject fighterTransportPrefab;
	public GameObject retrievalShuttlePrefab;
	public float retrievalShuttleSpawnRadius = 20;

	Vector2 insertionPoint;
	Vector2 spawnPos;

	[HideInInspector] public GameObject retrievalShuttle;
	GameObject playerEVA;


	void Awake () 
	{
		if(instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		pmcCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("PMC Commander").GetComponent<AICommander> ();
	}
		
	
	public void CallSupportMenu()
	{
		if(availableSupports.Count == 0)
		{
			Subtitles.instance.PostHint(new string[]{"There are no SUPPORTS available for this mission."});
			_battleEventManager.instance.CallPickupNegative();
		}
	}

	public void SendBackup()
	{
		if(backup.Count == 0)
		{
			Subtitles.instance.PostHint(new string[]{"BACKUP is not available. You're on your own!"});
			_battleEventManager.instance.CallPickupNegative();
		}
	}

	public void Extraction(GameObject theCaller)
	{
		if(availableFighterTransports == 0)
		{
			Subtitles.instance.PostHint(new string[]{"There are no transports available."});
			_battleEventManager.instance.CallPickupNegative();
			return;
		}

		insertionPoint = theCaller.transform.position;


		Collider2D[] worldEvents = Physics2D.OverlapCircleAll (theCaller.transform.position, 5f, LayerMask.GetMask("WorldEvents"));

		foreach(Collider2D wEvent in worldEvents)
		{
			if(wEvent.tag == "AsteroidField")
			{
				/*Subtitles.instance.PostHint(new string[] {"Can't send transports into asteroid fields. Get clear to call for transports."});
				_battleEventManager.instance.CallPickupNegative();
				return;*/

				insertionPoint = wEvent.transform.position + ((theCaller.transform.position - wEvent.transform.position).normalized 
					* wEvent.GetComponent<CircleCollider2D>().radius * wEvent.transform.localScale.y * 1.05f);
			}
		}

		if(FindObjectOfType<AsteroidField>())
		{
			spawnPos = (insertionPoint - (Vector2)FindObjectOfType<AsteroidField>().transform.position).normalized * 1000;
		}
		else
		{
			spawnPos = (insertionPoint - (Vector2)pmcCommander.transform.position).normalized * 1000;
		}

		Subtitles.instance.PostSubtitle(new string[]{"Pickup is on its way! ETA 10 seconds. Stand by!"});
		Tools.instance.CreateWaypoint (Tools.WaypointTypes.Extraction, insertionPoint);
		_battleEventManager.instance.CallPickupOnTheWay();

		GameObject transport = Instantiate(fighterTransportPrefab, spawnPos, Quaternion.identity) as GameObject;
		//TODO: naming system
		transport.name = "Transport " + availableFighterTransports;
		availableFighterTransports --;
		transport.GetComponent<AITransport>().ChangeToNewState(AITransport.StateMachine.WarpIn);
		transport.GetComponent<AITransport>().theCaller = theCaller;
		transport.GetComponent<AITransport>().insertionPoint = insertionPoint;
	}

	public void AutoRetrievePlayer()
	{
		if(availableRetrievalShuttles > 0)
		{
			Subtitles.instance.PostSubtitle(new string[]{"Pilot is EVA! Shuttle moving in to retrieve!"});
			playerEVA = GameObject.FindGameObjectWithTag("PlayerEVA");

			retrievalShuttle = Instantiate(retrievalShuttlePrefab, (Vector2)playerEVA.transform.position + 
				(Random.insideUnitCircle.normalized * retrievalShuttleSpawnRadius), Quaternion.identity) as GameObject;
			retrievalShuttle.name = "RESCUE 1";

			AIAssaultShuttle retrievalAI = retrievalShuttle.GetComponent<AIAssaultShuttle>();
			retrievalAI.dockTarget = playerEVA.transform;
			retrievalAI.ChangeToNewState(AIAssaultShuttle.StateMachine.Docking);
		}
	}
}//Mono

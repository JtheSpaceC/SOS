﻿using UnityEngine;

public class AIAssaultShuttle : SupportShipFunctions {

	public enum StateMachine {Holding, Retrieving, Docking, Docked, Moving, WarpIn, WarpOut};
	[Header("States")]

	public StateMachine currentState;
	public StateMachine previousState;

	public Transform dockTarget;
	public Vector2 targetPosition;

	float dockTimer;
	public float dockTime = 1.5f;

	Rigidbody2D targetRB;
	Vector2 dockingLookPos;
	Vector3 camOffset;
	float startTime;

	[HideInInspector] bool pickingUpPlayer = false;


	void Start()
	{
		healthScript = GetComponent<HealthTransport> ();
		engineScript = GetComponent<EnginesFighter> ();
		warpDrive = GetComponentInChildren<WarpDrive>();
		myRigidbody = GetComponent<Rigidbody2D>();
		SetUpSideInfo();

		if(myCommander == Tools.instance.pmcCommander)
			gameObject.layer = LayerMask.NameToLayer("PMCTransports");
		else if(myCommander == Tools.instance.pirateCommander)
			gameObject.layer = LayerMask.NameToLayer("EnemyTransports");
		
		myCommander.myAssaultShuttles.Add (this.gameObject);

		//turn turret placeholders into the list of turrets, then spawn real turrets from that
		Transform weaponsParent = transform.FindChild("Weapons");
		SpriteRenderer[] weaponChildren = weaponsParent.GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer child in weaponChildren)
		{
			if(child.name.StartsWith("tur"))
			{
				myTurrets.Add(child.gameObject);
			}
		}
		SpawnTurret(myTurrets, howWellDefended);

		WeaponsTurret[] turrets = weaponsParent.GetComponentsInChildren<WeaponsTurret>();
		foreach(WeaponsTurret tur in turrets)
		{
			myTurrets.Add(tur.gameObject);
		}

		ChangeTurretsSide(whichSide);
		foreach(GameObject turret in myTurrets)
		{
			if(!myCommander.myTurrets.Contains(turret))
			{
				myCommander.myTurrets.Add(turret);
			}
			if(enemyCommander.myTurrets.Contains(turret))
			{
				enemyCommander.myTurrets.Remove(turret);
			}
		}

		literalSpawnPoint = (Vector2)transform.position; //TODO: Replace this when there's a WarpIn function
	}


	void Update()
	{
		if(currentState == StateMachine.Holding)
		{
			HoldPosition();
		}
		else if(currentState == StateMachine.Moving)
		{
			if(!completedState)
				MoveToPosition(targetPosition);
			else
				ChangeToNewState(StateMachine.Holding);
		}
		else if(currentState == StateMachine.Docking)
		{
			Docking();
		}
		else if(currentState == StateMachine.WarpOut)
		{
			WarpOut();
		}
	}

	public void ChangeToNewState(StateMachine newState)
	{
		previousState = currentState;

		warpDrive.warpBubble.enabled = false;
		dockingLookPos = Vector2.zero;

		if(newState != StateMachine.Docked)
			transform.SetParent(null);

		currentState = newState;
		switchingStates = true;
		completedState = false;
	}

	void Docking()
	{
		if(switchingStates)
		{
			switchingStates = false;
			if(dockTarget.parent != null)
				targetRB = dockTarget.transform.parent.transform.parent.transform.parent.GetComponent<Rigidbody2D>();
			else
			{
				targetRB = dockTarget.GetComponent<Rigidbody2D>();
				dockingLookPos = (Vector2)(dockTarget.position - transform.position).normalized;
			}
		}
		targetPosition = dockTarget.position;

		if(dockingLookPos != Vector2.zero)
			engineScript.LookAtTarget((Vector2)transform.position + dockingLookPos);
		else
			engineScript.LookAtTarget(targetPosition + (Vector2)dockTarget.up * 5);
		
		MatchTargetVelocity((Vector2)dockTarget.position, targetRB, Vector2.zero);

		if(Vector2.Distance(transform.position, targetPosition) < 0.2f)
		{
			dockTimer += Time.deltaTime;
			if(dockTimer >= dockTime)
			{
				Subtitles.instance.PostSubtitle(new string[] {name + " has docked with " + dockTarget.transform.root.name});

				if(dockTarget.tag == "PlayerEVA")
				{
					pickingUpPlayer = true;
					_battleEventManager.instance.CallPlayerRescued();
				}
				
				if(pickingUpPlayer || dockTarget.tag == "PilotEVA")
				{
					dockTarget.SetParent(transform);
					dockTarget.gameObject.SetActive(false);

					if(!pickingUpPlayer)
						ChangeToNewState(StateMachine.Docked);					
				}
				else
				{
					transform.parent = dockTarget;
					transform.position = dockTarget.position;
					myRigidbody.velocity = Vector2.zero;
					myRigidbody.isKinematic = true;
					ChangeToNewState(StateMachine.Docked);
				}
			}
		}
		else 
			dockTimer = 0;
	}

	void Docked()
	{
		
	}

	void ReleaseFromDock()
	{
		transform.SetParent(null);
		myRigidbody.isKinematic = false;
	}

	void WarpOut()
	{
		if(switchingStates)
		{
			Subtitles.instance.PostSubtitle(new string[]{name + ", Engaging Warp Drive"});
			Camera.main.GetComponent<CameraControllerFighter>().target = null;
			Tools.instance.blackoutPanel.GetComponentInParent<Canvas> ().sortingOrder = 10;

			if(!_battleEventManager.instance.playerHasBeenRescued)
				_battleEventManager.instance.CallPlayerLeaving();
		
			Tools.instance.CommenceFade(6, 3, Color.clear, Color.black, true);
			Tools.instance.ClearWaypoints();

			foreach(GameObject turret in myTurrets)
			{
				turret.GetComponent<Collider2D>().enabled = false;
				turret.GetComponent<WeaponsTurret>().enabled = false;
			}
			camOffset = Camera.main.transform.position - transform.position;

			warpOutLookAtPoint = (literalSpawnPoint - transform.position).normalized * 1000;

			if(whichSide == WhichSide.PMC)
			{
				Subtitles.instance.PostSubtitle(new string[] {this.name + ". Warping out!"});
			}

			switchingStates = false;
		}


		if(!AmILookingAt(warpOutLookAtPoint, transform.up, 5))
		{
			engineScript.LookAtTarget(warpOutLookAtPoint);
		}
		else if(!warpDrive.warpBubble.enabled)
		{
			warpDrive.warpBubble.enabled = true;
			startTime = Time.time;
			engineScript.enabled = false;

			engineAudioSource.clip = warpStart;
			engineAudioSource.loop = false;
			engineAudioSource.Play();
		}
		else
		{
			transform.position += transform.up * Mathf.Pow((Time.time - startTime), 2) * 4 * Time.deltaTime;
		}
		//this will start the loop part of the warp noise after the first noise has played
		if(!engineAudioSource.isPlaying && warpDrive.warpBubble.enabled)
		{
			engineAudioSource.Stop();
			engineAudioSource.clip = warpLoop;
			engineAudioSource.loop = true;
			engineAudioSource.Play();
			if(pickingUpPlayer)
				StartCoroutine(Camera.main.GetComponent<CameraControllerFighter>().OrthoCameraZoomToSize(10, 0, 6));			
		}

		if (pickingUpPlayer)
		{
			Camera.main.transform.position = transform.position + camOffset;
		}
		else if(!pickingUpPlayer && Time.time > (startTime + warpInTime))
		{
			myCommander.myTransports.Remove(this.gameObject);
			if(enemyCommander.knownEnemyTransports.Contains(this.gameObject))
				enemyCommander.knownEnemyTransports.Remove(this.gameObject);
			healthScript.Invoke("Deactivate", 8);
		}
	}//end of WarpOut

	void ReportActivity()
	{
		CameraTactical.reportedInfo = this.name + "\n";

		CameraTactical.reportedInfo += StaticTools.SplitCamelCase(currentState.ToString());

		CameraTactical.reportedInfo += "\n";

		CameraTactical.instance.pipCamera.orthographicSize = 2;

		if (healthScript.health / healthScript.maxHealth < (0.33f)) {
			CameraTactical.reportedInfo += "Heavily Damaged";
		}
		else if (healthScript.health / healthScript.maxHealth < (0.66f)) {
			CameraTactical.reportedInfo += "Damaged";
		}
		else
			CameraTactical.reportedInfo += "Fully Functional";
	}
}

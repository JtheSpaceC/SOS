using UnityEngine;
using UnityEngine.UI;

public class AITransport : SupportShipFunctions {
	
	public enum StateMachine {AwaitingPickup, ReelingInPassengers, AllAboard, HoldingPosition, WarpIn, WarpOut, NA};
	public StateMachine currentState;
	public StateMachine previousState;

	[HideInInspector] public bool sentRadioMessageToPlayerGroup = false;
	[HideInInspector] public bool reelingInPlayerGroup = false;
	bool playerHadAutoDodge;
	int playerManaToRestore;

	public CircleCollider2D pickupOfferCollider;
	public Transform carry1;
	public Transform carry2;
	public Transform carry3;
	Transform[] carrySpots;

	GameObject carryFighter1;
	EnginesFighter carryFighter1Engines;
	HealthFighter carryFighter1Health;
	GameObject carryFighter2;
	EnginesFighter carryFighter2Engines;
	HealthFighter carryFighter2Health;
	GameObject carryFighter3;
	EnginesFighter carryFighter3Engines;
	HealthFighter carryFighter3Health;
	public GameObject[] fightersToCarry;
	EnginesFighter[] fighterEngineScripts;
	HealthFighter[] fighterHealthScripts;

	bool gaveAccelerateInput = false;
	[HideInInspector] public float startTime;
	Vector3 camOffset;

	[HideInInspector] public GameObject theCaller;
	[HideInInspector] public bool thisWasInitialInsertionJump = false;



	void Awake()
	{
		healthScript = GetComponent<HealthTransport> ();
		engineScript = GetComponent<EnginesFighter> ();
		warpDrive = GetComponentInChildren<WarpDrive>();
		//myRenderer = GetComponent<SpriteRenderer> ();
		//myRigidbody = GetComponent<Rigidbody2D> ();

		if (whichSide == WhichSide.Ally)
		{
			myCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("PMC Commander").GetComponent<AICommander> ();
			enemyCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("Enemy Commander").GetComponent<AICommander> ();
		}
		else if (whichSide == WhichSide.Enemy)
		{
			myCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("Enemy Commander").GetComponent<AICommander> ();
			enemyCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("PMC Commander").GetComponent<AICommander> ();
		}

		myCommander.myTransports.Add (this.gameObject);
		foreach(GameObject turret in myTurrets)
		{
			if(!myCommander.myTurrets.Contains(turret))
			{
				myCommander.myTurrets.Add(turret);
			}
		}

		warpOutLookAtPoint = myCommander.transform.position;
		carrySpots = new Transform[]{carry1, carry2, carry3};
		pickupOfferCollider.enabled = false;
	}

	void Start()
	{
		ChangeToNewState(currentState);
	}

	void OnEnable()
	{
		_battleEventManager.playerBeganDocking += PlayerCommencedDocking;
	}

	void OnDisable()
	{		
		_battleEventManager.playerBeganDocking -= PlayerCommencedDocking;		
	}


	void Update()
	{
		//TODO: Remove this?
		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.Keypad2))
		{
			GameObject.Find("ARROW 2").SendMessage("Death");
		}
		if(Input.GetKeyDown(KeyCode.Keypad3))
		{
			GameObject.Find("ARROW 3").SendMessage("Death");
		}
		if(Input.GetKeyDown(KeyCode.Keypad1))
		{
			GameObject.Find("ARROW 1").SendMessage("Death");
		}
		if (Input.GetKeyDown (KeyCode.Keypad4))
			this.SendMessage("Death");
		#endif

		if(previousState == StateMachine.WarpIn && switchingState)
		{
			engineAudioSource.Stop();
			engineAudioSource.loop = false;
			engineAudioSource.clip = warpEnd;
			engineAudioSource.Play();
		}

		if(currentState == StateMachine.AwaitingPickup)
		{
			AwaitingPickup();
			HoldPosition();
		}
		else if(currentState == StateMachine.HoldingPosition)
		{
			HoldPosition();
		}
		else if(currentState == StateMachine.ReelingInPassengers)
		{
			ReelInPassengers();
			if(reelingInPlayerGroup) //for breaking out of the dock menoeuvre
			{
				if(!gaveAccelerateInput && Input.GetAxis("Accelerate") != 0 && !RadioCommands.instance.buttonsShown)
				{
					RadioCommands.instance.communicatingGameObject = this.gameObject;
					RadioCommands.instance.activeButtons = new Button[]{RadioCommands.instance.button1, RadioCommands.instance.button3};
					RadioCommands.instance.ContextualTurnOnRadio ("CANCEL DOCK?", new string[]{"YES", "", "NO", ""}, false);
					gaveAccelerateInput = true;
				}
				else if(gaveAccelerateInput && Input.GetAxis("Accelerate") == 0)
					gaveAccelerateInput = false;
			}
		}
		else if(currentState == StateMachine.AllAboard)
		{
			AllAboard();
		}
		else if(currentState == StateMachine.WarpIn)
		{
			WarpIn();
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
		pickupOfferCollider.enabled = false;

		currentState = newState;
		switchingState = true;
	}


	void AwaitingPickup()
	{
		if(switchingState)
		{
			pickupOfferCollider.enabled = true;

			switchingState = false;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (currentState != StateMachine.AwaitingPickup)
			return;

		if(other.tag == "PlayerFighter")
		{
			if(!RadioCommands.instance.buttonsShown)
			{
				RadioCommands.instance.communicatingGameObject = this.gameObject;
				RadioCommands.instance.activeButtons = new Button[]{RadioCommands.instance.button1};
				RadioCommands.instance.ContextualTurnOnRadio ("DOCKING", new string[]{"DOCK", "", "", ""}, false);
				sentRadioMessageToPlayerGroup = true;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if((other.tag == "Fighter" || other.tag == "PlayerFighter") && other.transform.position.z != 0f)
		{
			other.transform.position = new Vector3 (other.transform.position.x, other.transform.position.y, 0);
		}
	}

	public void ReelInPassengers()
	{
		if(switchingState)
		{
			waypoint = (Vector2)transform.position + 2*GetComponent<Rigidbody2D>().velocity;

			warpOutLookAtPoint = literalSpawnPoint;
		
			SetUpReferences();

			switchingState = false;
		}//end of switch state. now for the actual function

		engineScript.MoveToTarget (waypoint, true);
		engineScript.LookAtTarget (warpOutLookAtPoint);

		if(reelingInPlayerGroup)
		{
			carryFighter1Engines.CheckAllThrusters();
			carryFighter1Engines.CheckThrusterAudio();
		}


		for(int i = 0; i < fightersToCarry.Length; i++)
		{
			Vector3 lineupPos = carrySpots[i].position + carrySpots[i].up * -2;

			//line the fighters up behind the transport first
			if(!fighterHealthScripts[i].dead && !fighterEngineScripts[i].linedUpToDock && 
				Vector2.Distance(fightersToCarry[i].transform.position, lineupPos) > 0.2f)
			{
				fighterEngineScripts[i].MoveToTarget(lineupPos, true);
				fighterEngineScripts[i].LookAtTarget(carrySpots[i].position);
			}
			else if(!fighterHealthScripts[i].dead && !fighterEngineScripts[i].linedUpToDock) //they're at the lineup point. Happens once
			{
				fighterEngineScripts[i].linedUpToDock = true;
				fighterEngineScripts[i].myRigidBody.velocity = Vector2.zero;
				fighterEngineScripts[i].transform.position = 
					new Vector3(fighterEngineScripts[i].transform.position.x, fighterEngineScripts[i].transform.position.y, carry1.position.z);
			}
			else if(fighterHealthScripts[i].dead)
			{
				fighterEngineScripts[i].linedUpToDock = true;
				fighterEngineScripts[i].securedToDock = true;
			}

			//reel them in
			if(!fighterHealthScripts[i].dead && fighterEngineScripts[i].linedUpToDock && !fighterEngineScripts[i].securedToDock)
			{
				fightersToCarry[i].transform.position += 
					((carrySpots[i].position - fightersToCarry[i].transform.position).normalized * 0.75f * Time.deltaTime);
				fighterEngineScripts[i].LookAtTarget(carrySpots[i].position + carrySpots[i].up);

				//check if they're in and dock them
				if(Vector2.Distance(fightersToCarry[i].transform.position, carrySpots[i].position) < 0.05f)
				{
					fightersToCarry[i].transform.position = carrySpots[i].position;
					fightersToCarry[i].transform.parent = carrySpots[i];
					fightersToCarry[i].GetComponent<TargetableObject>().myGui.SetActive(false);
					fighterEngineScripts[i].securedToDock = true;
					fighterEngineScripts[i].myRigidBody.isKinematic = true;
					fighterEngineScripts[i].EnginesEffect(0, false);
					//TODO: this may cause issues with enemies targeting us;

					if(fightersToCarry[i].tag == "PlayerFighter")
						GetComponent<AudioSource> ().Play (); //plays the clunking noise


					//check if that was the last fighter aboard
					bool allAboard = true;
					foreach(EnginesFighter engScript in fighterEngineScripts)
					{
						if(engScript.securedToDock == false)
							allAboard = false;
					}
					//if it was, change states
					if(allAboard)
						ChangeToNewState(StateMachine.AllAboard);
				}
			}
		}//end or for loop

	}//end of ReelInPassengers


	public void SetUpReferences()
	{
		if(reelingInPlayerGroup)
		{
			carryFighter1 = GameObject.FindGameObjectWithTag("PlayerFighter");
			carryFighter1.GetComponent<PlayerFighterMovement>().enabled = false; //we'll have to take over certain animation functions here, then.
			carryFighter1.GetComponentInChildren<Dodge>().enabled = false;
			carryFighter1Engines = carryFighter1.GetComponent<EnginesFighter>();
			carryFighter1Health = carryFighter1.GetComponent<HealthFighter>();
			playerHadAutoDodge = carryFighter1Health.playerHasAutoDodge;
			playerManaToRestore = carryFighter1Health.snapFocusAmount;
			carryFighter1Health.snapFocusAmount = 0;
			carryFighter1Health.playerHasAutoDodge = false;
			carryFighter1.SendMessage("ToggleWeaponsOnOff", false);
		}
		else
		{
			//TODO: let the AI groups do this too somehow
		}
		
		//change the squad's orders behaviour if the wingmen aren't retreating

		if(carryFighter1.GetComponentInChildren<SquadronLeader>().activeWingmen.Count == 0)
		{
			fightersToCarry = new GameObject[]{carryFighter1};
			fighterEngineScripts = new EnginesFighter[]{carryFighter1Engines};
			fighterHealthScripts = new HealthFighter[] {carryFighter1Health};
		}
		else if(carryFighter1.GetComponentInChildren<SquadronLeader>().firstFlightOrders != SquadronLeader.Orders.Disengage)
		{
			carryFighter1.GetComponentInChildren<SquadronLeader>().firstFlightOrders = SquadronLeader.Orders.Extraction;
			//TODO: check if it's actually the first flight, or if 2nd - 4th		
			
			//get the wingmen and create arrays of engine and fighter references
			if(carryFighter1.GetComponentInChildren<SquadronLeader>().activeWingmen.Count >= 1)
			{
				carryFighter2 = carryFighter1.GetComponentInChildren<SquadronLeader>().activeWingmen[0];
				carryFighter2Engines = carryFighter2.GetComponent<EnginesFighter>();
				fighterEngineScripts = new EnginesFighter[]{carryFighter1Engines, carryFighter2Engines};
				carryFighter2Health = carryFighter2.GetComponent<HealthFighter>();
				fighterHealthScripts = new HealthFighter[] {carryFighter1Health, carryFighter2Health};
				fightersToCarry = new GameObject[]{carryFighter1, carryFighter2};
				
				if(carryFighter1.GetComponentInChildren<SquadronLeader>().activeWingmen.Count >= 2)
				{
					carryFighter3 = carryFighter1.GetComponentInChildren<SquadronLeader>().activeWingmen[1];
					carryFighter3Engines = carryFighter3.GetComponent<EnginesFighter>();
					fighterEngineScripts = new EnginesFighter[]{carryFighter1Engines, carryFighter2Engines, carryFighter3Engines};
					carryFighter3Health = carryFighter3.GetComponent<HealthFighter>();
					fighterHealthScripts = new HealthFighter[] {carryFighter1Health, carryFighter2Health, carryFighter3Health};
					fightersToCarry = new GameObject[]{carryFighter1, carryFighter2, carryFighter3};
				}
			}
		}
		else
		{
			Debug.LogError("This shouldn't happen");
		}
		
		//change states and disable components
		foreach(GameObject fighter in fightersToCarry)
		{
			if(fighter.tag == "Fighter")
			{
				AIFighter fighterAIScript = fighter.GetComponent<AIFighter>();
				fighterAIScript.ChangeToNewState(new AIFighter.StateMachine[]{AIFighter.StateMachine.Docking}, new float[]{1});
				fighterAIScript.dockingWith = this.gameObject;
				fighterAIScript.healthScript.hasDodge = false;
				fighter.transform.FindChild("Breathing Room").gameObject.SetActive(false);
				fighter.SendMessage("ToggleWeaponsOnOff", false);
			}
		}
		foreach(EnginesFighter engScript in fighterEngineScripts)
		{
			engScript.linedUpToDock = false;
			engScript.securedToDock = false;
		}
	}//end of SetUpReferences


	void AllAboard()
	{
		if(switchingState)
		{
			if(reelingInPlayerGroup && !carryFighter1.GetComponent<HealthFighter>().dead)
			{
				Subtitles.instance.PostSubtitle(new string[]{"Transport here. All craft secured. Ready to go?"});
				
				RadioCommands.instance.communicatingGameObject = this.gameObject;
				RadioCommands.instance.activeButtons = new Button[]{RadioCommands.instance.button1, RadioCommands.instance.button3};
				RadioCommands.instance.ContextualTurnOnRadio("LEAVE?", new string[]{"CANCEL", "", "LEAVE", ""}, true);
			}
			else
			{
				reelingInPlayerGroup = false;
				ChangeToNewState(StateMachine.WarpOut);
				return;
			}

			switchingState = false;
		}

		engineScript.MoveToTarget (waypoint, true);
		engineScript.LookAtTarget (warpOutLookAtPoint);

		//for if this was the player group, but player dies before he presses Leave
		if(reelingInPlayerGroup && carryFighter1.GetComponent<HealthFighter>().dead)		
		{
			reelingInPlayerGroup = false;
			ChangeToNewState(StateMachine.WarpOut);
			Subtitles.instance.PostSubtitle(new string[] {carryFighter1.name + " is down! We're bugging out! Send recovery!!"});
		}
	}

	public void InstantAttachFighters(GameObject fighter1, GameObject fighter2, GameObject fighter3)
	{		
		if(fighter1 != null)
		{
			if(fighter1.activeSelf)
			{
				fighter1.transform.SetParent (carry1);
				fighter1.transform.position = carry1.transform.position;
				fighter1.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
				fighter1.GetComponent<Rigidbody2D> ().isKinematic = true;
				fighter1.transform.rotation = carry1.transform.rotation;
				fighter1.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Stop();
				fighter1.GetComponent<TargetableObject>().myGui.SetActive(false);
			}
		}

		if(fighter2 != null)
		{
			if(fighter2.activeSelf)
			{
				fighter2.transform.SetParent (carry2);
				fighter2.transform.position = carry2.transform.position;
				fighter2.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
				fighter2.GetComponent<Rigidbody2D> ().isKinematic = true;
				fighter2.transform.rotation = carry2.transform.rotation;
				fighter2.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Stop();
				fighter2.GetComponent<TargetableObject>().myGui.SetActive(false);
			}
		}

		if(fighter3 != null)
		{
			if(fighter3.activeSelf)
			{
				fighter3.transform.SetParent (carry3);
				fighter3.transform.position = carry3.transform.position;
				fighter3.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
				fighter3.GetComponent<Rigidbody2D> ().isKinematic = true;
				fighter3.transform.rotation = carry3.transform.rotation;
				fighter3.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Stop();
				fighter3.GetComponent<TargetableObject>().myGui.SetActive(false);
			}
		}
	}

	void WarpIn()
	{
		if(switchingState)
		{
			if(CheckTargetIsLegit(theCaller))
			{
				if(insertionPoint == Vector3.zero)
				{
					insertionPoint = theCaller.transform.position + (Vector3)Random.insideUnitCircle * 5f;
					insertionPoint += new Vector3(0, 0, transform.position.z);
				}
				literalSpawnPoint = transform.position;
				startTime = Time.time;
			}
			if(reelingInPlayerGroup)
			{
				RadioCommands.instance.canAccessRadio = false;
				CameraTactical.instance.canAccessTacticalMap = false;

				if(carryFighter2 != null)
				{
					carryFighter2.GetComponent<AIFighter>().myCharacterAvatarScript.warpBG.enabled = true;
					carryFighter2.GetComponent<AIFighter>().myCharacterAvatarScript.bgScrollerScript.dogfightVerticalScrollSpeed *= 20;
				}
				if(carryFighter3 != null)
				{
					carryFighter3.GetComponent<AIFighter>().myCharacterAvatarScript.warpBG.enabled = true;
					carryFighter3.GetComponent<AIFighter>().myCharacterAvatarScript.bgScrollerScript.dogfightVerticalScrollSpeed *= 20;
				}
			}

			camOffset = new Vector3(0, 0, Camera.main.transform.position.z);
			warpDrive.warpBubble.enabled = true;

			engineAudioSource.clip = warpLoop;
			engineAudioSource.loop = true;
			engineAudioSource.Play();

			switchingState = false;
		}
		transform.position = Vector3.Lerp (literalSpawnPoint, insertionPoint, (Time.time - startTime) / warpInTime);
		engineScript.LookAtTarget (insertionPoint + transform.up);

		if (reelingInPlayerGroup)
		{
			Camera.main.transform.position = transform.position + camOffset;
		}

		if(Time.time > startTime + warpInTime)
		{
			if(whichSide == WhichSide.Ally && thisWasInitialInsertionJump)
			{
				Invoke("ReleaseFightersAfterInsertion", 1.5f);
				Subtitles.instance.PostSubtitle(new string[] {this.name + " has arrived. Releasing Fighters.."});
				ChangeToNewState(StateMachine.HoldingPosition);
			}
			else if(whichSide == WhichSide.Ally && !thisWasInitialInsertionJump)
			{
				Subtitles.instance.PostSubtitle(new string[] {this.name + " entering combat zone. Ready for extraction."});
				ChangeToNewState(StateMachine.AwaitingPickup);
			}
			if (reelingInPlayerGroup) //turn off the speed particles for a moment
			{
				GameObject speedParticles = Camera.main.GetComponentInChildren<ParticleSystem>().gameObject;
				speedParticles.SetActive(false);
				speedParticles.SetActive(true);

				if(carryFighter2 != null)
				{
					carryFighter2.GetComponent<AIFighter>().myCharacterAvatarScript.warpBG.enabled = false;
					carryFighter2.GetComponent<AIFighter>().myCharacterAvatarScript.bgScrollerScript.dogfightVerticalScrollSpeed /= 20;
				}
				if(carryFighter3 != null)
				{
					carryFighter3.GetComponent<AIFighter>().myCharacterAvatarScript.warpBG.enabled = false;
					carryFighter3.GetComponent<AIFighter>().myCharacterAvatarScript.bgScrollerScript.dogfightVerticalScrollSpeed /= 20;
				}
			}
		}
	}//end of WarpIn
	void ReleaseFightersAfterInsertion(){ReleaseFighters (false);}


	void WarpOut()
	{
		if(switchingState)
		{
			if(reelingInPlayerGroup)
			{
				Subtitles.instance.PostSubtitle(new string[]{"Roger! Engaging Warp Drive"});
				Camera.main.GetComponent<CameraControllerFighter>().target = null;
				RadioCommands.instance.canAccessRadio = false;
				Tools.instance.blackoutPanel.GetComponentInParent<Canvas> ().sortingOrder = 10;
				_battleEventManager.instance.CallPlayerLeaving();
				Tools.instance.CommenceFadeout(6, 3);
				carryFighter1.GetComponent<PlayerAILogic>().orders = PlayerAILogic.Orders.RTB;
				Tools.instance.ClearWaypoints();

				if(carryFighter2 != null)
				{
					carryFighter2.GetComponent<AIFighter>().myCharacterAvatarScript.warpBG.enabled = true;
					carryFighter2.GetComponent<AIFighter>().myCharacterAvatarScript.bgScrollerScript.dogfightVerticalScrollSpeed *= 20;
				}
				if(carryFighter3 != null)
				{
					carryFighter3.GetComponent<AIFighter>().myCharacterAvatarScript.warpBG.enabled = true;
					carryFighter3.GetComponent<AIFighter>().myCharacterAvatarScript.bgScrollerScript.dogfightVerticalScrollSpeed *= 20;
				}
			}
			foreach(GameObject fighter in fightersToCarry)
			{
				fighter.GetComponent<Collider2D>().enabled = false;
			}
			foreach(GameObject turret in myTurrets)
			{
				turret.GetComponent<Collider2D>().enabled = false;
				turret.GetComponent<WeaponsTurret>().enabled = false;
			}
			camOffset = Camera.main.transform.position - transform.position;

			warpOutLookAtPoint = (literalSpawnPoint - transform.position).normalized * 10000;

			if(whichSide == WhichSide.Ally)
			{
				Subtitles.instance.PostSubtitle(new string[] {this.name + ". Warping out!"});
			}

			switchingState = false;
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
		}

		if (reelingInPlayerGroup)
		{
			Camera.main.transform.position = transform.position + camOffset;
		}
		else if(!reelingInPlayerGroup && Time.time > (startTime + warpInTime))
		{
			myCommander.myTransports.Remove(this.gameObject);
			if(enemyCommander.knownEnemyTransports.Contains(this.gameObject))
				enemyCommander.knownEnemyTransports.Remove(this.gameObject);
			healthScript.Invoke("Deactivate", 8);
		}
	}




	public void ReleaseFighters(bool isThisACancelledPickup)
	{
		if(isThisACancelledPickup)
		{
			Subtitles.instance.PostSubtitle (new string[]{"Cancelling Docking Procedure.", "Roger, aborting pickup."});
			ChangeToNewState(StateMachine.AwaitingPickup);
		}
		
		if(reelingInPlayerGroup)
		{
			RadioCommands.instance.canAccessRadio = true;
			CameraTactical.instance.canAccessTacticalMap = true;
			PlayerFighterMovement carryFighter1Movement = carryFighter1.GetComponent<PlayerFighterMovement>();
			carryFighter1Movement.enabled = true;
			carryFighter1.GetComponent<Rigidbody2D>().isKinematic = false;
			carryFighter1.GetComponentInChildren<Dodge>().enabled = true;
			carryFighter1.SendMessage("ToggleWeaponsOnOff", true);
			carryFighter1Health.playerHasAutoDodge = playerHadAutoDodge;
			carryFighter1Health.snapFocusAmount = playerManaToRestore;
			carryFighter1.GetComponentInChildren<SquadronLeader>().firstFlightOrders = SquadronLeader.Orders.CoverMe; //TODO: May move this line to respect AI leaders
			carryFighter1.GetComponent<TargetableObject>().myGui.SetActive(true);
		}
		else
		{
			//TODO: let the AI groups do this too somehow
		}
		
		//change states and enable components
		if (fightersToCarry == null)
			return;

		GetComponent<AudioSource> ().Play ();

		foreach(GameObject fighter in fightersToCarry)
		{
			if(fighter.tag == "Fighter" && fighter.activeSelf)
			{
				AIFighter fighterAIScript = fighter.GetComponent<AIFighter>();
				fighter.GetComponent<Rigidbody2D>().isKinematic = false;
				fighterAIScript.ChangeToNewState(new AIFighter.StateMachine[]{fighterAIScript.formerState}, new float[]{1});
				fighterAIScript.healthScript.hasDodge = true; //TODO: What if they didn't have dodge before?
				fighter.transform.FindChild("Breathing Room").gameObject.SetActive(true);
				fighter.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Play();
				fighter.SendMessage("ToggleWeaponsOnOff", true);
				fighterAIScript.myGui.SetActive(true);
			}

			fighter.transform.SetParent(null);
		}	
		fightersToCarry = new GameObject[0];
		reelingInPlayerGroup = false;
	}//end of ReleaseFighters
		

	void ReportActivity()
	{
		CameraTactical.reportedInfo = this.name + "\n";

		CameraTactical.reportedInfo += StaticTools.SplitCamelCase(currentState.ToString());

		CameraTactical.reportedInfo += "\n";

		if (healthScript.health / healthScript.maxHealth < (0.33f)) {
			CameraTactical.reportedInfo += "Heavily Damaged";
		}
		else if (healthScript.health / healthScript.maxHealth < (0.66f)) {
			CameraTactical.reportedInfo += "Damaged";
		}
		else
			CameraTactical.reportedInfo += "Fully Functional";
	}

	void PlayerCommencedDocking()
	{
		Subtitles.instance.PostSubtitle (new string[]{"Engaging Docking Procedure.", "Roger, recovering fighters."});
	}

}//Mono

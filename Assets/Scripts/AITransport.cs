using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AITransport : SupportShipFunctions {
	
	public enum StateMachine {awaitingPickup, reelingInPassengers, allAboard, holdingPosition, warpIn, warpOut, NA};
	public StateMachine currentState;
	public StateMachine previousState;

	[HideInInspector] public bool sentRadioMessageToPlayerGroup = false;
	[HideInInspector] public bool reelingInPlayerGroup = false;
	bool playerHadAutoDodge;
	int playerManaToRestore;

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
	GameObject[] fightersToCarry;
	EnginesFighter[] fighterEngineScripts;
	HealthFighter[] fighterHealthScripts;

	bool gaveAccelerateInput = false;
	[HideInInspector] public float startTime;
	Vector3 camOffset;

	[HideInInspector] public GameObject theCaller;
	[HideInInspector] public Vector2 insertionPoint;
	[HideInInspector] public Vector2 literalSpawnPoint;
	[HideInInspector] public Vector2 loadingUpLookAtPoint;
	[HideInInspector] public bool thisWasInitialInsertionJump = false;



	void Awake()
	{
		healthScript = GetComponent<HealthTransport> ();
		engineScript = GetComponent<EnginesFighter> ();
<<<<<<< HEAD
		warpDrive = GetComponentInChildren<WarpDrive>();
=======
>>>>>>> parent of ac76538... EVA Pilot, New Warp script. Assault Shuttle work.
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

		loadingUpLookAtPoint = myCommander.transform.position;
		carrySpots = new Transform[]{carry1, carry2, carry3};
	}


	void Update()
	{
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

		if(previousState == StateMachine.warpIn && switchingState)
		{
			engineAudioSource.Stop();
			engineAudioSource.loop = false;
			engineAudioSource.clip = warpEnd;
			engineAudioSource.Play();
		}

		if(currentState == StateMachine.awaitingPickup)
		{
			HoldPosition();
			AwaitingPickup();
		}
		else if(currentState == StateMachine.holdingPosition)
		{
			HoldPosition();
		}
		else if(currentState == StateMachine.reelingInPassengers)
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
		else if(currentState == StateMachine.allAboard)
		{
			AllAboard();
		}
		else if(currentState == StateMachine.warpIn)
		{
			WarpIn();
		}
		else if(currentState == StateMachine.warpOut)
		{
			WarpOut();
		}
	}


	public void ChangeToNewState(StateMachine newState)
	{
		previousState = currentState;

		warpBubble.enabled = false;

		currentState = newState;
		switchingState = true;
	}


	void AwaitingPickup()
	{
		if(switchingState)
		{
			switchingState = false;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (currentState != StateMachine.awaitingPickup)
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

	void OnTriggerExit2D (Collider2D other)
	{
		if(other.tag == "Fighter")
		{
			if(!other.GetComponent<EnginesFighter>().linedUpToDock)
			{
				other.GetComponent<SpriteRenderer>().sortingLayerName = "Fighters";
				other.SendMessage("MoveAllSortingOrders");
			}
		}
		else if(other.tag == "PlayerFighter")
		{
			if(!other.GetComponent<PlayerFighterMovement>().linedUpToDock)
			{
				other.GetComponent<SpriteRenderer>().sortingLayerName = "Fighters";
				other.SendMessage("MoveAllSortingOrders");
			}
		}
	}

	public void ReelInPassengers()
	{
		if(switchingState)
		{
			Subtitles.instance.PostSubtitle (new string[]{"Engaging Docking Procedure.", "Roger, recovering fighters."});

			waypoint = (Vector2)transform.position + 2*GetComponent<Rigidbody2D>().velocity;

			loadingUpLookAtPoint = literalSpawnPoint;
		
			SetUpReferences();

			switchingState = false;
		}//end of switch state. now for the actual function

		engineScript.MoveToTarget (waypoint, true);
		engineScript.LookAtTarget (loadingUpLookAtPoint);

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
			else if(!fighterHealthScripts[i].dead && !fighterEngineScripts[i].linedUpToDock)
			{
				fighterEngineScripts[i].linedUpToDock = true;
				fighterEngineScripts[i].myRigidBody.velocity = Vector2.zero;
				SetSortingLayersBeneathTheTransport(fightersToCarry[i]); //TODO: This seems to sometimes not work, 
				//at least I assume this is the problem
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
						ChangeToNewState(StateMachine.allAboard);
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
			playerManaToRestore = carryFighter1Health.manaToRestoreOnAHit;
			carryFighter1Health.manaToRestoreOnAHit = 0;
			carryFighter1Health.playerHasAutoDodge = false;
			carryFighter1.SendMessage("ToggleWeaponsOnOff", false);
		}
		else
		{
			//TODO: let the AI groups do this too somehow
		}
		
		//change the squad's orders behaviour
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
		else
		{
			fightersToCarry = new GameObject[]{carryFighter1};
			fighterEngineScripts = new EnginesFighter[]{carryFighter1Engines};
			fighterHealthScripts = new HealthFighter[] {carryFighter1Health};
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


	public void SetSortingLayersBeneathTheTransport(GameObject fighterToChange)
	{
		//first change the fighter's layer to this one
		fighterToChange.GetComponent<SpriteRenderer>().sortingLayerName = this.GetComponent<SpriteRenderer>().sortingLayerName;
		//then make it change all its own children to match its own new layer
		fighterToChange.SendMessage("MoveAllSortingOrders");
	}

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
				ChangeToNewState(StateMachine.warpOut);
				return;
			}

			switchingState = false;
		}

		engineScript.MoveToTarget (waypoint, true);
		engineScript.LookAtTarget (loadingUpLookAtPoint);

		//for if this was the player group, but player dies before he presses Leave
		if(reelingInPlayerGroup && carryFighter1.GetComponent<HealthFighter>().dead)		
		{
			reelingInPlayerGroup = false;
			ChangeToNewState(StateMachine.warpOut);
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
				//fighter1.GetComponent<HealthFighter>().trailRenderer.enabled = false; //not using anymore
				fighter1.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Stop();
				SetSortingLayersBeneathTheTransport (fighter1);
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
				//fighter2.GetComponent<HealthFighter>().trailRenderer.enabled = false;
				fighter2.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Stop();
				SetSortingLayersBeneathTheTransport (fighter2);
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
				//fighter3.GetComponent<HealthFighter>().trailRenderer.enabled = false;
				fighter3.transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().Stop();
				SetSortingLayersBeneathTheTransport (fighter3);
			}
		}
	}

	void WarpIn()
	{
		if(switchingState)
		{
			if(CheckTargetIsLegit(theCaller))
			{
				if(insertionPoint == Vector2.zero)
					insertionPoint = (Vector2)theCaller.transform.position + Random.insideUnitCircle * 5f;
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

			camOffset = new Vector3(0, 0, -50);
			warpBubble.enabled = true;

			engineAudioSource.clip = warpLoop;
			engineAudioSource.loop = true;
			engineAudioSource.Play();

			switchingState = false;
		}
		transform.position = Vector2.Lerp (literalSpawnPoint, insertionPoint, (Time.time - startTime) / warpInTime);
		engineScript.LookAtTarget (insertionPoint + (Vector2)transform.up);

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
				ChangeToNewState(StateMachine.holdingPosition);
			}
			else if(whichSide == WhichSide.Ally && !thisWasInitialInsertionJump)
			{
				Subtitles.instance.PostSubtitle(new string[] {this.name + " entering combat zone. Ready for extraction."});
				ChangeToNewState(StateMachine.awaitingPickup);
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
				Invoke("CommenceFadeout", 6);
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

			loadingUpLookAtPoint = literalSpawnPoint;

			if(whichSide == WhichSide.Ally)
			{
				Subtitles.instance.PostSubtitle(new string[] {this.name + ". Warping out!"});
			}

			switchingState = false;
		}


		if(!AmILookingAt(loadingUpLookAtPoint, transform.up, 5))
		{
			engineScript.LookAtTarget(loadingUpLookAtPoint);
		}
		else if(!warpBubble.enabled)
		{
			warpBubble.enabled = true;
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
		if(!engineAudioSource.isPlaying && warpBubble.enabled)
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
			ChangeToNewState(StateMachine.awaitingPickup);
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
			carryFighter1Health.manaToRestoreOnAHit = playerManaToRestore;
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
			}

			fighter.transform.SetParent(null);
		}	
		fightersToCarry = new GameObject[0];
		reelingInPlayerGroup = false;
	}//end of ReleaseFighters

	void CommenceFadeout()
	{
		Tools.instance.CommenceFadeout (3);
	}

	void ReportActivity()
	{
		PlayerPrefs.SetString ("craftName", this.name);
		
		PlayerPrefs.SetString("craftOrders", this.currentState.ToString());
		
		if (healthScript.health / healthScript.maxHealth < (1 / 3))
			PlayerPrefs.SetString ("craftHealth", "Heavily Damaged");
		else if (healthScript.health / healthScript.maxHealth < (2 / 3))
			PlayerPrefs.SetString ("craftHealth", "Damaged");
		else
			PlayerPrefs.SetString ("craftHealth", "Fully Functional");
	}

}//Mono

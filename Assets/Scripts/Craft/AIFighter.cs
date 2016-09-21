using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AIFighter : FighterFunctions {

	[HideInInspector]public HealthFighter healthScript;
	[HideInInspector]public EnginesFighter engineScript;
	[HideInInspector]public WeaponsPrimaryFighter shootScript;
	[HideInInspector]public WeaponsSecondaryFighter missilesScript;
	public Character myCharacterAvatarScript;

	public enum Orders {FighterSuperiority, Patrol, Escort, Wingman, RTB, FullRetreat, NA}; //set by commander //TODO: not used yet
	public Orders orders;

	public enum StateMachine {Patroling, Tailing, Jousting, Evade, Retreat, FormUp, Covering, Docking, NA};
	public StateMachine currentState;
	public StateMachine formerState;
	[HideInInspector] public StateMachine[] normalStates;
	[HideInInspector] public StateMachine[] combatStates;
	[HideInInspector] public StateMachine[] formUpStates;
	[HideInInspector] public StateMachine[] coverMeStates;
	[HideInInspector] public StateMachine[] retreatStates;
	[HideInInspector] public StateMachine[] deathStates;

	public GameObject target;
	GameObject targetCheck;
	GameObject localTarget;
	public GameObject formerTarget;
	public GameObject flightLeader;
	[HideInInspector] public SquadronLeader flightLeadSquadronScript;
	[HideInInspector] public Rigidbody2D flightLeaderRigidbody;

	//these are set by Squadron Leader script
	public enum SquadronMembership {one, two, three, four, five, six, seven, eight, nine, ten, eleven, twelve};
	[HideInInspector] public SquadronMembership squadronMembership;

	[HideInInspector] public GameObject myFormationPosition;
	[HideInInspector] public int mySquadUnitNumber = 1;

	public Vector2 patrolPoint;
	public Vector2 guardPoint;
	public Transform escortShip;
	public bool movingGuardPoint = false;
	public float guardDistance = 20; //when Patrolling, how close an enemy has to get to break up the patrol
	public float coveringDistance = 15; //how close to stay to leader when Covering them
	[Tooltip("Used when Evading to see if nearby people need to be avoided.")]
	public float dangerRadius = 25f;
	LayerMask enemyTargets;
	LayerMask dangerSources; //Any layers that could do this fighter harm. Fighter will stay away if retreating
	public Vector2 evadePosition;
	public Vector2 retreatPosition;

	float timer = 0;
	float nextTime = 0;

	[Tooltip("If health is below this proportion of maxHealth, they'll evade & retreat. Is randomised away from this base number at Start")]
	[Range(0, 100f)]
	public float cowardice = 30;
	public bool inRetreatState = false;

	public GameObject HUDPointer;
	[HideInInspector] public GameObject dockingWith;
	public Text nameHUDText;

	[HideInInspector] public bool statsAlreadyAdjusted = false;

	public int mySkillLevel = 2;
	

	void Awake()
	{
		healthScript = GetComponent<HealthFighter> ();
		engineScript = GetComponent<EnginesFighter> ();
		shootScript = GetComponentInChildren<WeaponsPrimaryFighter> ();
		missilesScript = GetComponentInChildren<WeaponsSecondaryFighter> ();
		if(transform.FindChild("Effects/GUI"))
		{
			myGui = transform.FindChild("Effects/GUI").gameObject;
		}

		myRigidbody = GetComponent<Rigidbody2D> ();

		SetUpSideInfo();
		enemyTargets = myCommander.fighterEnemyTargets;
		dangerSources = myCommander.fighterEnemyDangerSources;

		myCommander.myFighters.Add (this.gameObject);
		enemyCommander.AddEnemyFighters(this.gameObject); //TODO; AI Commander instantly knows all enemies. Make more complex

		normalStates = new StateMachine[]{StateMachine.Patroling};
		combatStates = new StateMachine[]{StateMachine.Tailing, StateMachine.Jousting};
		formUpStates = new StateMachine[]{StateMachine.FormUp};
		coverMeStates = new StateMachine[]{StateMachine.Covering};
		retreatStates = new StateMachine[] {StateMachine.Evade, StateMachine.Retreat};
		deathStates = new StateMachine[] {StateMachine.NA};

		if(GetComponentInChildren<Character>())
		{
			myCharacterAvatarScript = GetComponentInChildren<Character>();
			myCharacterAvatarScript.myAIFighterScript = this;
			myCharacterAvatarScript.name += " " + this.gameObject.name;
			myCharacterAvatarScript.transform.SetParent(null); //detach from this so it's not always doing position updates with movement
		}
	}

	void Start () 
	{
		if(whichSide == WhichSide.Enemy)
			cowardice = Mathf.Clamp(cowardice *= Random.Range (0.25f, 1.5f), 0, 99); //TODO: based on character stats for PMC?
		else
			cowardice = 100/healthScript.maxHealth;

		StartCoroutine(SetUpAvatarBars());
	}

	IEnumerator SetUpAvatarBars()
	{
		while(flightLeader == null)
			yield return new WaitForEndOfFrame();
		
		if(flightLeader != null && flightLeader.tag == "PlayerFighter" && mySquadUnitNumber <= 3)
		{
			//old way
			/*healthScript.avatarAwarenessBars = myCharacterAvatarScript.avatarOutput.transform.FindChild("Awareness Panel");
			healthScript.avatarHealthBars = myCharacterAvatarScript.avatarOutput.transform.FindChild("HP Panel");*/
			healthScript.avatarRadialHealthBar = 
				myCharacterAvatarScript.avatarOutput.transform.parent.parent.FindChild("2 Hull Health").GetComponent<Image>();
			healthScript.avatarRadialAwarenessBar = 
				myCharacterAvatarScript.avatarOutput.transform.parent.parent.FindChild("3 Situational Awareness").GetComponent<Image>();
			healthScript.avatarFlashImage = myCharacterAvatarScript.avatarOutput.transform.FindChild("Flash Image").GetComponent<Image>();
			healthScript.SetUpAvatarBars();
		}
	}

	public void SetSquadReferences()
	{
		flightLeadSquadronScript = flightLeader.GetComponentInChildren<SquadronLeader> ();
		flightLeaderRigidbody = flightLeader.GetComponent<Rigidbody2D>();

		if(squadronMembership == SquadronMembership.one)
		{
			mySquadUnitNumber = 1;
		}
		else if(squadronMembership == SquadronMembership.two)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos2;
			mySquadUnitNumber = 2;
		}
		else if(squadronMembership == SquadronMembership.three)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos3;
			mySquadUnitNumber = 3;
		}
		else if(squadronMembership == SquadronMembership.four)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos4;
			mySquadUnitNumber = 4;
		}
		else if(squadronMembership == SquadronMembership.five)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos5;
			mySquadUnitNumber = 5;
		}
		else if(squadronMembership == SquadronMembership.six)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos6;
			mySquadUnitNumber = 6;
		}
		else if(squadronMembership == SquadronMembership.seven)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos7;
			mySquadUnitNumber = 7;
		}
		else if(squadronMembership == SquadronMembership.eight)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos8;
			mySquadUnitNumber = 8;
		}
		else if(squadronMembership == SquadronMembership.nine)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos9;
			mySquadUnitNumber = 9;
		}
		else if(squadronMembership == SquadronMembership.ten)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos10;
			mySquadUnitNumber = 10;
		}
		else if(squadronMembership == SquadronMembership.eleven)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos11;
			mySquadUnitNumber = 11;
		}
		else if(squadronMembership == SquadronMembership.twelve)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPos12;
			mySquadUnitNumber = 12;
		}
	}
	

	void Update () 
	{	
		#region To Manage Retreating	

		//if we get hit, go evasive if health is lower than cowardice rating
		if(currentState != StateMachine.Docking && !inRetreatState && !healthScript.dead &&
			(healthScript.health/healthScript.maxHealth * 100) <= cowardice)
		{
			if(healthScript.lastDamageWasFromAsteroid)
			{ 
				//do nothing
			}
			else
			{
				ChangeToNewState(retreatStates, new float[]{2,1});
				healthScript.awareness += (int)(healthScript.maxAwareness/2f);
				inRetreatState = true;
				if(flightLeadSquadronScript.activeWingmen.Contains(this.gameObject))
					flightLeadSquadronScript.activeWingmen.Remove(this.gameObject);
				if(!flightLeadSquadronScript.retreatingWingmen.Contains(this.gameObject))
					flightLeadSquadronScript.retreatingWingmen.Add(this.gameObject);

				if(LayerMask.LayerToName(gameObject.layer) == "PMCFighters")
				{
					Subtitles.instance.PostSubtitle(new string[]{"This is " +this.name + ". I'm hit! Breaking off!", 
						"This is " +this.name + ". I've taken critical damage! Bugging out!"});
					HUDPointerOn(4);
					_battleEventManager.instance.CallWingmanInTrouble();

					myCharacterAvatarScript.StartCoroutine("Speaking");
				}
			}
		}
		//if we were retreating and health goes up, return to battle
//		else if(inRetreatState && orders != Orders.FullRetreat && (healthScript.health * 100 / healthScript.maxHealth) > cowardice
//		        &&(healthScript.health  > healthScript.maxHealth * 3/4))
//		{
//			//return to last state (and re-establish initial parameters for it)
//			switchingStates = true;
//			inRetreatState = false;
//			currentState = formerState;
//			target = (CheckTargetIsLegit(formerTarget) == true)? formerTarget: null; //check if old target is still alive
//
//			if(LayerMask.LayerToName(gameObject.layer) == "PMCFighters")
//			{
//				Subtitles.instance.PostSubtitle(new string[]{"This is " +this.name + ", returning to the fight!",
//				this.name + " back on-station!",
//				"This is " + this.name + ", damage under control. I'm good to go!"});
//				HUDPointerOn(4);
//				_battleEventManager.instance.CallWingmanBack();
//
//				myCharacterAvatarScript.StartCoroutine("Speaking");
//			}
//		}
		#endregion

		if(currentState == StateMachine.Tailing)
		{
			Tailing();
		}
		else if(currentState == StateMachine.Jousting)
		{
			Jousting();
		}
		else if(currentState == StateMachine.Patroling)
		{
			Patrol();
		}
		else if(currentState == StateMachine.FormUp)
		{
			FormUp();
		}
		else if(currentState == StateMachine.Covering)
		{
			Covering();
		}
		else if(currentState == StateMachine.Evade)
		{
			Evade();
		}
		else if(currentState == StateMachine.Retreat)
		{
			Retreat();
		}
		else if(currentState == StateMachine.Docking)
		{
			//then the Transport will take over, like autopilot
		}
	}//end of Update
	

	public void ChangeToNewState(StateMachine [] possibleStates, float[] weights)
	{
		//TODO: Make this a better check
		if(orders == Orders.FullRetreat)
			return;

		if(target != null)
		{
			target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); 
			formerTarget = target;
			target = null;
		}
		timer = 0;
		nextTime = 0;
		inRetreatState = false;
		CancelInvoke ("LeaveThemAlone");
		engineScript.currentMaxVelocityAllowed = engineScript.maxNormalVelocity;
		engineScript.currentAccelerationRate = engineScript.normalAccelerationRate;
		shootScript.enabled = true;

		float totalWeight = 0;
		float weightCheck = 0;
		
		for(int i = 0; i < weights.Length; i++)
		{
			totalWeight += weights[i];
		}
		float roll = Random.Range (0, totalWeight);
		
		for(int i = 0; i< possibleStates.Length; i++)
		{
			weightCheck += weights[i];
			if(roll <= weightCheck)
			{
				switchingStates = true;
				//record our last order so we can go back to it later
				formerState = currentState != StateMachine.Docking? currentState: formerState;
				currentState = possibleStates[i];
				return;
			}
		}
		//if we failed
		switchingStates = true;
		Debug.LogError ("SWITCHING STATES function FAILED on " + this.gameObject.name + ". Setting state to Patrol (default)");
		currentState = StateMachine.Patroling;
	}

	#region State Handler Methods
	void Tailing()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;
			target = myCommander.ClosestPriorityTarget(myCommander.knownEnemyFighters, transform.position);

			if(target == null)
				target = myCommander.ClosestPriorityTarget(myCommander.knownEnemyTurrets, transform.position);

			if(target == null)
			{
				float[] weights = new float[]{1};
				ChangeToNewState(normalStates, weights);
				return;
			}
			if(target != null)
			{
				if(!Tools.instance.CheckTargetIsLegit(target))
				{
					if(myCommander.knownEnemyFighters.Contains(target))
						myCommander.knownEnemyFighters.Remove(target);

					target = null;
					return;
				}

				target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
			}
	
			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;
			constantThrustProportion = 0;
			//TODO: adjust the turn rate based on the forward speed

			switchingStates = false;
		}

		if(target == null)
		{
			float[] weights = new float[]{1};
			ChangeToNewState(normalStates, weights);
			return;
		}
		if (Tools.instance.CheckTargetIsLegit (target) == true) 
		{
			TailingFunction (engineScript, target, shootScript, constantThrustProportion);

			//if the target is retreating and there are other active fighters, let this one go.
			if(Tools.instance.CheckTargetIsRetreating(targetCheck, this.gameObject, "dogfighting"))
			{
				if(myCommander.ClosestPriorityTarget(myCommander.knownEnemyFighters, transform.position) != null
					|| (escortShip && Vector2.Distance(target.transform.position, escortShip.position) > (guardDistance * 2)))
				{
					Invoke("LeaveThemAlone", 2);
				}
			}
		} 
		else 
		{
			target = null;
		}
	}//end of TAILING()


	void Jousting()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;
			target = myCommander.ClosestPriorityTarget(myCommander.knownEnemyFighters, transform.position);

			if(target == null)
				target = myCommander.ClosestPriorityTarget(myCommander.knownEnemyTurrets, transform.position);

			if(target == null)
			{
				float[] weights = new float[]{1};
				ChangeToNewState(normalStates, weights);
				return;
			}
			if(target != null)
			{
				if(!Tools.instance.CheckTargetIsLegit(target))
				{
					if(myCommander.knownEnemyFighters.Contains(target))
						myCommander.knownEnemyFighters.Remove(target);

					target = null;
					return;
				}

				target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
			}

			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;
			constantThrustProportion = 0;
			//TODO: adjust the turn rate based on the forward speed

			switchingStates = false;
		}

		if(target == null)
		{
			float[] weights = new float[]{1};
			ChangeToNewState(normalStates, weights);
			return;
		}
		if (Tools.instance.CheckTargetIsLegit (target) == true) 
		{
			JoustingFunction (engineScript, target, shootScript, constantThrustProportion);

			//if the target is retreating and there are other active fighters, let this one go.
			if(Tools.instance.CheckTargetIsRetreating(targetCheck, this.gameObject, "dogfighting"))
			{
				if(myCommander.ClosestPriorityTarget(myCommander.knownEnemyFighters, transform.position) != null
					|| (escortShip && Vector2.Distance(target.transform.position, escortShip.position) > (guardDistance * 2)))
				{
					Invoke("LeaveThemAlone", 2);
				}
			}
		} 
		else 
		{
			target = null;
		}
	}//end of JOUSTING


	void Patrol()
	{
		if(switchingStates)
		{
			shootScript.enabled = false;
			if(target != null)
			{	
				target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); //may not be needed
				Debug.Log ("Target wasn't null for some reason");
			}
			target = null;
			timer = 1;

			ChooseGuardPoint();
			patrolPoint = PatrolPoint(guardPoint, guardDistance);
			engineScript.currentMaxVelocityAllowed = engineScript.maxNormalVelocity * 3/4;

			if(orders == Orders.Wingman)
			{
				movingGuardPoint = true;
			}
			else
				movingGuardPoint = false;

			switchingStates = false;
		}

		timer += Time.deltaTime;
		if(timer >= 1)
		{
			timer = 0;
			targetCheck = myCommander.ClosestPriorityTarget(myCommander.knownEnemyFighters, transform.position);

			if(targetCheck != null)
			{
				//CheckAndAddTargetToCommanderList(myCommander, targetCheck);
				if(escortShip && Vector2.Distance(targetCheck.transform.position, escortShip.position) < guardDistance + 15f)
				{
					myCommander.RequestOrders(this);
					return;
				}
				else if(escortShip && Vector2.Distance(targetCheck.transform.position, transform.position) < 15f)
				{
					myCommander.RequestOrders(this);
					return;
				}
				else if(!escortShip)
				{
					myCommander.RequestOrders(this);
					return;
				}
			}
		}

		if(Vector2.Distance(transform.position, patrolPoint) < 5)
		{
			ChooseGuardPoint();
			patrolPoint = PatrolPoint(guardPoint, guardDistance);
		}
		engineScript.LookAtTarget (patrolPoint);

		if(Vector2.Angle(transform.up, patrolPoint - (Vector2)transform.position) < 10)
			engineScript.MoveToTarget (patrolPoint, false);
	}//end of PATROL()


	void FormUp()
	{
		if(switchingStates)
		{
			timer = 0;

			target = myFormationPosition.gameObject;

			shootScript.enabled = true;
			engineScript.currentAccelerationRate = engineScript.normalAccelerationRate * engineScript.afterburnerMultiplier;

			switchingStates = false;
		}

		engineScript.currentMaxVelocityAllowed = 
			Mathf.Clamp(engineScript.maxAfterburnerVelocity + Vector2.Distance(target.transform.position, transform.position) * 1.5f,
				engineScript.maxAfterburnerVelocity + 2, engineScript.maxAfterburnerVelocity + 15);

			
		if(Vector2.Distance(((Vector2)transform.position + myRigidbody.velocity), 
			(Vector2)target.transform.position + flightLeaderRigidbody.velocity) < 4)
		{
			engineScript.LookAtTarget (flightLeader.transform.position + flightLeader.transform.up * 50);
			engineScript.MoveToTarget (target, 0);
		}
		else
		{
			engineScript.LookAtTarget (engineScript.newMovementPosition);

			if(Vector2.Angle(transform.up, engineScript.newMovementPosition - (Vector2)transform.position) < 10)
				engineScript.MoveToTarget (target, 0);			
		}

		if(Time.time > shootScript.nextFire && (TakePotshot(transform, shootScript.weaponsRange) == true))
		{
			if(shootScript.enabled)
				shootScript.FirePrimary(false);
			//NOTE: The joint-fire shooting is handled from the weapon script on the player
		}
	}//end of FORMUP()


	void Covering()
	{
		//NOTE: Part of the target finding functionality is in the TargetDestroyed() method.
		if(switchingStates)
		{
			if(myFormationPosition == null) //if we haven't a leader or it's not set up properly, we can't 
				//cover anyone, so return
			{
				ChangeToNewState(normalStates, new float[]{1});
				return;
			}
			
			shootScript.enabled = true;
			timer = 0;
			constantThrustProportion = Random.Range (0,76)/100; //not used
			engineScript.currentAccelerationRate = engineScript.normalAccelerationRate * engineScript.afterburnerMultiplier;

			switchingStates = false;
		}
		if(target == null)
		{
			engineScript.currentMaxVelocityAllowed = 
				Mathf.Clamp(engineScript.maxAfterburnerVelocity + 
					Vector2.Distance(myFormationPosition.transform.position, transform.position) * 1.5f,
					engineScript.maxAfterburnerVelocity + 2, engineScript.maxAfterburnerVelocity + 15); 

			//if no targets, form up like in Form Up function, but search for enemies like on Patrol

			if(Vector2.Distance(((Vector2)transform.position + myRigidbody.velocity), 
				(Vector2)myFormationPosition.transform.position + flightLeaderRigidbody.velocity) < 4)
			{
				engineScript.LookAtTarget (flightLeader.transform.position + flightLeader.transform.up * 50);
				engineScript.MoveToTarget (myFormationPosition, 0);
			}
			else
			{
				engineScript.LookAtTarget (engineScript.newMovementPosition);

				/*if(Vector2.Angle(transform.up, engineScript.newMovementPosition - (Vector2)transform.position) < 10)	*/				
					engineScript.MoveToTarget (myFormationPosition, 0);
			}

			//check for targets every second, instead of every frame
			timer += Time.deltaTime;
			if(timer >= 1)
			{
				timer = 0;
				GameObject targetCheck;

				if(flightLeader.tag == "PlayerFighter")
					targetCheck = myCommander.ClosestPriorityTarget(flightLeader.GetComponent<PlayerAILogic>().myAttackers, transform.position);
				else
					targetCheck = myCommander.ClosestPriorityTarget(flightLeader.GetComponent<AIFighter>().myAttackers, transform.position);

				if(targetCheck != null) //if someone's attacking my Leader
				{
					if(!Tools.instance.CheckTargetIsLegit(targetCheck) || Tools.instance.CheckTargetIsRetreating(targetCheck, this.gameObject, "first. timer > 1. targetCheck != null"))
					{
						RemoveBadTargetFromLeadersAttackers(flightLeader, targetCheck);
						return;
					}
					if(Vector2.Distance(flightLeader.transform.position, targetCheck.transform.position) < coveringDistance)
					{
						target = targetCheck;
						target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
					}
				}
				else // if targetCheck is null
				{
					localTarget = CheckLocaleForTargets(transform.position, coveringDistance, enemyTargets);
					if(localTarget != null)
					{
						if(!Tools.instance.CheckTargetIsLegit(localTarget))
						{
							//Debug.Log("SCANNED LOCALITY FOR A TARGET AND CHOSE INACTIVE ONE");
							return;
						}
						else if(Tools.instance.CheckTargetIsRetreating(localTarget, this.gameObject,
							"timer >1. else. localTarget != null. else"))
						{
							return;
						}
						//CheckAndAddTargetToCommanderList(myCommander, localTarget);

						target = localTarget;
						target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
					}
				}
			}//end of 1 second check
		}//end of if(target == null)

		else if(target != null)
		{

			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;
			if(Tools.instance.CheckTargetIsLegit(target) == true)
			{
				TailingFunction (engineScript, target, shootScript, constantThrustProportion); //WARNING: This function can result in target being set to null
			}
			//This section should be excessive, but is still the only way to stop craft targeting dead enemies and shooting at their death spots
			else
			{
				//REMOVE when sure I don't want any more
				//Debug.Log(gameObject.name + " is setting " + target.name + " to null. NOT LEGIT");
				target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); 
				target = null;
				return;
			}
			if(target != null && Tools.instance.CheckTargetIsRetreating(target, this.gameObject, "the last one"))
			{
				Invoke("LeaveThemAlone", 2);
				return;
			}
		}
	}//end of COVERING()


	void Evade()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;

			timer = 1;
			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;
			
			switchingStates = false;
		}
		timer += Time.deltaTime;

		if(timer >= nextTime)
		{
			timer = 0;
			nextTime = Random.Range (1f, 2.5f);

			if(CheckLocaleForTargets(transform.position, dangerRadius, dangerSources) == null)
			{
				ChangeToNewState(new StateMachine[]{StateMachine.Retreat}, new float[] {1});
				return;
			}
			else
				evadePosition = ChooseEvadePos(myRigidbody, dangerSources, myCommander);
		}

		engineScript.LookAtTarget (evadePosition);
		engineScript.MoveToTarget (evadePosition, false);

		//for potshot
		if(Time.time > shootScript.nextFire && TakePotshot(transform, shootScript.weaponsRange) == true)
		{
			if(shootScript.enabled)
			{
				shootScript.FirePrimary(false);
			}
		}
	}//end of EVADE()


	void Retreat()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;
			
			timer = 1.5f;
			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;

			//if you are the flight leader set a new leader
			SquadronLeader squadLeadScript = GetComponentInChildren<SquadronLeader>();

			if(squadLeadScript != null && squadLeadScript.firstFlightOrders != SquadronLeader.Orders.Extraction)
			{
				squadLeadScript.AssignNewLeader(true); //if there's another active wingman, leadership will pass to them
			}
			//else if you were a squad member
			else if(flightLeadSquadronScript) 
			{
				flightLeadSquadronScript.activeWingmen.Remove(gameObject);
				flightLeadSquadronScript.CheckActiveMateStatus();
				flightLeadSquadronScript.AssignNewLeader(false); //may or may not reassign wingmen to new squads
			}
			
			switchingStates = false;
		}
		timer += Time.deltaTime;
		
		if(timer >= nextTime)
		{
			timer = 0;
			nextTime = Random.Range (0.5f, 1.5f);

			evadePosition = ChooseRetreatPosition(myRigidbody, potshotAtEnemiesMask, enemyCommander);

			if(Vector2.Distance(transform.position, Camera.main.transform.position) > 250f)
				healthScript.RetreatAndRetrieval();
		}

		engineScript.LookAtTarget (evadePosition);
		engineScript.MoveToTarget (evadePosition, false);
		
		if(Time.time > shootScript.nextFire && TakePotshot(transform, shootScript.weaponsRange) == true)
		{
			if(shootScript.enabled)
			{
				shootScript.FirePrimary(false);
			}
		}
	}//end of RETREAT()



	//MECHANICAL-TYPE FUNCTIONS BELOW HERE
	
	void TargetDestroyed()
	{
		killsThisBattle ++;
		totalKills ++;
		//if the target was attacking me, it's removed from the message sent to AICommander script by the target's Health script

		target = null;
		CancelInvoke("LeaveThemAlone");

		if(currentState != StateMachine.Covering)
			target = SelectAnEnemyAttackingMe (myAttackers);

		if(target != null)
		{
			target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
		}
		else if(target == null && currentState == StateMachine.Covering)
		{
			return;
		}
		else if(target == null)
		{
			myCommander.RequestOrders(this.GetComponent<AIFighter>());
		}
	}

	void PloughTheRoad()
	{
		if (healthScript.dead || inRetreatState || ClickToPlay.instance.paused)
			return;
		if (Vector2.Distance (transform.position, flightLeader.transform.position) > 7.5f) //so they don't fire while miles out of position
			return;

		StartCoroutine (ActuallyPloughTheRoad());
	}
	IEnumerator ActuallyPloughTheRoad()
	{
		yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));

		shootScript.FirePrimary (true);
	}
	void HUDPointerOn(float offTime)
	{
		CancelInvoke ();
		HUDPointer.SetActive (true);
		Invoke ("HUDPointerOff", offTime);
	}
	void HUDPointerOff()
	{
		if(HUDPointer != null)
			HUDPointer.SetActive (false);
	}
	void DefendYourself(GameObject theAttacker)
	{
		if(currentState != StateMachine.Retreat && currentState != StateMachine.Evade && currentState != StateMachine.FormUp 
			&& target != theAttacker && theAttacker != gameObject && !StaticTools.IsInLayerMask(theAttacker, friendlyFireMask))
		{
			if(!StaticTools.IsInLayerMask(theAttacker, LayerMask.GetMask("PMCCapital", "EnemyCapital")))
			{
				if(target)
					target.GetComponent<TargetableObject>().myAttackers.Remove(this.gameObject);
				
				target = theAttacker;
				target.GetComponent<TargetableObject>().myAttackers.Add(this.gameObject);
			}
		}
	}
	void LeaveThemAlone()
	{
		if(Tools.instance.CheckTargetIsLegit(target) && Tools.instance.CheckTargetIsRetreating(target, this.gameObject, "Leave Them Alone"))
		{
			target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver);
			target = null;
		}
	}

	void ReportActivity()
	{
		CameraTactical.reportedInfo = this.name + "\n";

		if(target != null)
			CameraTactical.reportedInfo += StaticTools.SplitCamelCase(currentState.ToString()) + " " + target.name;
		else 
			CameraTactical.reportedInfo += StaticTools.SplitCamelCase(currentState.ToString());

		CameraTactical.reportedInfo += "\n";

		if ((float)healthScript.health / healthScript.maxHealth < (0.33f)) 
			CameraTactical.reportedInfo += "Heavily Damaged";		
		else if (healthScript.health == healthScript.maxHealth) 
			CameraTactical.reportedInfo += "Fully Functional";		
		else
			CameraTactical.reportedInfo += "Damaged";
	}

	void ChooseGuardPoint() //used in Patrol
	{
		if(escortShip)
		{
			guardPoint = (Vector2)escortShip.transform.position;
		}
		else if (movingGuardPoint && flightLeader != null)
		{
			guardPoint = (Vector2)flightLeader.transform.position;
		}
		else if(movingGuardPoint && flightLeader == null)
			guardPoint = (Vector2)transform.position;
	}

	#endregion
}//Mono

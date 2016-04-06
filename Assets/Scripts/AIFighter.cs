using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AIFighter : FighterFunctions {

	[HideInInspector]public HealthFighter healthScript;
	[HideInInspector]public EnginesFighter engineScript;
	[HideInInspector]public WeaponsPrimaryFighter shootScript;
	[HideInInspector]public WeaponsSecondaryFighter missilesScript;
	[HideInInspector]public Character myCharacterAvatarScript;

	public enum Orders {FighterSuperiority, Patrol, Escort, Wingman, RTB, FullRetreat, NA}; //set by commander //TODO: not used yet
	public Orders orders;

	public enum StateMachine {Patroling, Dogfight, Evade, Retreat, FormUp, Covering, Docking, NA};
	public StateMachine currentState;
	public StateMachine formerState;
	[HideInInspector] public StateMachine[] normalStates;
	[HideInInspector] public StateMachine[] combatStates;
	[HideInInspector] public StateMachine[] formUpStates;
	[HideInInspector] public StateMachine[] coverMeStates;
	[HideInInspector] public StateMachine[] retreatStates;
	[HideInInspector] public StateMachine[] deathStates;

	bool switchingStates = true;

	public GameObject target;
	public GameObject formerTarget;
	public GameObject flightLeader;
	[HideInInspector] public SquadronLeader flightLeadSquadronScript;
	[HideInInspector] public Rigidbody2D flightLeaderRigidbody;

	public enum SquadronMembership {one, two, three}; //these are set by Squadron Leader script
	[HideInInspector] public SquadronMembership squadronMembership;

	[HideInInspector] public GameObject myFormationPosition;
	[HideInInspector] public int mySquadUnitNumber = 0;

	public Vector2 patrolPoint;
	public Vector2 guardPoint;
	public bool movingGuardPoint = false;
	public float guardDistance = 20;
	public float mySensorRadius = 45;
	public LayerMask enemyTargets;
	[Tooltip("Any layers that could do this fighter harm. Fighter will stay away if retreating.")] 
	public LayerMask dangerSources;
	public Vector2 evadePosition;
	public Vector2 retreatPosition;

	float timer = 0;
	float nextTime = 0;

	//[Tooltip("If health is below this proportion of maxHealth, they'll evade & retreat. Is randomised away from this base number at Start")]
	//public float cowardice = 50;
	public bool inRetreatState = false;

	public GameObject HUDPointer;
	[HideInInspector] public GameObject dockingWith;
	

	void Awake()
	{
		healthScript = GetComponent<HealthFighter> ();
		engineScript = GetComponent<EnginesFighter> ();
		shootScript = GetComponentInChildren<WeaponsPrimaryFighter> ();
		missilesScript = GetComponentInChildren<WeaponsSecondaryFighter> ();

		myRigidbody = GetComponent<Rigidbody2D> ();

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
		myCommander.myFighters.Add (this.gameObject);

		normalStates = new StateMachine[]{StateMachine.Patroling};
		combatStates = new StateMachine[]{StateMachine.Dogfight};
		formUpStates = new StateMachine[]{StateMachine.FormUp};
		coverMeStates = new StateMachine[]{StateMachine.Covering};
		retreatStates = new StateMachine[] {StateMachine.Evade, StateMachine.Retreat};
		deathStates = new StateMachine[] {StateMachine.NA};

		if(GetComponentInChildren<Character>())
		{
			myCharacterAvatarScript = GetComponentInChildren<Character>();
			myCharacterAvatarScript.name += " " + this.gameObject.name;
			myCharacterAvatarScript.transform.SetParent(null); //detach from this so it's not always doing position updates with movement
		}
	}

	void Start () 
	{
		//cowardice = Mathf.Clamp(cowardice *= Random.Range (0.25f, 1.5f), 0, 99); //TODO: based on character stats for PMC?

		StartCoroutine(SetUpAvatarBars());
	}

	IEnumerator SetUpAvatarBars()
	{
		while(flightLeader == null)
			yield return new WaitForEndOfFrame();
		
		if(flightLeader != null && flightLeader.tag == "PlayerFighter")
		{
			healthScript.avatarAwarenessBars = myCharacterAvatarScript.avatarOutput.transform.FindChild("Awareness Panel");
			healthScript.avatarHealthBars = myCharacterAvatarScript.avatarOutput.transform.FindChild("HP Panel");
			healthScript.SetUpAvatarBars();
		}
	}

	public void SetSquadReferences()
	{
		flightLeadSquadronScript = flightLeader.GetComponentInChildren<SquadronLeader> ();
		flightLeaderRigidbody = flightLeader.GetComponent<Rigidbody2D>();

		if(squadronMembership == SquadronMembership.two)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPosLeft;
			mySquadUnitNumber = 2;
		}
		else if(squadronMembership == SquadronMembership.three)
		{
			myFormationPosition = flightLeadSquadronScript.wingmanPosRight;
			mySquadUnitNumber = 3;
		}
	}
	

	void Update () 
	{	
		#region To Manage Retreating	
		/*
		//if we get hit, go evasive if health is lower than cowardice rating
		if(currentState != StateMachine.Docking && !inRetreatState && !healthScript.dead && (healthScript.health/ healthScript.maxHealth * 100) <= cowardice)
		{
			if(healthScript.lastDamageWasFromAsteroid)
			{ 
				//do nothing
			}
			else
			{
				ChangeToNewState(retreatStates, new float[]{2,1});
				inRetreatState = true;

				if(LayerMask.LayerToName(gameObject.layer) == "PMCFighters")
				{
					Subtitles.instance.PostSubtitle(new string[]{"This is " +this.name + ". I'm hit! Breaking off!", 
						"This is " +this.name + ". I could use some cover!"});
					HUDPointerOn(4);
					_battleEventManager.instance.CallWingmanInTrouble();

					myCharacterAvatarScript.StartCoroutine("Speaking");
				}
			}
		}
		//if we were retreating and health goes up, return to battle
		else if(inRetreatState && orders != Orders.FullRetreat && (healthScript.health * 100 / healthScript.maxHealth) > cowardice
		        &&(healthScript.health  > healthScript.maxHealth * 3/4))
		{
			//return to last state (and re-establish initial parameters for it)
			switchingStates = true;
			inRetreatState = false;
			healthScript.rollSkill = healthScript.normalRollSkill;
			currentState = formerState;
			target = (CheckTargetIsLegit(formerTarget) == true)? formerTarget: null; //check if old target is still alive

			if(LayerMask.LayerToName(gameObject.layer) == "PMCFighters")
			{
				Subtitles.instance.PostSubtitle(new string[]{"This is " +this.name + ", returning to the fight!",
				this.name + " back on-station!",
				"This is " + this.name + ", damage under control. I'm good to go!"});
				HUDPointerOn(4);
				_battleEventManager.instance.CallWingmanBack();

				myCharacterAvatarScript.StartCoroutine("Speaking");
			}
		}*/
		#endregion

		if(currentState == StateMachine.Dogfight)
		{
			Dogfight();
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
		healthScript.rollSkill = healthScript.normalRollSkill;
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
	void Dogfight()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;
			target = myCommander.ClosestTarget(myCommander.knownEnemyFighters, transform.position);

			if(target == null)
				target = myCommander.ClosestTarget(myCommander.knownEnemyTurrets, transform.position);

			if(target == null)
			{
				float[] weights = new float[]{1};
				ChangeToNewState(normalStates, weights);
				return;
			}
			if(target != null)
			{
				if(!CheckTargetIsLegit(target))
				{
					Debug.Log("ERROR: "+ gameObject.name +" CHOSE INACTIVE TARGET ON DOGFIGHT");
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
			Debug.Log("target is null and " + this.name +" was dogfighting ");
			float[] weights = new float[]{1};
			ChangeToNewState(normalStates, weights);
			return;
		}
		if (CheckTargetIsLegit (target) == true) 
		{
			DogfightingFunction (engineScript, target, shootScript, constantThrustProportion);
		} 
		else 
		{
			target = null;
		}
	}

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
			GameObject targetCheck = CheckLocaleForTargets(transform.position, mySensorRadius, enemyTargets);
			if(targetCheck != null)
			{
				CheckAndAddTargetToCommanderList(myCommander, targetCheck);
				myCommander.RequestOrders(GetComponent<AIFighter>());
				return;
			}
		}

		if(Vector2.Distance(transform.position, patrolPoint) < 5)
		{
			if (movingGuardPoint && flightLeader != null)
			{
				guardPoint = (Vector2)flightLeader.transform.position;
			}
			else if(movingGuardPoint && flightLeader == null)
				guardPoint = (Vector2)transform.position;

			patrolPoint = PatrolPoint(guardPoint, guardDistance);
		}
		engineScript.LookAtTarget (patrolPoint);

		if(Vector2.Angle(transform.up, patrolPoint - (Vector2)transform.position) < 10)
			engineScript.MoveToTarget (patrolPoint, false);
	}


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
	}


	void Covering()
	{
		//NOTE: Part of the target finding functionality is in the TargetDestroyed() method.
		if(switchingStates)
		{
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
					targetCheck = myCommander.ClosestTarget(flightLeader.GetComponent<PlayerAILogic>().myAttackers, transform.position);
				else
					targetCheck = myCommander.ClosestTarget(flightLeader.GetComponent<AIFighter>().myAttackers, transform.position);

				if(targetCheck != null)
				{
					if(!CheckTargetIsLegit(targetCheck) || CheckTargetIsRetreating(targetCheck))
					{
						Debug.Log(gameObject.name +" CHOSE INACTIVE TARGET FROM LEADER'S ATTACKERS");
						RemoveBadTargetFromLeadersAttackers(flightLeader, targetCheck);
						return;
					}
					if(Vector2.Distance(flightLeader.transform.position, targetCheck.transform.position) < guardDistance)
					{
						target = targetCheck;
						target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
					}
				}
				else //targetCheck is null
				{
					GameObject localTarget = CheckLocaleForTargets(transform.position, guardDistance, enemyTargets);
					if(localTarget != null)
					{
						if(!CheckTargetIsLegit(localTarget))
						{
							Debug.Log("SCANNED LOCALITY FOR A TARGET AND CHOSE INACTIVE ONE");
							return;
						}
						else if(CheckTargetIsRetreating(localTarget))
						{
							return;
						}
						CheckAndAddTargetToCommanderList(myCommander, localTarget);


						target = localTarget;
						target.SendMessage("AddSomeoneAttackingMe", this.gameObject);
					}
				}
			}//end of 1 second check
		}//end of if(target == null)

		else if(target != null)
		{
			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;

			if(CheckTargetIsLegit(target) == true)
			{
				DogfightingFunction (engineScript, target, shootScript, constantThrustProportion); //WARNING: This function can result in target being set to null
			}
			//This section should be excessive, but is still the only way to stop craft targeting dead enemies and shooting at their death spots
			else
			{
				Debug.Log(gameObject.name + " is setting " + target.name + " to null. NOT LEGIT");
				target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); 
				target = null;
				return;
			}
			if(target != null && CheckTargetIsRetreating(target))
			{
				Invoke("LeaveThemAlone", 2);
				return;
			}
		}
	}//end of COVERING


	void Evade()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;

			healthScript.rollSkill = healthScript.normalRollSkill * 1.5f;
			timer = 1;
			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;
			
			switchingStates = false;
		}
		timer += Time.deltaTime;

		if(timer >= nextTime)
		{
			timer = 0;
			nextTime = Random.Range (1f, 2.5f);

			if(CheckLocaleForTargets(transform.position, mySensorRadius, dangerSources) == null)
			{
				evadePosition = PatrolPoint(transform.position, guardDistance);
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
	}//end of Evade


	void Retreat()
	{
		if(switchingStates)
		{
			shootScript.enabled = true;
			
			healthScript.rollSkill = healthScript.normalRollSkill * 1.5f;
			timer = 1.5f;
			engineScript.currentMaxVelocityAllowed = engineScript.maxAfterburnerVelocity;
			
			switchingStates = false;
		}
		timer += Time.deltaTime;
		
		if(timer >= nextTime)
		{
			timer = 0;
			nextTime = Random.Range (0.5f, 1.5f);

			if(CheckLocaleForTargets(transform.position, mySensorRadius, dangerSources) == null)
			{
				evadePosition = PatrolPoint(transform.position, guardDistance);
			}
			else
				evadePosition = ChooseRetreatPosition(myRigidbody, potshotAtEnemiesMask, enemyCommander);
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
	}//end of RETREAT



	//MECHANICAL-TYPE FUNCTIONS BELOW HERE
	
	void TargetDestroyed()
	{
		killsThisBattle ++;
		totalKills ++;
		//if the target was attacking me, it's removed from the message sent to AICommander script by the target's Health script

		target = null;

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
		   && target != theAttacker)
		{
			if(!Tools.instance.IsInLayerMask(theAttacker, LayerMask.GetMask("PMCCapital", "EnemyCapital")))
				target = theAttacker;
		}
	}
	void LeaveThemAlone()
	{
		if(CheckTargetIsLegit(target) && CheckTargetIsRetreating(target))
		{
			target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); 
			target = null;
		}
	}

	void ReportActivity()
	{
		CameraTactical.reportedInfo = this.name + "\n";

		if(target != null)
			CameraTactical.reportedInfo += this.currentState.ToString() + " " + target.name;
		else 
			CameraTactical.reportedInfo += this.currentState.ToString();

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
	#endregion
}//Mono

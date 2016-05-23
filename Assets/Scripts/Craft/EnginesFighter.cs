using UnityEngine;
using System.Collections;


public class EnginesFighter : MonoBehaviour {

	[HideInInspector] public Rigidbody2D myRigidBody;

	Vector2 targetLook; 
	[HideInInspector]public Vector2 targetMove;
	Vector2 newTargetPosition;

	//for MOVING
	public float startSpeed = 0;
	public float normalAccelerationRate = 300;
	[HideInInspector] public float currentAccelerationRate;
	[Range(1, 5)]
	public float afterburnerMultiplier = 1;
	public float reverseMultiplier = 0.66f; //on the Arrows, I allow each of 4 reversing thrustsers a value of 1 power, and the rear engines
	//have a value of 3 each, totalling 6 power. 6 forward versus 4 reverse. 4/6 = 0.66f
	public float lateralThrustMultiplier = 0.66f; //there's visually only two thrusters on either side, on the Arrows, pretending there's 4
	protected float mySpeed;
	protected bool stillHaveAfterburnMomentum = false;


	public bool hasAfterburner = true;
	[Tooltip("Ship lengths (Unity Units) per second. Speedometer will read m/sec, which is 10 times this amount.")] 
	public float maxNormalVelocity = 7f;
	[Tooltip("Ship lengths (Unity Units) per second. Speedometer will read m/sec, which is 10 times this amount.")] 
	public float maxAfterburnerVelocity = 12f;
	public int maxNitro = 600;
	[HideInInspector] public float nitroRemaining;
	[Tooltip("Per second.")] public float nitroBurnRate = 10;

	[HideInInspector] public float currentMaxVelocityAllowed;
	float speed;
	[HideInInspector] public Vector2 newMovementPosition;
	//float powerToMainEngines;
	//float angleToMovePos;

	//for ROTATION
	[Tooltip("Degrees per second")]
	public float maxRotateSpeed = 144;
	[HideInInspector] public float turnSpeed;


	//for FIRING SOLUTION
	float shooterToTargetDistance;
	float bulletToTargetTime;
	[Tooltip("How fast does my ammo normally travel? For firing solution.")] public float shotSpeedAvg = 19f;

	//for DOCKING MANOEUVRES
	[HideInInspector] public bool linedUpToDock = false;
	[HideInInspector] public bool securedToDock = false;

	[Header ("Animation/Effects Stuff")]
	public AudioSource thrusterAudio;
	public AudioSource engineNoise;
	public AudioSource afterburnerNoise;
	[Tooltip("Use true for fighters with variable acceleration, but not really for transports. Depends on setup.")]
	public bool myAudioPitchSouldScaleForEngines = true;

	[Header("Thruster animation arrays")]
	[Tooltip("Place Thrusters in here. Checks for audio purposes. Ignore engines.")]
	public Thruster[] allThruster;
	[Tooltip("In each slot, put in all thrusters that should be on when moving a certain direction.")]
	public Thruster[] toRotateLeft;
	public Thruster[] toRotateRight;
	public Thruster[] toStrafeLeft;
	public Thruster[] toStrafeRight;
	public Thruster[] toReverse;
	float angleToDestination;
	bool doAnimations;

	public SpriteRenderer engine1;
	public SpriteRenderer engine2;

	[HideInInspector] public float previousThrustValue = 0;
	public float rearEngineVisualSize = 3;
	float smoothedAccelerationInput;
	float smoothedRotationalInput;
	[HideInInspector] public bool afterburnerIsOn = false;
	[HideInInspector] public bool braking = false;

	protected Vector2 previousLookRotation; //used for StickPoints controls
	protected Vector2 currentLookRotation;
	float previousSign; //used for turning left/right animations


	void Awake () 
	{
		myRigidBody = GetComponent<Rigidbody2D> ();
	}

	void Start()
	{
		currentMaxVelocityAllowed = maxNormalVelocity;
		myRigidBody.velocity = transform.up * startSpeed;
		currentAccelerationRate = normalAccelerationRate;
	}
	

	void Update () //doesn't run from a derived class, only if this script is directly on the GameObject
	{		
		//FOR CAPPING SPEED

		speed = myRigidBody.velocity.magnitude;
		
		if(speed >= currentMaxVelocityAllowed)
		{
			myRigidBody.velocity = Vector3.Normalize(myRigidBody.velocity) * currentMaxVelocityAllowed;
		}
		
	} // end of Update

	void LateUpdate()
	{
		CheckAllThrusters();
		CheckThrusterAudio();
	}

	
	public void MoveToTarget (GameObject target, float constantForwardThrustProportion)
	{
		if (target == null || ClickToPlay.instance.paused)
			return;

		if (target.tag == "Fighter" || target.tag == "PlayerFighter") 
		{
			targetMove = target.transform.FindChild("Craft's Six").position;
			GetMovementSolution (target, targetMove, true, false);
		}
		else if(target.tag == "Turret")
		{
			targetMove = target.transform.FindChild("Craft's Six").position;
			GetMovementSolution (target, targetMove, false, false);
		}
		else if(target.tag == "Debris")
		{
			GetComponent<AIFighter>().target = null;
		}
		else //like for formations
		{
			targetMove = target.transform.position;
			GetMovementSolution(target, targetMove, false, false);
		}

		//New complex equation in this function
		ApplyThrust(newMovementPosition);

		//TODO: add attack patterns that send fighters away from their target at different rates
		//the higher this force, the less ships will stop and turn on the spot. They'll have wider turning circles
		//myRigidBody.AddForce (transform.up * currentAccelerationRate * Time.deltaTime * constantForwardThrustProportion);

	}
	public void MoveToTarget(Vector2 waypoint, bool stopAtWaypoint)
	{
		if (ClickToPlay.instance.paused)
			return;
		
		GetMovementSolution (null, waypoint, false, stopAtWaypoint); //this sets a newMovementPosition vector

		ApplyThrust (newMovementPosition);
	}

	protected void ApplyThrust(Vector2 towardsWhere)
	{
		//NOTE: towardsWhere is a worldspace co-ordinate, not local
		//Debug.DrawLine((Vector2)transform.position, towardsWhere, Color.cyan, 1);

		//this will stop animations firing constantly when in formation making minor adjustments
		if(Vector2.Distance(transform.position, towardsWhere) > 0.15f)
		{
			angleToDestination = Vector2.Angle(transform.up, towardsWhere - (Vector2)transform.position);
			if(angleToDestination > 10 && angleToDestination < 170)
				doAnimations = true;
			else
			{
				doAnimations = false;
			}
		}
		else 
			doAnimations = false;

		if(hasAfterburner && !afterburnerIsOn)
			afterburnerNoise.Stop();
		else if(afterburnerIsOn && !afterburnerNoise.isPlaying)
			afterburnerNoise.Play();

		Vector2 dirInLocalTerms = transform.InverseTransformDirection(towardsWhere - (Vector2)transform.position);

		//Debug.DrawLine (transform.position, transform.position + transform.TransformVector (dirInLocalTerms), Color.red);

		//if we're very almost where we want to be, set to zero so more thrust isn't added
		if(Mathf.Abs(dirInLocalTerms.y) < 0.1f)
		{
			dirInLocalTerms.y = 0;
			ForwardOrBackwardThrust(0, doAnimations);
		}
		if(Mathf.Abs(dirInLocalTerms.x) < 0.1f)
		{
			dirInLocalTerms.x = 0;
		}

		//if we were basically stopped in both directions, really stop, and return
		if(dirInLocalTerms.x ==0 && dirInLocalTerms.y ==0)
		{
			//add no forces
			return;
		}

		//otherwise..
		//First see if reverse Thrust is needed
		if(dirInLocalTerms.y < 0)
		{
			ForwardOrBackwardThrust(-reverseMultiplier, doAnimations);
		}

		//Then see if we need lateral thrust, first if want to go left, then right
		if(dirInLocalTerms.x < 0) //if we want to go left
		{
			LateralThrust(-1, doAnimations); //fire right side thrusters to send us left
		}
		else if(dirInLocalTerms.x > 0)
		{
			LateralThrust(1, doAnimations);
		}

		//Lastly, see if we should thrust forward
		if(dirInLocalTerms.y > 0)
		{
			ForwardOrBackwardThrust(1, doAnimations);
		}
		//myRigidBody.AddForce ((newMovementPosition - (Vector2)transform.position).normalized * currentAccelerationRate * Time.deltaTime);
	}
	
	
	void GetMovementSolution(GameObject target, Vector2 moveToWhere, bool hasRigidbody2D, bool stopAtWaypoint)
	{	
		if(hasRigidbody2D)
		{
			newMovementPosition = moveToWhere + (target.GetComponent<Rigidbody2D>().velocity *1) 
				- (myRigidBody.velocity *1);
			//AdjustForAfterburner(myRigidBody,target.GetComponent<Rigidbody2D>()); 
		}
		else if(target != null && target.tag == "FormationPosition")
		{
			newMovementPosition = moveToWhere + (target.transform.parent.parent.parent.GetComponent<Rigidbody2D>().velocity *1) 
				- (myRigidBody.velocity *1);
			//AdjustForAfterburner(myRigidBody,target.transform.parent.parent.parent.GetComponent<Rigidbody2D>()); 
		}
		else if(target != null && target.tag == "Turret")
		{
			newMovementPosition = moveToWhere + (target.transform.parent.parent.GetComponent<Rigidbody2D>().velocity *1) 
				- (myRigidBody.velocity *1);
			//AdjustForAfterburner(myRigidBody,target.transform.parent.parent.parent.GetComponent<Rigidbody2D>()); 
		}
		else if(target == null)
		{
			if(stopAtWaypoint)
				newMovementPosition = moveToWhere - (myRigidBody.velocity * myRigidBody.velocity.magnitude);
			else
				newMovementPosition = moveToWhere - myRigidBody.velocity;
			
		}
		else
		{ 
			//so this is a target with no rigidbody which isn't a formation position, which shouldn't happen
			newMovementPosition = moveToWhere;
			Debug.LogError ("ERROR: MOVE TO WHERE. "+ gameObject.name + " to " + target.name + "("+ target.tag +"). "+ hasRigidbody2D + " for RigidBody.");
			GetComponent<AIFighter>().target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); 
			GetComponent<AIFighter>().target = null;
		}
	}

	public void LookAtTarget(GameObject target)
	{
		if (target != null)
		{
			if (target.layer == LayerMask.NameToLayer("PMCFighters") || target.layer == LayerMask.NameToLayer("EnemyFighters")
			    || target.tag == "Turret")		
			{
				GetFiringSolution (target);
				targetLook = newTargetPosition;
			}
		}
		else if(targetMove != Vector2.zero)
		{
			targetLook = targetMove;
		}
		else 
			targetLook = transform.up;
		
		Vector3 dir = (Vector3)targetLook - transform.position; 
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

		float angleToTarget = Vector2.Angle(transform.up, target.transform.position - transform.position);

		if(!System.Single.IsNaN(angle) && !ClickToPlay.instance.paused)
		{
			Vector2 dirInLocalTerms = transform.InverseTransformDirection(target.transform.position - transform.position);
			TurnAnimation(dirInLocalTerms.normalized.x, angleToTarget);

			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, (Time.deltaTime * maxRotateSpeed));
		}
	}


	public void LookAtTarget(Vector2 waypoint)
	{
		Vector3 dir = (Vector3)waypoint - transform.position; 
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

		float angleToTarget = Vector2.Angle(transform.up, waypoint - (Vector2)transform.position);

		if(!System.Single.IsNaN(angle) && !ClickToPlay.instance.paused)
		{
			Vector2 dirInLocalTerms = transform.InverseTransformDirection(waypoint - (Vector2)transform.position);
			TurnAnimation(dirInLocalTerms.normalized.x, angleToTarget);

			//Debug.DrawLine(transform.position, waypoint, Color.red);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, (Time.deltaTime * maxRotateSpeed));
		}
	}

	protected void LookAt(Vector2 lookPos) //used by Player (control stick points), not AI. Local coord, not World
	{
		lookPos += (Vector2)transform.position; //becomes Worldspace coordinate

		Vector3 dir = (Vector3)lookPos - transform.position; 

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward); 

		float angleToTarget = Vector2.Angle(transform.up, lookPos - (Vector2)transform.position);

		if(!System.Single.IsNaN(angle))
		{
			Vector2 dirInLocalTerms = transform.InverseTransformDirection(lookPos - (Vector2)transform.position);

			if(Vector2.Angle(previousLookRotation, currentLookRotation) < 0.5f)
			{
				TurnAnimation(0, angleToTarget);			
			}
			else if(dirInLocalTerms.normalized.x < 0)
			{
				TurnAnimation(-1, angleToTarget);
			}
			else if(dirInLocalTerms.normalized.x > 0)
			{
				TurnAnimation(1, angleToTarget);
			}

			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, Time.deltaTime * turnSpeed);
		}
	}

	
	void GetFiringSolution(GameObject target)
	{
		if(target != null)
		{
			shooterToTargetDistance = (newTargetPosition - (Vector2)transform.position).magnitude;
			
			bulletToTargetTime = shooterToTargetDistance / shotSpeedAvg;
			
			if(System.Single.IsNaN(bulletToTargetTime))
			{
				newTargetPosition = new Vector2 (target.transform.position.x, target.transform.position.y);
				Debug.Log("Rotation Error Overcome by " + gameObject.name + " targeting " + target.name);
			}
			else
			{
				if(target.GetComponent<Rigidbody2D>())
				{
					newTargetPosition = (Vector2)target.transform.position + 
						(target.GetComponent<Rigidbody2D>().velocity*bulletToTargetTime) - 
							(myRigidBody.velocity*bulletToTargetTime ); //this was the ideal
				}
				else
				{
					newTargetPosition = (Vector2)target.transform.position + 
						(target.transform.parent.parent.GetComponent<Rigidbody2D>().velocity*bulletToTargetTime) 
							- (myRigidBody.velocity*bulletToTargetTime );
				}
			}
		}
		else
		{
			Debug.Log("ERROR: Target was NULL. Firing solution becomes (0,0)");
			newTargetPosition = Vector2.zero;
		}
	}


	void AdjustForAfterburner(Rigidbody2D shipRB, Rigidbody2D targetRB)
	{
		/*if (!hasAfterburner)
			return;

		float angle = Vector2.Angle (shipRB.velocity.normalized, targetRB.velocity.normalized);
		if(angle > 45)
		{
			currentAccelerationRate = normalAccelerationRate * 1.5f;
			print ("angle > 45, using afterburner");
			return;
		}
		else 
		{
			if(targetRB.velocity.magnitude > shipRB.velocity.magnitude / 1.25f)
			{
				currentAccelerationRate = normalAccelerationRate * 1.5f;
				print ("same direction, but target is faster, using afterburner");
				return;
			}
		}
		currentAccelerationRate = normalAccelerationRate;
		print ("normal acceleration");*/
		//currentAccelerationRate = normalAccelerationRate * 1.5f;
	}


	#region MOVEMENT FUNCTIONS

	protected void ForwardOrBackwardThrust(float direction, bool doAnimations)
	{
		EnginesEffect(direction, doAnimations);

		if(stillHaveAfterburnMomentum && hasAfterburner && !afterburnerIsOn)
		{
			if((myRigidBody.velocity + ((Vector2)transform.up * direction * currentAccelerationRate * Time.deltaTime)).magnitude 
				> mySpeed)
			{
				if(direction > 0)
				{
					//this artificially slows the current velocity so we can add a new one
					myRigidBody.AddForce (myRigidBody.velocity.normalized * -direction * currentAccelerationRate * Time.deltaTime);
				}
				else return;
			}
		}
		myRigidBody.AddForce (transform.up * direction * currentAccelerationRate * Time.deltaTime);
	}

	protected void SpaceBrake()
	{
		afterburnerNoise.Stop();

		Vector2 localVelocity = transform.InverseTransformDirection(myRigidBody.velocity);

		//if we're almost stopped in a certain direction, set to zero so more thrust isn't added
		if(Mathf.Abs(localVelocity.y) < 0.1f)
		{
			localVelocity.y = 0;
			ForwardOrBackwardThrust(0, false);
		}
		if(Mathf.Abs(localVelocity.x) < 0.1f)
		{
			localVelocity.x = 0;
		}

		//if we were basically stopped in both directions, really stop, and return
		if(localVelocity.x ==0 && localVelocity.y ==0)
		{
			myRigidBody.velocity = Vector2.zero;
			return;
		}

		//otherwise..
		//First see if reverse Thrust is needed
		if(localVelocity.y > 0)
		{
			ForwardOrBackwardThrust(-reverseMultiplier, true);
		}

		//Then see if we need lateral thrust, first right side firing (so, moving towards left), then left side firing

		if(localVelocity.x > 0) //if drifting right
		{
			LateralThrust(-1, true); //move us left
		}
		else if(localVelocity.x < 0)
		{
			LateralThrust(1, true);
		}

		//Lastly, see if we should thrust forward
		if(localVelocity.y < 0)
		{
			ForwardOrBackwardThrust(1, true);
		}
	}


	protected void RotationalThrust(float direction)
	{
		transform.Rotate (0.0f, 0.0f, turnSpeed * -direction * Time.deltaTime); //old way. no force

		#region Force-Based Turning
		//this didn't feel good, and rotate was technically way too fast for the engines but felt too slow

		/*if(myRigidBody.angularVelocity != 0 && direction == myRigidBody.angularVelocity) //then input was zero so we're adding counter thrust to stop the turn
		{
			print("no input. zeroing " + myRigidBody.angularVelocity);

			float forceToAdd = turnSpeed * Mathf.Sign(-direction) * Time.deltaTime;

			if(Mathf.Sign(myRigidBody.angularVelocity + forceToAdd) != Mathf.Sign(-direction)) //if we've gone through zero and past it, just stop
			{
				print("stopping");
				myRigidBody.angularVelocity = 0;
			}
			else
				myRigidBody.AddTorque(forceToAdd);			
		}
		else //adding force based on the stick
		{
			myRigidBody.AddTorque(turnSpeed * -direction * Time.deltaTime);
			print("some input");
		}

		//clamp the rotation to the max turn speed
		if(Mathf.Abs(myRigidBody.angularVelocity) > turnSpeed)
		{
			//this divides the velocity down to 1 or -1, depending on direction, then sets to max or negative max;
			myRigidBody.angularVelocity /= Mathf.Abs(myRigidBody.angularVelocity);
			myRigidBody.angularVelocity *= maxRotateSpeed; 
		}	*/
		#endregion
	}


	protected void LateralThrust(float direction, bool doAnimations)
	{
		//so we can't increase overall speed if above normal and not holding afterburners, but we can decrease
		if(stillHaveAfterburnMomentum && !afterburnerIsOn) 
		{
			if((myRigidBody.velocity + 
				((Vector2)transform.right * normalAccelerationRate*lateralThrustMultiplier * direction * Time.deltaTime)).magnitude > mySpeed)
			{
				myRigidBody.AddForce (-myRigidBody.velocity.normalized * normalAccelerationRate*lateralThrustMultiplier * Time.deltaTime);
			}
		}
		myRigidBody.AddForce(transform.right * normalAccelerationRate*lateralThrustMultiplier * direction * Time.deltaTime);

		if(!doAnimations)
			return;

		if(direction > 0)
		{
			TurnOnThrusterGroup(toStrafeRight);
			//if(name == "SENTINEL 1")
			//	print("strafe right");
		}
		else if(direction < 0)
		{
			TurnOnThrusterGroup(toStrafeLeft);
		//	if(name == "SENTINEL 1")
			//	print("strafe left");
		}
	}
	#endregion




	#region Engine and Thruster visuals

	public void TurnAnimation(float axisValue, float angle)
	{
		if(Mathf.Abs(axisValue) > 1)
			Debug.LogError("axisValue was greater than 1: " + axisValue);


		previousSign = Mathf.Sign(previousThrustValue); //so we know which side of zero we're coming from and don't shoot past it..
																//when resetting to 0 for Player

		//first, smooth out the input so controller, keyboard, or AI can all graduate the engines nicely, visually
		if(Mathf.Abs(axisValue) == 1)
		{
			smoothedRotationalInput += Mathf.Sign(axisValue) * Time.deltaTime / 0.33f;
		}
		else if (!Mathf.Approximately (axisValue, 0)) //most likely a controller is being used
		{
			smoothedRotationalInput = axisValue; 
		}
		else //It's zero so bring it towards zero gradually
		{
			smoothedRotationalInput -= Mathf.Sign(smoothedRotationalInput) * Time.deltaTime / 0.33f;
			if(Mathf.Sign(smoothedRotationalInput) != Mathf.Sign(previousSign))
			{
				smoothedRotationalInput = 0;
			}
		}
		smoothedRotationalInput = Mathf.Clamp(smoothedRotationalInput, -1, 1);

		if(Mathf.Abs(smoothedRotationalInput) < 0.01f && !braking)	
			//this way if you hold a direction, you only get the burst at the start. It's accurate but maybe ugly
			// 'smoothedRotationalInput' and '0' will give the two different behaviours if swapped out
		{
			//do nothing
		}
		else if(smoothedRotationalInput < previousThrustValue)
		{
			if(angle > 20)
				TurnOnThrusterGroup(toRotateLeft);
		}
		else if(smoothedRotationalInput > previousThrustValue)
		{
			if(angle >20)	
				TurnOnThrusterGroup(toRotateRight);
		}
		previousThrustValue = smoothedRotationalInput;
	}



	public void EnginesEffect(float inputAmount, bool doAnimations)
	{
		//for engine effect. grows with acceleration

		//first, smooth out the input so controller, keyboard, or AI can all graduate the engines nicely, visually
		if(inputAmount == 1)
			smoothedAccelerationInput += inputAmount * 0.33f;
		else if (inputAmount > 0)
			smoothedAccelerationInput = inputAmount;
		else
			smoothedAccelerationInput -= Time.deltaTime / 0.33f;

		smoothedAccelerationInput = Mathf.Clamp01(smoothedAccelerationInput);


		//first we adjust the engine scale. This always needs to animate, so we haven't checked the bool yet

		Vector3 engineScale = engine1.transform.localScale;
		engineScale.x = rearEngineVisualSize; //just resetting to normal so it doesn't break after afterburning before
		engineScale.y = smoothedAccelerationInput * rearEngineVisualSize;
		engineScale.z = 1;
		if(myAudioPitchSouldScaleForEngines)
			engineNoise.pitch = 0.2f + (0.7f * smoothedAccelerationInput);

		if(afterburnerIsOn) //increase the length and width of the effect
		{
			engineScale.x *= afterburnerMultiplier;
			engineScale.y *= afterburnerMultiplier;
		}
		if(securedToDock) //if we're docked, make sure the engines look off
			engineScale.y = 0;

		engine1.transform.localScale = engineScale;
		engine2.transform.localScale = engineScale;

		if(!doAnimations) //if the movement point was too close, we might not want to animate, so skip what comes next
			return;

		//if we're actually reversing, do reverse animation
		if(inputAmount < 0)
		{
			TurnOnThrusterGroup(toReverse);
			if(myAudioPitchSouldScaleForEngines)
				engineNoise.pitch = 0.2f;
		}
	}

	public void TurnOnThrusterGroup (Thruster[] whichGroup)
	{
		/*if(whichGroup == toStrafeLeft || whichGroup == toStrafeRight || whichGroup == toReverse)
			return;*/
		if(whichGroup.Length == 0)
			return;

		for(int i = 0; i < whichGroup.Length; i++)
		{
			whichGroup[i].myRenderer.enabled = true;
			whichGroup[i].lastTurnedOnTime = Time.time;
		}
	}

	public void CheckAllThrusters()
	{
		for(int i = 0; i < allThruster.Length; i++)
		{
			if(allThruster[i].myRenderer.enabled)
			{
				if(Time.time > allThruster[i].lastTurnedOnTime + allThruster[i].minimumFireTime)
				{
					allThruster[i].myRenderer.enabled = false;
				}
			}				
		}
	}

	public void CheckThrusterAudio()
	{
		for(int i = 0; i < allThruster.Length; i++) //check all thruster sprites
		{
			if(allThruster[i].myRenderer.enabled) //if a single one is on, start or continue playing audio
			{
				if(!thrusterAudio.isPlaying)
				{
					thrusterAudio.Play();
				}
				return;
			}
		}
		thrusterAudio.Stop(); //we only get here if none of the sprites were on. In which case, make sure audio isn't playing
	}

	#endregion


}//Mono

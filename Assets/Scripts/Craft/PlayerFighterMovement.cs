using UnityEngine;
using UnityEngine.UI;

public class PlayerFighterMovement : EnginesFighter {

	Text playerSpeedText;

	
	void Awake()
	{
		myRigidBody = GetComponent<Rigidbody2D> ();
		try
		{
			playerSpeedText = GameObject.Find ("GUI PlayerSpeed").GetComponent<Text> ();
		}
		catch
		{
			//Debug.Log("GUI Player Speed Text missing for Player");
		}

		turnSpeed = maxRotateSpeed;
		currentAccelerationRate = normalAccelerationRate;
		currentMaxVelocityAllowed = maxNormalVelocity;
	}

	void Start()
	{
		PlayerPrefsManager.SetControllerStickBehaviourKey (PlayerPrefsManager.GetControllerStickBehaviourKey());

		if(Time.time < 1) //if it's after a second the speed will have been set by a cutscene or hangar launch or something else
		{
			myRigidBody.velocity = transform.up * startSpeed;	
		}
		else if(myRigidBody.velocity.magnitude > maxNormalVelocity)
		{
			currentMaxVelocityAllowed = myRigidBody.velocity.magnitude;
			stillHaveAfterburnMomentum = true;
		}

		nitroRemaining = maxNitro;

		Tools.instance.nitroRemainingSlider.maxValue = maxNitro;
		UpdateNitroHUDElements();
	}

	void Update()
	{
		//FOR NOISES. Engine pitch is set in EngineEffect()
		if(!ClickToPlay.instance.paused)
		{
			if(!engineNoise.gameObject.activeSelf)
				engineNoise.gameObject.SetActive(true);

			if (Input.GetButton ("Afterburners") && !afterburnerNoise.isPlaying /*&& Input.GetAxis("Accelerate") != 0 */ && nitroRemaining > 0)
				afterburnerNoise.Play ();
			else if(!Input.GetButton("Afterburners")/* || Input.GetAxis("Accelerate") == 0*/ || nitroRemaining <= 0)
				afterburnerNoise.Stop ();
		}
		else
			engineNoise.gameObject.SetActive(false);

		if (ClickToPlay.instance.paused)
			return;


		//FOR CAPPING PLAYER SPEED
		mySpeed = myRigidBody.velocity.magnitude;

		if(!Input.GetButton("Afterburners") && !stillHaveAfterburnMomentum)
		{
			currentMaxVelocityAllowed = maxNormalVelocity;
		}
		else if(!Input.GetButton("Afterburners") && stillHaveAfterburnMomentum)
		{
			if(mySpeed <= maxNormalVelocity)
			{
				currentMaxVelocityAllowed = maxNormalVelocity;
				stillHaveAfterburnMomentum = false;
			}
			else
			{	
				currentMaxVelocityAllowed = mySpeed;
			}
		}	
		//set the velocity to the max allowed
		if(mySpeed >= currentMaxVelocityAllowed)
		{
			myRigidBody.velocity = Vector3.Normalize(myRigidBody.velocity) * currentMaxVelocityAllowed;
		}

		//GUI
		if(playerSpeedText != null)
		{
			mySpeed *= 10;
			mySpeed = (mySpeed > 65 && mySpeed < 75) ? 70 : mySpeed;
			mySpeed = (mySpeed > 115 && mySpeed < 125) ? 120 : mySpeed;
			playerSpeedText.text = (mySpeed <10)? "Speed: 0": "Speed: ";
			playerSpeedText.text += (int)mySpeed;
			mySpeed /= 10;
		}
		
		//FOR BASIC MOVEMENT
		braking = false;

		//for braking
		if(Input.GetAxis("Accelerate") > 0 && Input.GetAxis("Reverse") >0)
		{
			braking = true;
			SpaceBrake();
		}
		//normal acceleration
		else if(Input.GetAxis("Accelerate")>0)
		{
			ForwardOrBackwardThrust(Input.GetAxis("Accelerate"), true);
		}
		else if(Input.GetButton("Afterburners")) // added afterburners here to allow them on without accelerator button
		{
			ForwardOrBackwardThrust(1, true);
		}
		else if(Input.GetAxis("Reverse")>0) //reversing
		{
			//this is where the forward thrust will be put backwards and reduced			
			ForwardOrBackwardThrust(Input.GetAxis("Reverse") * -reverseMultiplier, true);
		}
		else 
		{
			EnginesEffect(0, true);
		}

		//FOR ROTATION

		if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse ||
			PlayerPrefsManager.leftGamepadStickBehaviour == PlayerPrefsManager.LeftGamepadStickBehaviour.stickRotates)
		{
			if (!ClickToPlay.instance.paused)
			{
				RotationalThrust(Input.GetAxis("Horizontal"));
				//animations called further down in this update method
			}
			else if(!ClickToPlay.instance.paused)
			{
				RotationalThrust(myRigidBody.angularVelocity);
			}
		}
		else if(PlayerPrefsManager.leftGamepadStickBehaviour == PlayerPrefsManager.LeftGamepadStickBehaviour.stickPoints)
		{
			if(!ClickToPlay.instance.paused)
			{
				currentLookRotation = new Vector2 (Input.GetAxis ("Horizontal"), -Input.GetAxis ("Vertical"));
				if(currentLookRotation == Vector2.zero)
					currentLookRotation = transform.up;
				
				LookAt(currentLookRotation);

				previousLookRotation = currentLookRotation;
			}
		}

		//FOR AFTERBURNERS
	
		if(!braking && Input.GetButton("Afterburners") && nitroRemaining > 0 /*&& Input.GetAxis("Accelerate") > 0*/)
		{
			Tools.instance.VibrateController(0, 0.1f, 0.1f, 0.1f);
			afterburnerIsOn = true;

			nitroRemaining -= nitroBurnRate * Time.deltaTime;
			UpdateNitroHUDElements();

			currentMaxVelocityAllowed = maxAfterburnerVelocity;
			currentAccelerationRate = normalAccelerationRate * afterburnerMultiplier;

			if(mySpeed > maxNormalVelocity)
			{
				stillHaveAfterburnMomentum = true;
			}
		}
		else 
		{
			afterburnerIsOn = false;
			currentAccelerationRate = normalAccelerationRate;
		}


		//FOR TURN RESTRICTION
		
		if(Input.GetAxis("Accelerate") != 0 || Input.GetAxis("Reverse") !=0)
		{
			if(!Input.GetButton("Afterburners"))
				turnSpeed = maxRotateSpeed / 2f;
			else
				turnSpeed = maxRotateSpeed / 4f;
		}
		else if(Input.GetButton("Afterburners"))
		{
			turnSpeed = maxRotateSpeed / 4f;
		}		
		else if(Input.GetAxis("Accelerate") == 0 && !Input.GetButton("Afterburners"))
		{
			turnSpeed = maxRotateSpeed;
		}

		if(PlayerPrefsManager.leftGamepadStickBehaviour != PlayerPrefsManager.LeftGamepadStickBehaviour.stickPoints)
			TurnAnimation(Input.GetAxis("Horizontal"), 999); //WARNING: moving this out of order has unwanted behaviour

		//FOR STRAFING
		if(!braking)
		{			
			if (Input.GetButton("StrafeLeft"))
			{				
				LateralThrust(-1, true);
			}

			if (Input.GetButton("StrafeRight"))
			{
				LateralThrust(1, true);
			}
		}

		CheckAllThrusters();
		CheckThrusterAudio(); //measures if any thrusters are on at the end of the frame and makes sure to play audio if one is on.

	} //End of Update
		

	public void UpdateNitroHUDElements()
	{
		Tools.instance.nitroRemainingSlider.value = nitroRemaining;
		Tools.instance.nitroRemainingText.text = "NITRO: "  + "<color>" + (int)nitroRemaining + " / " + maxNitro + "</color>";
	}

}//Mono

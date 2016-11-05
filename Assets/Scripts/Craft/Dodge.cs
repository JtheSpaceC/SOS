using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class Dodge : MonoBehaviour
{
	PlayerFighterMovement playerMovementScript;
	EnginesFighter enginesFighterScript;
	AIFighter aiFighterScript;
	HealthFighter healthScript;
	[HideInInspector] public SpriteAnimator mySpriteAnimator;
	bool hasAvatar = false;

	public enum AnimationStyle {SpriteSheet, UnityAnimator, RollAModel};
	public AnimationStyle animationStyle;

	public bool playerControlled = false;
	public static bool playerIsDodging = false;

	[Tooltip("Set true unless you want player to have no auto-dodge")] public bool awarenessMechanicEnabled = true;
	[Tooltip("Set true if you want the one hit kills to happen on full mana")] public bool oneHitKillsMechanicEnabled = false;
	[HideInInspector] public bool playerActivatedManualDodge = false;
	[HideInInspector] public Image dodgeCooldownImage;
	[HideInInspector] public Text dodgeCooldownImageText;
	bool showRollCooldownImage = true;

	[HideInInspector] public bool dodging = false;
	public float rollDuration = 1.11f;
	float rollTime;
	float rollCooldown = 0f; 
	public float cooldownAmount = 1.11f; //used in Inspector to set how long the cooldown is. Stays constant. Player only
	[HideInInspector] public bool canDodge = true;
	[HideInInspector] public bool dodgeCoroutineStarted;
	float cantDodgeTime = 0.25f;

	private Animator animator;
	[Tooltip ("If there's extra items you want to rotate.")] public Transform[] alsoRotate;
	bool invertAnimationDirection = false; //to roll left or right
	Vector3 rotation;
	float rotY;
	float t; //time
	Vector3 newRot;

	public bool dodgingCostsNitro = false;


	//public Image awarenessManaFillImage;
	//[HideInInspector] public Image powerupReadyImage; //TODO: delete this?
	Image sliderFillImage;
	Color originalSliderColour;
	float startTime;
	AudioSource myAudioSource;
	AudioSource awarenessMeterAudioSource;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip manaDumpSound;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip manaIncreaseSound;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip manaFullSound;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip oneHitKillSound;

	float originalBarFill;
	float logoScale;

	[Header("Dodge Skills")]
	[Range (0, 100f)]
	public float frontDodgeSkill = 50f;
	[Range (0, 100f)]
	public float sideDodgeSkill	= 25f;
	[Range (0, 100f)]
	public float rearDodgeSkill = 0f;

	[Tooltip("Above chances multiplied by this number if attacked by missile. Default = 0.5f")]
	public float missileMultiplier = 0.5f;

	[Tooltip("Above chances multiplied by this number if dodging asteroid. Default = 2f")]
	public float asteroidMultiplier = 2f;

	[Header("Front/Side/Rear Definitions")]
	[Tooltip("What angle is defined as the front? Default 45. Cockpit visibility (fiction) should be considered.")]
	[Range (0, 90f)]
	public float frontAngle = 45f;
	[Tooltip("After the front, up to what remaining angle is defined as the side? Default 135. Rear is automatically this number up to 180")]
	[Range (45f, 170f)]
	public float sideAngle = 135f;

	float angle;
	float diceRoll;



	void Start()
	{
		animator = transform.parent.parent.GetComponent<Animator> ();
		mySpriteAnimator = transform.parent.parent.FindChild("Effects/Animation/roll (sprite swap)").GetComponent<SpriteAnimator>();
		rollTime = rollDuration;
		if(mySpriteAnimator)
			mySpriteAnimator.framesPerSecond = mySpriteAnimator.frames.Length/rollDuration;
		myAudioSource = GetComponent<AudioSource>();
		playerMovementScript = transform.parent.transform.parent.GetComponent<PlayerFighterMovement>();
		enginesFighterScript = transform.parent.transform.parent.GetComponent<EnginesFighter>();
		aiFighterScript = enginesFighterScript.GetComponent<AIFighter>();
		healthScript = transform.parent.transform.parent.GetComponent<HealthFighter>();

		if(aiFighterScript && aiFighterScript.myCharacterAvatarScript != null)
			hasAvatar = true;

		if(playerControlled)
		{			
			dodgeCooldownImage = GameObject.Find("Dodge Cooldown Image").GetComponent<Image>();
			dodgeCooldownImageText = dodgeCooldownImage.GetComponentInChildren<Text>();
			//awarenessManaFillImage = GameObject.Find("Awareness Image").GetComponent<Image>();
			awarenessMeterAudioSource = healthScript.awarenessSlider.GetComponent<AudioSource>();
			//powerupReadyImage = GameObject.Find("Powerup Image").GetComponent<Image>();

			if(awarenessMechanicEnabled)
			{
				healthScript.awarenessSlider.value = (float)healthScript.awareness/healthScript.maxAwareness * 100/1;

				sliderFillImage = healthScript.awarenessSlider.transform.FindChild("Fill Area/Fill").GetComponent<Image>();
				originalSliderColour = sliderFillImage.color;

				if(healthScript.awareness >= healthScript.maxAwareness)

				/*if(healthScript.awareness < healthScript.maxAwareness)
					powerupReadyImage.transform.localScale = Vector3.zero;
				else
				{*/
					//powerupReadyImage.transform.localScale = Vector3.one;

				if(oneHitKillsMechanicEnabled)
				{
					if(healthScript.awareness >= healthScript.maxAwareness)
					{
						sliderFillImage.color = Color.yellow;
						_battleEventManager.instance.playerHasOneHitKills = true;
					}
				}
				//}
			}
			else
			{
				healthScript.awarenessSlider.gameObject.SetActive(false);
			}
		}
		if(cooldownAmount <= rollDuration)
		{
			showRollCooldownImage = false;
		}
	}


	void OnTriggerEnter2D(Collider2D other)
	{
		if(!canDodge)
			return;

		//Detect the angle the bullet/asteroid/missile is coming from (front vs rear, etc), and roll the dice to use later
		angle =  Vector2.Angle(other.transform.position - transform.position, transform.up);
		diceRoll = Random.Range(0, 100f);

		//if a bullet, check who fired it
		if(other.tag == "Bullet")
		{
			//return if it's friendly fire
			if(StaticTools.IsInLayerMask(other.GetComponent<ShotHit>().theFirer, transform.root.GetComponent<TargetableObject>().friendlyFireMask))
			{
				return;
			}
				
			RunChanceToDodgeCalculation(1);
		}
		else if(other.tag == "Asteroid")
		{
			if(!playerControlled)
			{
				if(!transform.parent.parent.GetComponent<SpriteRenderer>().isVisible) //will never hit asteroids when off screen
				{
					Roll();
					return;
				}
				//see about asteroid dodge skill
				RunChanceToDodgeCalculation(asteroidMultiplier);
			}
		}
		else if(other.tag == "Bomb")
		{
			if(!playerControlled)
			{
				//return if it's friendly fire
				if(StaticTools.IsInLayerMask(other.GetComponent<Missile>().theFirer, transform.root.GetComponent<TargetableObject>().friendlyFireMask))
				{
					return;
				}
				//see about missile dodge skill
				RunChanceToDodgeCalculation(missileMultiplier);
			}
		}
	}

	void RunChanceToDodgeCalculation(float multiplier)
	{
		//Run chance to roll from the given skill for that angle

		if(angle < frontAngle) //comes from front
		{
			if(diceRoll < frontDodgeSkill * multiplier)
			{
				Roll();
			}
			else
			{
				canDodge = false;
				Invoke("CanDodgeAgain", cantDodgeTime);
			}
		}
		else if(angle < sideAngle) // comes from side
		{
			if(diceRoll < sideDodgeSkill * multiplier)
			{
				Roll();
			}
			else
			{
				canDodge = false;
				Invoke("CanDodgeAgain", cantDodgeTime);
			}
		}
		else //comes from rear
		{
			if(diceRoll < rearDodgeSkill * multiplier)
			{
				Roll();
			}
			else
			{
				canDodge = false;
				Invoke("CanDodgeAgain", cantDodgeTime);
			}
		}
	}

	void CanDodgeAgain()
	{
		if(!healthScript.dead)
			canDodge = true;
	}

	
	void Update ()
	{	
		if(playerControlled)
		{
			if(rollCooldown <= 0)
			{ 		
				dodgeCooldownImage.enabled = false;
				dodgeCooldownImageText.enabled = false;

				if (Input.GetButtonDown("Dodge"))
				{
					DodgePressed();
				}		
			}
			else if(rollCooldown >0 && showRollCooldownImage)
			{
				dodgeCooldownImage.enabled = true;
				dodgeCooldownImage.GetComponentInChildren<Text>().enabled = true;
				dodgeCooldownImage.fillAmount = rollCooldown / cooldownAmount;
			}
		}

		RollTimer ();
		RollCooldown ();

	}//end of UPDATE

	public void DodgePressed()
	{
		if(canDodge == true)
		{
			Director.instance.numberOfManualDodges++;
			playerActivatedManualDodge = true;
			Roll (/*0*/);
		}
	}

	//REMOVE: can probably refactor and remove this once sure it's no longer used
	IEnumerator SetDodgingToTrue(/*int frameDelay*/)
	{
		dodgeCoroutineStarted = true;

		//next line just in to make the code continue to run. Not really using coroutine anymore
		yield return new WaitForEndOfFrame();

		/*while (frameDelay > 0)
		{
			frameDelay--;
			yield return new WaitForEndOfFrame();
		}*/
		dodging = true;

		dodgeCoroutineStarted = false;
	}
	
	
	public void Roll(/*int frameDelay*/)
	{
		if (ClickToPlay.instance.paused || !canDodge)
			return;

		if(dodgingCostsNitro && playerControlled && playerMovementScript.nitroRemaining/playerMovementScript.nitroBurnRate < rollDuration)
			return;

		StartCoroutine(SetDodgingToTrue(/*frameDelay*/));

		canDodge = false;
		myAudioSource.Play();
		rollCooldown = cooldownAmount;

		//ANIMATIONS


		if(animationStyle == AnimationStyle.SpriteSheet)
		{
			RollSpriteAnimation ();
		}
		else if(animationStyle == AnimationStyle.UnityAnimator)
		{
			animator.SetTrigger ("Dodging");
		}
		else if(animationStyle == AnimationStyle.RollAModel)
		{
			StartCoroutine("RollAnimation"); //used to roll a model with code instead of using the animator
		}


		if(!playerControlled)
		{
			if(hasAvatar)
			{
				aiFighterScript.myCharacterAvatarScript.bgScrollerScript.FastTurnRandomDirection();
			}
			//Player collider stays on for Awareness mana. Player's Health script will manage.
		}
		else
		{
			playerIsDodging = true;
			if(dodgingCostsNitro)
			{
				playerMovementScript.nitroRemaining -= playerMovementScript.nitroBurnRate * rollDuration;
				playerMovementScript.UpdateNitroHUDElements();
			}
		}
	}

	public void RollSpriteAnimation ()
	{		
		invertAnimationDirection = Random.Range (0, 2) == 1;
		mySpriteAnimator.SetPlayInReverseOrder (invertAnimationDirection);
		mySpriteAnimator.StartAnimatingSpriteSwap ();
		StartCoroutine ("RollAnimation");
	}

	IEnumerator RollAnimation()
	{		
		startTime = Time.time;

		while(Time.time < startTime + rollDuration)
		{
			//forumula to smooth in & out
			/*t = (Time.time - startTime)/ rollDuration;
			t = t * t * (3 - (2 * t));*/

			//formula to ease out of animation
			//t = (Time.time - startTime)/ rollDuration;
			//t = Mathf.Sin(t * Mathf.PI * 0.5f);

			//linear formula
			t = (Time.time - startTime)/ rollDuration;

			if(invertAnimationDirection)
				rotY = Mathf.Lerp(0, 360f, t);
			else
				rotY = Mathf.Lerp(360f, 0, t);
			newRot = new Vector3 (0, rotY, 0);

			if(alsoRotate.Length > 0)
			{
				for(int i = 0; i < alsoRotate.Length; i++)
				{
					if(alsoRotate[i] != null)
					{
						alsoRotate[i].localRotation = Quaternion.Euler(newRot);
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}

		rotY = 0;
	}

	public void CancelRollForDeath()
	{
		StopAllCoroutines();
		if(animator)
			animator.SetTrigger("Death");
	}

	
	void RollTimer()
	{		
		if(dodging)
		{
			rollTime -= Time.deltaTime;

			if(playerControlled)
			{
				playerMovementScript.TurnOnThrusterGroup(enginesFighterScript.toStrafeLeft);
				playerMovementScript.TurnOnThrusterGroup(enginesFighterScript.toStrafeRight);
			}
			else if(!playerControlled)
			{
				enginesFighterScript.TurnOnThrusterGroup(enginesFighterScript.toStrafeLeft);
				enginesFighterScript.TurnOnThrusterGroup(enginesFighterScript.toStrafeRight);				
			}
		}

		//for ending the actual dodge;
		if(dodging && rollTime <= 0.0f)
		{	
			playerActivatedManualDodge = false;
			
			dodging = false;
			if(playerControlled)
				playerIsDodging = false;
			else if(hasAvatar)
				aiFighterScript.myCharacterAvatarScript.bgScrollerScript.ReturnToNormal();

			rollTime = rollDuration;
		}		
	}


	void RollCooldown()
	{
		rollCooldown -= Time.deltaTime; 

		if (rollCooldown <= 0)
			CanDodgeAgain();
	}

	
	public IEnumerator DumpPlayerAwarenessMana(float howMany)
	{
		if(!awarenessMechanicEnabled)
			yield break;
		
		originalBarFill = healthScript.awarenessSlider.value;
		//logoScale = powerupReadyImage.transform.localScale.x;

		if(healthScript.awareness < 0)
			healthScript.awareness = 0;

		if(_battleEventManager.instance.playerHasOneHitKills && howMany >1)
		{
			awarenessMeterAudioSource.clip = oneHitKillSound;
		}
		else
			awarenessMeterAudioSource.clip = manaDumpSound;
		
		awarenessMeterAudioSource.Play();

		_battleEventManager.instance.playerHasOneHitKills = false;

		while(healthScript.awarenessSlider.value > (float)healthScript.awareness/healthScript.maxAwareness * 100/1)
		{
			healthScript.awarenessSlider.value -= originalBarFill * Time.deltaTime;

			sliderFillImage.color = Color.Lerp(Color.yellow, originalSliderColour, Time.time/startTime);

			/*if(logoScale > 0)
			{
				logoScale -= Time.deltaTime;
				powerupReadyImage.transform.localScale = new Vector3 (logoScale, logoScale, 1);
			}*/
			yield return new WaitForEndOfFrame();
		}
		healthScript.awarenessSlider.value = (float)healthScript.awareness/healthScript.maxAwareness * 100/1;
		//powerupReadyImage.transform.localScale = Vector3.zero;
	}


	public IEnumerator IncreasePlayerAwarenessMana()
	{
		if(!awarenessMechanicEnabled)
			yield break;

		originalBarFill = 100/healthScript.maxAwareness;

		//set new awareness and play sound
		if(healthScript.awareness < healthScript.maxAwareness)
		{
			healthScript.awareness++;

			if(oneHitKillsMechanicEnabled)
			{
				if(healthScript.awareness >= healthScript.maxAwareness)
				{
					sliderFillImage.color = Color.yellow;
					awarenessMeterAudioSource.clip = manaFullSound;
					_battleEventManager.instance.playerHasOneHitKills = true;
				}
			}
			else
				awarenessMeterAudioSource.clip = manaIncreaseSound;

			awarenessMeterAudioSource.Play();
		}

		//grow the slider to the right size
		while(healthScript.awarenessSlider.value < (float)healthScript.awareness/healthScript.maxAwareness * 100)
		{
			healthScript.awarenessSlider.value += originalBarFill * Time.deltaTime;
			/*if(logoScale > 0)
			{
				logoScale -= Time.deltaTime;
				powerupReadyImage.transform.localScale = new Vector3 (logoScale, logoScale, healthScript.maxAwareness);
			}*/
			yield return new WaitForEndOfFrame();
		}
		healthScript.awarenessSlider.value = (float)healthScript.awareness/healthScript.maxAwareness * 100/1;

		startTime = Time.time;

		//Get the powerup
		if(oneHitKillsMechanicEnabled && healthScript.awareness >= healthScript.maxAwareness)
		{
			sliderFillImage.color = Color.Lerp(originalSliderColour, Color.yellow, Time.time/startTime);
			/* = powerupReadyImage.transform.localScale.x;

			while(logoScale < 1)
			{
				logoScale += Time.deltaTime;
				powerupReadyImage.transform.localScale = new Vector3 (logoScale, logoScale, 1);
			}
			powerupReadyImage.transform.localScale = Vector3.one;*/


			yield return new WaitForEndOfFrame();
		}
	}

	void OnDisable()
	{
		CancelRollForDeath();
	}
}//Mono
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
	bool hasAvatar = false;

	public bool playerControlled = false;
	public static bool playerIsDodging = false;

	public bool powerupMechanicEnabled = true;
	[HideInInspector] public bool playerActivatedManualDodge = false;
	[HideInInspector] public Image dodgeCooldownImage;
	[HideInInspector] public Text dodgeCooldownImageText;

	public bool dodging = false;
	public float rollDuration = 1.11f;
	float rollTime;
	float rollCooldown = 0f; 
	public float cooldownAmount = 1.11f; 
	public bool canDodge = true;

	private Animator animator;


	[HideInInspector] public Image awarenessManaFillImage;
	[HideInInspector] public Image powerupReadyImage;
	AudioSource myAudioSource;
	AudioSource awarenessMeterAudioSource;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip manaDumpSound;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip manaIncreaseSound;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip manaFullSound;
	[Tooltip("Only needed on Player's Dodge")] public AudioClip oneHitKillSound;

	float originalBarFill;
	float logoScale;

	
	void Start()
	{
		animator = transform.parent.parent.GetComponent<Animator> ();
		rollTime = rollDuration;
		myAudioSource = GetComponent<AudioSource>();
		playerMovementScript = transform.parent.transform.parent.GetComponent<PlayerFighterMovement>();
		enginesFighterScript = transform.parent.transform.parent.GetComponent<EnginesFighter>();
		aiFighterScript = enginesFighterScript.GetComponent<AIFighter>();
		healthScript = transform.parent.transform.parent.GetComponent<HealthFighter>();

		if(aiFighterScript && aiFighterScript.myCharacterAvatarScript)
			hasAvatar = true;

		if(playerControlled)
		{
			/*dodgeCooldownImage = GameObject.Find("Dodge Cooldown Image").GetComponent<Image>();
			dodgeCooldownImageText = dodgeCooldownImage.GetComponentInChildren<Text>();*/
			awarenessManaFillImage = GameObject.Find("Awareness Image").GetComponent<Image>();
			awarenessMeterAudioSource = awarenessManaFillImage.GetComponent<AudioSource>();
			powerupReadyImage = GameObject.Find("Powerup Image").GetComponent<Image>();

			if(powerupMechanicEnabled)
			{
				awarenessManaFillImage.fillAmount = (float)healthScript.awareness/healthScript.maxAwareness;
				if(healthScript.awareness != healthScript.maxAwareness)
					powerupReadyImage.transform.localScale = Vector3.zero;
				else
				{
					powerupReadyImage.transform.localScale = Vector3.one;
					_battleEventManager.instance.playerHasOneHitKills = true;
				}
			}
			else
			{
				awarenessManaFillImage.gameObject.SetActive(false);
			}
		}

	}
	
	void Update ()
	{
		if(playerControlled)
		{
			if(rollCooldown <= 0)
			{ 		
			/*	dodgeCooldownImage.enabled = false;
				dodgeCooldownImageText.enabled = false;*/

				if (Input.GetButtonDown("Dodge"))
				{
					if(canDodge == true)
					{
						Director.instance.numberOfManualDodges++;
						playerActivatedManualDodge = true;
						Roll ();
					}
				}		
			}
			else if(rollCooldown >0)
			{
				/*dodgeCooldownImage.enabled = true;
				dodgeCooldownImage.GetComponentInChildren<Text>().enabled = true;
				dodgeCooldownImage.fillAmount = rollCooldown / cooldownAmount;*/
			}
		}

		RollTimer ();
		RollCooldown ();

	}//end of UPDATE
	
	
	public void Roll()
	{
		if (ClickToPlay.instance.paused || !canDodge)
			return;

		if(playerControlled && playerMovementScript.nitroRemaining/playerMovementScript.nitroBurnRate < rollDuration)
			return;

		dodging = true;
		canDodge = false;
		myAudioSource.Play();
		rollCooldown = cooldownAmount; 
		animator.SetTrigger ("Dodging");

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
			playerMovementScript.nitroRemaining -= playerMovementScript.nitroBurnRate * rollDuration;
			playerMovementScript.UpdateNitroHUDElements();
		}
	}

	public void CancelRollForDeath()
	{
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
			canDodge = true;
	}

	
	public IEnumerator DumpPlayerAwarenessMana(int howMany)
	{
		if(!powerupMechanicEnabled)
			yield break;
		
		originalBarFill = awarenessManaFillImage.fillAmount;
		logoScale = powerupReadyImage.transform.localScale.x;

		healthScript.awareness -= howMany;
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

		while(awarenessManaFillImage.fillAmount > (float)healthScript.awareness/healthScript.maxAwareness)
		{
			awarenessManaFillImage.fillAmount -= originalBarFill * Time.deltaTime;
			if(logoScale > 0)
			{
				logoScale -= Time.deltaTime;
				powerupReadyImage.transform.localScale = new Vector3 (logoScale, logoScale, 1);
			}
			yield return new WaitForEndOfFrame();
		}
		awarenessManaFillImage.fillAmount = (float)healthScript.awareness/healthScript.maxAwareness;
		powerupReadyImage.transform.localScale = Vector3.zero;
	}


	public IEnumerator IncreasePlayerAwarenessMana()
	{
		if(!powerupMechanicEnabled)
			yield break;
		
		if(healthScript.awareness < healthScript.maxAwareness)
		{
			healthScript.awareness++;
			if(healthScript.awareness == healthScript.maxAwareness)
			{
				awarenessMeterAudioSource.clip = manaFullSound;
				_battleEventManager.instance.playerHasOneHitKills = true;
			}
			else
				awarenessMeterAudioSource.clip = manaIncreaseSound;

			awarenessMeterAudioSource.Play();
		}

		while(awarenessManaFillImage.fillAmount < (float)healthScript.awareness/healthScript.maxAwareness)
		{
			awarenessManaFillImage.fillAmount += Time.deltaTime;
			if(logoScale > 0)
			{
				logoScale -= Time.deltaTime;
				powerupReadyImage.transform.localScale = new Vector3 (logoScale, logoScale, healthScript.maxAwareness);
			}
			yield return new WaitForEndOfFrame();
		}
		awarenessManaFillImage.fillAmount = (float)healthScript.awareness/healthScript.maxAwareness;

		if(healthScript.awareness >= healthScript.maxAwareness)
		{
			logoScale = powerupReadyImage.transform.localScale.x;

			while(logoScale < 1)
			{
				logoScale += Time.deltaTime;
				powerupReadyImage.transform.localScale = new Vector3 (logoScale, logoScale, 1);
			}
			powerupReadyImage.transform.localScale = Vector3.one;
			yield return new WaitForEndOfFrame();
		}
	}
}//Mono
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class HealthFighter : Health {

	AIFighter myAIScript;
	Dodge dodgeScript;

	bool thisIsPlayer = false;

	[Header("For dodging shots")]
	public bool hasDodge = true;
	public bool playerHasAutoDodge = false;
	public bool alwaysDodgesAsteroids = false;
	[Tooltip("If you don't always dodge asteroids, this number comes into play")]
	[Range(0, 100)]
	public float asteroidDodgeSkill = 80f;
	[Tooltip("If they take a hit, they'll automatically get back this much mana.")]
	public int manaToRestoreOnAHit = 1;
	[Range(0,100)]
	public float missileDodgeSkill = 10;
	Sprite startSprite;

	[Header("For turning off at Death")]
	public SpriteRenderer shadow;
	public SpriteRenderer effectAnimation;
	public SpriteRenderer sideOnSprite;
	public GameObject radarSig;
	public AudioSource engineNoise;
	public TrailRenderer trailRenderer;
	public GameObject thrustersParent;

	[Header("Other")]
	public GameObject previousAttacker;
	public GameObject previousPreviousAttacker;
	public bool lastDamageWasFromAsteroid = false;

	[HideInInspector] public Transform avatarAwarenessBars;
	[HideInInspector] public Transform avatarHealthBars;
	bool updateAvatarBars = false;

	//TODO:REMOVE THESE
	[HideInInspector]public float rollSkill;
	[HideInInspector]public float normalRollSkill;


	void Awake()
	{
		AwakeBaseClass ();

		myAIScript = GetComponent<AIFighter> ();

		rollSkill = normalRollSkill;

		dodgeScript = GetComponentInChildren<Dodge>();

		startSprite = GetComponent<SpriteRenderer> ().sprite;

		if(this.tag == "PlayerFighter")
			thisIsPlayer = true;
	}

	public void SetUpAvatarBars()
	{
		updateAvatarBars = true;

		GameObject healthBar = avatarHealthBars.GetChild(0).gameObject;
		for(int i = 0; i < avatarHealthBars.childCount; i++)
		{		
			avatarHealthBars.GetChild(i).gameObject.SetActive(false);
		}
		for(int i = 0; i < maxHealth; i++)
		{
			GameObject newBar = Instantiate(healthBar) as GameObject;
			newBar.name = "health bar";
			newBar.transform.SetParent(avatarHealthBars);
			newBar.SetActive(true);
		}

		GameObject awarenessBar = avatarAwarenessBars.GetChild(0).gameObject;
		for(int i = 0; i < avatarAwarenessBars.childCount; i++)
		{		
			avatarAwarenessBars.GetChild(i).gameObject.SetActive(false);
		}
		for(int i = 0; i < maxAwareness; i++)
		{
			GameObject newBar = Instantiate(awarenessBar) as GameObject;
			newBar.name = "awareness bar";
			newBar.transform.SetParent(avatarAwarenessBars);
			newBar.SetActive(true);
		}

		UpdateAvatarHealthBars();
		UpdateAvatarAwarenessBars();
	}

	
	void Update()
	{
		UpdateBaseClass (); //only affects Player through 'Health' script
		
		//FOR PARTICLE DAMAGE
		
		if(health <= (maxHealth*0.66f)&& health>0)
		{
			smoke.gameObject.SetActive(true);
			smokeEm.rate = new ParticleSystem.MinMaxCurve((1 -(health*0.01f))*150);
		}
		else
		{smoke.gameObject.SetActive(false);}
		
		
		if(health <= (maxHealth*0.33f) && health >0)
		{
			flames.gameObject.SetActive(true);
			flamesEm.rate = new ParticleSystem.MinMaxCurve( (0.5f -(health*0.01f))*100);
		}
		else
		{flames.gameObject.SetActive(false);}
		
		if(health <= maxHealth/5 && !hasFireExtinguisher)
		{
			healthRecoveryRate = -1f;
		}
		
		if(health <= 0f && dead == false && !dodgeScript.dodging)
		{
			Death();
		}

	}//end of Update


	void AwarenessRecharge()
	{
		awareness++;
		awareness = Mathf.Clamp(awareness, 0, maxAwareness);
		UpdateAvatarAwarenessBars();

		if(awareness == maxAwareness)
			CancelInvoke("AwarenessRecharge");
	}


	public void YouveBeenHit(GameObject theAttacker, GameObject theBullet, float baseDamage, float critChance)
	{
		int diceRoll = 0;
		float damage = baseDamage;

		if(theAttacker == previousAttacker && theAttacker == previousPreviousAttacker)
		{
			gameObject.SendMessage("DefendYourself", theAttacker, SendMessageOptions.DontRequireReceiver);
		}

		if (theAttacker.tag != "Asteroid") 
		{
			lastDamageWasFromAsteroid = false;
			previousPreviousAttacker = previousAttacker;
			previousAttacker = theAttacker;
		}
		else 
			lastDamageWasFromAsteroid = true;

		#region Player Has Been Hit
		if(thisIsPlayer)
		{
			//1. see if Player is already rolling. Increase Awareness mana if it was deliberate, and break out.

			if(dodgeScript.dodging)
			{
				if(dodgeScript.playerActivatedManualDodge && !lastDamageWasFromAsteroid)
				{
					dodgeScript.playerActivatedManualDodge = false;
					StartCoroutine(dodgeScript.IncreasePlayerAwarenessMana());
				}
				return;
			}
			//otherwise we're not already dodging

			//2. If fire is by Player's own team, don't take damage, and break out.
			if(theAttacker != null && Tools.instance.IsInLayerMask(theAttacker, friendlyFire))
			{
				return;
			}

			//3. see if you can dodge out of trouble

			if(playerHasAutoDodge && dodgeScript.canDodge && awareness >0)
			{
				if(theBullet.tag == "Bomb" && missileDodgeSkill <= 0)
				{/*do nothing*/}
				else if(theBullet.tag == "Bomb" && missileDodgeSkill > 0)
				{					
					dodgeScript.Roll();
					StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(1));
				}
				else
				{
					dodgeScript.Roll();
					StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(1));
					return;
				}
			}				

			//4. If we get here, the shot will hit. Shake camera

			if(theBullet.tag != "Asteroid")
			{
				theBullet.GetComponent<ShotHit>().CamShake();
			}
			else
			{
				theBullet.GetComponent<Asteroid>().CamShake();
			}			

			//5. apply damage

			health -= (int)damage;
			Tools.instance.SpawnExplosionMini (this.gameObject, 0.35f);

			if (theBullet.tag != "Asteroid" && theBullet.tag != "Bomb") 
			{
				theBullet.SetActive (false);
			}
			else if(theBullet.tag == "Bomb")
			{
				theBullet.GetComponent<Missile>().Explode();
			}

			bloodSplashImage.color = bloodSplashColour;
			myAudioSource.clip = playerHitSound;
			myAudioSource.Play();
			

			if(health <= 0 && dead == false)
			{
				Death();

				if(theAttacker.tag == "Asteroid")
				{
					StartCoroutine(theAttacker.GetComponent<Asteroid>().DestroyAsteroid());
				}
			}
			else if(health > 0 && theBullet.tag == "Bomb")
			{
				Vector2 pos = theBullet.transform.position + theBullet.transform.up * 0.25f;
				Tools.instance.SpawnExplosion(this.gameObject, pos, true);
			}

			if(!dead)
			{
				Tools.instance.VibrateController(0, 0.5f, 0.5f, 0.2f);
			}

			//7. Flash collider off to reduce repeat hits all at once
			if (!dead)
				StartCoroutine (FlashOnInvincibility ());

			//8. flash screen white and vibrate for pain
			StartCoroutine (Tools.instance.WhiteScreenFlash(0.1f));

			//9. Restore a mana if alive and not docking
			if(!dead && manaToRestoreOnAHit > 0)
				for(int i = 0; i < manaToRestoreOnAHit; i++)
					StartCoroutine(dodgeScript.IncreasePlayerAwarenessMana());
		}
		//end of Player Has Been Hit
		#endregion


		#region AI Has Been Hit
	
		else if(!thisIsPlayer)
		{
			//0. If already dodging, break out

			if(dodgeScript.dodging || temporarilyInvincible)
			{
				return;
			}

			//1. If fire is by this fighter's own team, roll to not take damage

			if(theAttacker != null && Tools.instance.IsInLayerMask(theAttacker, friendlyFire))
			{
				diceRoll = Random.Range(0, 101);
				if(diceRoll > 10)
				{
					return;
				}				
			}

			//2. If player hit with OneHitKills, kill immediately (no dodge chance)

			if(theAttacker.tag == "PlayerFighter" && _battleEventManager.instance.playerHasOneHitKills && theBullet.tag != "Bomb")
			{
				theAttacker.SendMessage("CallDumpAwarenessMana", 3); //TODO: Make this less arbitrary
				_battleEventManager.instance.playerHasOneHitKills = false;
				Death();
				return;
			}

			//3. see if you can dodge out of trouble (player must not be on cooldown)

			if(hasDodge && dodgeScript.canDodge)
			{
				if(theBullet.tag == "Bomb" && missileDodgeSkill <= 0)
				{/*do nothing*/}
				else if(theBullet.tag == "Bomb" && missileDodgeSkill > 0)
				{
					diceRoll = Random.Range(0, 101);
					if(diceRoll <= missileDodgeSkill)
					{
						dodgeScript.Roll();
						return;
					}
				}
				else if(theBullet.tag == "Asteroid")
				{
					if(alwaysDodgesAsteroids)
					{
						dodgeScript.Roll();
						return;
					}
					else
					{
						diceRoll = Random.Range(0, 101);
						if(diceRoll <= asteroidDodgeSkill) //if skill is high enough, take a free roll
						{
							dodgeScript.Roll();
							return;
						}
						else if(awareness > 0) // if it's not, the roll will cost 1 awareness, or proceed to damage
						{
							awareness--;
							UpdateAvatarAwarenessBars();
							dodgeScript.Roll();

							if(awarenessRechargeTime > 0)
							{
								CancelInvoke("AwarenessRecharge");
								InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
							}

							return;
						}
					}
				}
				else if(awareness > 0) //otherwise, dodge a bullet
				{	
					awareness--;
					UpdateAvatarAwarenessBars();
					dodgeScript.Roll();

					if(awarenessRechargeTime > 0)
					{
						CancelInvoke("AwarenessRecharge");
						InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
					}
					return;
				}
			}

			//4. If we get here, the shot will hit. apply damage

			health -= (int)damage;

			Tools.instance.SpawnExplosionMini (this.gameObject, 0.35f);

			if (theBullet.tag != "Asteroid" && theBullet.tag != "Bomb") 
			{
				theBullet.SetActive (false);
			}
			else if(theBullet.tag == "Bomb")
			{
				theBullet.GetComponent<Missile>().Explode();
			}
				
			//this returns at start of function if this ship doesn't display bars (i.e. if updateAvatarBars == false)
			UpdateAvatarHealthBars (); 


			if(health > 0 && snapFocusAmount > 0)
			{
				awareness += snapFocusAmount;
				UpdateAvatarAwarenessBars();
			}

			if(health <= 0 && !dead)
			{
				Death();

				if(theAttacker.tag == "Asteroid")
				{
					StartCoroutine(theAttacker.GetComponent<Asteroid>().DestroyAsteroid());
				}
			}
			else if(health > 0 && theBullet.tag == "Bomb") //happens if their health was greater than the missile damage
			{
				Vector2 pos = theBullet.transform.position + theBullet.transform.up * 0.25f;
				Tools.instance.SpawnExplosion(this.gameObject, pos, true);
			}

			//5. Flash collider off to reduce repeat hits all at once
			if (!dead)
				StartCoroutine (FlashOnInvincibility ());

			if(theAttacker.tag == "PlayerFighter")
			{
				//StartCoroutine(Tools.instance.HitCamSlowdown());
			}
		}
		#endregion

	}//end of YouveBeenHit


	public void UpdateAvatarHealthBars ()
	{
		if(!updateAvatarBars)
			return;
		
		for (int i = 0; i < avatarHealthBars.childCount; i++) 
		{
			avatarHealthBars.GetChild (i).gameObject.SetActive (false);
		}
		for (int j = 0; j < maxHealth; j++) 
		{
			avatarHealthBars.GetChild (j).gameObject.SetActive (true);
			if (j < maxHealth - health) 
			{
				Color inactiveColour = avatarHealthBars.GetChild (j).GetComponent<Image> ().color;
				inactiveColour.a = 0.25f;
				avatarHealthBars.GetChild (j).GetComponent<Image> ().color = inactiveColour;
			}
		}
	}

	public void UpdateAvatarAwarenessBars ()
	{
		if(!updateAvatarBars)
			return;
		
		for (int i = 0; i < avatarAwarenessBars.childCount; i++) 
		{
			avatarAwarenessBars.GetChild (i).gameObject.SetActive (false);
		}
		for (int j = 0; j < maxAwareness; j++) 
		{
			avatarAwarenessBars.GetChild (j).gameObject.SetActive (true);
			if (j < maxAwareness - awareness) 
			{
				Color inactiveColour = avatarAwarenessBars.GetChild (j).GetComponent<Image> ().color;
				inactiveColour.a = 0.25f;
				avatarAwarenessBars.GetChild (j).GetComponent<Image> ().color = inactiveColour;
			}
			else
			{
				Color activeColour = avatarAwarenessBars.GetChild (j).GetComponent<Image> ().color;
				activeColour.a = 1f;
				avatarAwarenessBars.GetChild (j).GetComponent<Image> ().color = activeColour;
			}
		}
	}

	void Death()
	{
		//this first part fixes the Sprite Exploder issues that comes with dying near the end of roll animation
		if(GetComponent<SpriteRenderer>().sprite == null)
		{
			GetComponent<SpriteRenderer>().sprite = startSprite;
		}

		//TODO: collider seems to be turned off by Sprite Slicer. Must investigate to confirm

		dead = true;

		if(LayerMask.LayerToName(gameObject.layer) == "PMCFighters" && !thisIsPlayer)
		{
			Subtitles.instance.PostSubtitle(new string[]{this.name + " HAS BEEN DESTROYED!!!"});
			_battleEventManager.instance.CallWingmanDied();
		}
			
		if (hasDodge)
			dodgeScript.CancelRollForDeath ();

		awareness = 0;
		CancelInvoke("AwarenessRecharge");

		Tools.instance.SpawnExplosion (this.gameObject, transform.position, true);

		GetComponent<Animator>().enabled = false;
		GetComponentInChildren<Dodge>().enabled = false;
		radarSig.SetActive (false);
		transform.FindChild("Effects").gameObject.SetActive(false);

		transform.SetParent (null);

		//TODO: if you are the flight leader, record death stats and set a new leader
		if(GetComponentInChildren<SquadronLeader>() != null && 
		   GetComponentInChildren<SquadronLeader>().firstFlightOrders != SquadronLeader.Orders.Extraction)
		{
			GetComponentInChildren<SquadronLeader>().EngageAtWill();
			GetComponentInChildren<SquadronLeader>().gameObject.SetActive(false);
		}

		//Player-specific death
		if(thisIsPlayer)
		{
			PlayerAILogic myAIScript = GetComponent<PlayerAILogic>();

			myAIScript.engineScript.engineNoise.gameObject.SetActive(false);
			myAIScript.shootScript.enabled = false;
			myAIScript.engineScript.enabled = false;
			RadioCommands.instance.gameObject.SetActive(false);
			//dodgeScript.dodgeCooldownImage.gameObject.SetActive(false);

			if(myAIScript.enemyCommander.knownEnemyFighters.Contains(gameObject))
			{
				myAIScript.enemyCommander.knownEnemyFighters.Remove(gameObject);
			}
			myAIScript.enemyCommander.RemoveFromMyAttackersWhenDead(gameObject);
			myAIScript.myCommander.myFighters.Remove(gameObject);
			myAIScript.orders = PlayerAILogic.Orders.NA;
			myAIScript.myCommander.losses ++;

			foreach(GameObject go in myAIScript.myAttackers)
			{
				go.SendMessage("TargetDestroyed");
			}
			myAIScript.myAttackers.Clear();

			Tools.instance.VibrateController(0, 1, 1, 0.75f);

			_battleEventManager.instance.CallPlayerShotDown();
			Director.instance.SpawnPilotEVA(transform.position, transform.rotation, true);
		}
		//any other fighter death
		else
		{
			myAIScript.shootScript.enabled = false;
			myAIScript.engineScript.enabled = false;
			if(updateAvatarBars)
			{
				myAIScript.myCharacterAvatarScript.gameObject.SetActive(false);
				myAIScript.myCharacterAvatarScript.avatarOutput.GetComponent<Animator>().enabled = true;
				myAIScript.myCharacterAvatarScript.avatarOutput.GetComponent<Animator>().SetBool("isShotDown", true);
			}
			gameObject.SendMessage("HUDPointerOff");

			if(myAIScript.enemyCommander.knownEnemyFighters.Contains(gameObject))
			{
				myAIScript.enemyCommander.knownEnemyFighters.Remove(gameObject);
			}
			myAIScript.enemyCommander.RemoveFromMyAttackersWhenDead(gameObject); //removes this from whoever it was attacking
			myAIScript.myCommander.myFighters.Remove(gameObject);
			myAIScript.orders = AIFighter.Orders.NA;
			myAIScript.ChangeToNewState(myAIScript.deathStates, new float[]{1});
			myAIScript.myCommander.losses ++;
	
			if(previousAttacker != null)
			{
				previousAttacker.SendMessage("AddKill", SendMessageOptions.DontRequireReceiver);
			}

			foreach(GameObject go in myAIScript.myAttackers)
			{
				//this should never return null ref because myAttackers is adjusted at death time of the attacker, but it does sometimes, hence
				//don't require receiver
				go.SendMessage("TargetDestroyed", SendMessageOptions.DontRequireReceiver);
			}
			//myAIScript.myAttackers.Clear();
			try{
			myAIScript.flightLeadSquadronScript.activeWingmen.Remove(gameObject);
			myAIScript.flightLeadSquadronScript.deadWingmen.Add(gameObject);
			myAIScript.flightLeadSquadronScript.CheckActiveMateStatus();
			}catch{} 

			Invoke("Deactivate", 10);
		}
		SpriteExploder.instance.Explode (this.gameObject, 3, 0.5f);

	}//end of Death


}//Mono

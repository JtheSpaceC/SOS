using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class HealthFighter : Health {

	AIFighter myAIScript;
	Dodge dodgeScript;

	bool thisIsPlayer = false;

	[Header("For dodging shots")]

	[Tooltip("Only affects if this is the player.")]
	[HideInInspector] public bool playerHasAutoDodge = false;

	[Range(0, 100)]
	public float asteroidDodgeSkill = 80f;
	[Range(0,100)]
	public float missileDodgeSkill = 10;
	Sprite startSprite;

	[Header("For turning off at Death")]
	public SpriteRenderer shadow;
	public SpriteRenderer effectAnimation;
	public SpriteRenderer sideOnSprite;
	public GameObject radarSig;
	public AudioSource engineNoise;

	[Header("Other. (Look for debugging. Don't touch!)")]
	public GameObject previousAttacker;
	public GameObject previousPreviousAttacker;
	public bool lastDamageWasFromAsteroid = false;

	[HideInInspector] public Transform avatarAwarenessBars;
	[HideInInspector] public Transform avatarHealthBars;
	[HideInInspector] public Image avatarFlashImage;
	 public Image avatarRadialHealthBar;
	 public Image avatarRadialAwarenessBar;
	float maxFill = 0.25f;
	bool updateAvatarBars = false;


	void Awake()
	{
		AwakeBaseClass ();

		myAIScript = GetComponent<AIFighter> ();

		startSprite = GetComponent<SpriteRenderer> ().sprite;

		if(this.tag == "PlayerFighter")
			thisIsPlayer = true;

		dodgeScript = GetComponentInChildren<Dodge>();
	}

	public void SetUpAvatarBars()
	{
		updateAvatarBars = true;

		//OLD SYSTEM
		/*GameObject healthBar = avatarHealthBars.GetChild(0).gameObject;
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
		}*/

		UpdateAvatarHealthBars();
		UpdateAvatarAwarenessBars();
	}

	
	void Update()
	{
		if(dead)
			return;
		
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

		//FOR HEALTH RECOVERY
		
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


	public void YouveBeenHit(GameObject theAttacker, GameObject theBullet, float baseDamage, float critChance, float accuracy)
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

			if(dodgeScript.dodging || temporarilyInvincible)
			{
				if(dodgeScript.playerActivatedManualDodge && !lastDamageWasFromAsteroid)
				{
					if(awarenessMode == AwarenessMode.SkillBased)
					{
						dodgeScript.playerActivatedManualDodge = false;
						StartCoroutine(dodgeScript.IncreasePlayerAwarenessMana());
					}
					Director.instance.numberOfSuccessfulDodges ++;
				}
				return;
			}
			//otherwise we're not already dodging

			//2. If fire is by Player's own team, don't take damage, and break out.
			if(theAttacker != null && StaticTools.IsInLayerMask(theAttacker, GetComponent<PlayerAILogic>().friendlyFireMask))
			{
				return;
			}

			//3. see if you can auto-dodge out of trouble 

		/*	if(playerHasAutoDodge && dodgeScript.canDodge && awareness >0) //mostly disabled now
			{
				if(theBullet.tag == "Bomb" && missileDodgeSkill <= 0)
				{do nothing}
				else if(theBullet.tag == "Bomb" && missileDodgeSkill > 0)
				{
//					dodgeScript.Roll(0);
//					Director.instance.numberOfAutomatedDodges++;
//				
//					StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(1));
				}
				else if(theBullet.tag == "Asteroid")
				{
					//do nothing. player should manually dodge asteroids
				}
				else //it's a bullet
				{
					dodgeScript.Roll(0);
					Director.instance.numberOfAutomatedDodges++;
					StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(1));
					return;
				}


			}				
			else*/ 

			if(!playerHasAutoDodge && awareness > 0)
			{
				if(awareness > 0 && theBullet.tag == "Bullet") //otherwise, dodge a bullet
				{
					if(accuracy <= awareness)
					{
						awareness = Mathf.Clamp(awareness - accuracy, 0, maxAwareness);
						StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(damage)); 

						if(awarenessRechargeTime > 0)
						{
							CancelInvoke("AwarenessRecharge");
							InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
						}
						return;
					}
					else if(accuracy > awareness)
					{
						damage *= (accuracy - awareness)/accuracy;
						awareness = 0;
						StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(damage)); 

						if(awarenessRechargeTime > 0)
						{
							CancelInvoke("AwarenessRecharge");
							InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
						}
					}
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

			if(awarenessMode == AwarenessMode.Recharge)
			{
				if(awarenessRechargeTime > 0)
				{
					CancelInvoke("AwarenessRecharge");
					InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
				}
			}
			//5. apply damage

			health -= damage;
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

				bloodSplashImage.color = Color.clear;

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
				StartCoroutine (FlashOnInvincibility (0));

			//8. flash screen white and vibrate for pain
			StartCoroutine (Tools.instance.WhiteScreenFlash(0.1f));

			//9. Restore a mana if alive and not docking
			if(!dead && snapFocusAmount > 0 && theBullet.tag != "Asteroid")
				for(int i = 0; i < snapFocusAmount; i++)
				{
					StartCoroutine(dodgeScript.IncreasePlayerAwarenessMana());
				}
		}
		//end of Player Has Been Hit
		#endregion


		#region AI Has Been Hit
	
		else if(!thisIsPlayer)
		{
			//0. If already dodging, break out

			if((dodgeScript != null && dodgeScript.dodging) || temporarilyInvincible)
			{
				return;
			}

			//1. If fire is by this fighter's own team, roll to not take damage

			if(theAttacker != null && StaticTools.IsInLayerMask(theAttacker, myAIScript.friendlyFireMask))
			{
				if(theAttacker == gameObject) //can't shoot yourself
					return;
				diceRoll = Random.Range(0, 101);
				if(diceRoll > 10)
				{
					return;
				}				
			}

			//2. If player hits with OneHitKills, kill immediately (no dodge chance)

			if(theAttacker.tag == "PlayerFighter" && _battleEventManager.instance.playerHasOneHitKills && theBullet.tag != "Bomb")
			{
				Director.instance.numberOfSpecialsUsed ++;
				theAttacker.SendMessage("CallDumpAwarenessMana", 3); //TODO: Make this less arbitrary
				_battleEventManager.instance.playerHasOneHitKills = false;
				Death();
				return;
			}

			//3. see if AI can dodge out of trouble
			#region removed most of step 3. Now handled on dodge scipt itself

			/*
			if((hasDodge && (dodgeScript.canDodge || dodgeScript.dodgeCoroutineStarted))
				|| theBullet.tag == "Asteroid")
			{
				if(theBullet.tag == "Bomb" && missileDodgeSkill <= 0) //if it's a missile and you CAN'T dodge missiles
				{//do nothing
		}
				else if(theBullet.tag == "Bomb" && missileDodgeSkill > 0) //if it's a missile and you CAN dodge missiles
				{
					diceRoll = Random.Range(0, 101);
					if(diceRoll <= missileDodgeSkill)
					{
						if(myAIScript.whichSide == TargetableObject.WhichSide.Enemy)
							dodgeScript.Roll(2);
						else
						{
							dodgeScript.Roll(0);

							if(updateAvatarBars)
							StartCoroutine(Tools.instance.ImageFlashToClear(avatarFlashImage, Tools.instance.avatarAwarenessFlashColour, 1f));
						}
						return;
					}
				}
				else if(theBullet.tag == "Asteroid") //if it's an Asteroid
				{
					if(!GetComponent<SpriteRenderer>().isVisible) //will never hit asteroids when off screen
					{
						if(myAIScript.whichSide == TargetableObject.WhichSide.Enemy)
							dodgeScript.Roll(2);
						else
							dodgeScript.Roll(0);						
						return;
					}
					else
					{
						diceRoll = Random.Range(0, 101);
						if(diceRoll <= asteroidDodgeSkill) //if skill is high enough, take a free roll
						{
							if(myAIScript.whichSide == TargetableObject.WhichSide.Enemy)
								dodgeScript.Roll(2);
							else
								dodgeScript.Roll(0);						
							return;
						}
						else if(awareness > 0) // if it's not, the roll will cost 1 awareness, or proceed to damage
						{
							awareness = Mathf.Clamp(awareness - damage, 0, maxAwareness);
							UpdateAvatarAwarenessBars();

							if(myAIScript.whichSide == TargetableObject.WhichSide.Enemy)
								dodgeScript.Roll(2);
							else
							{
								dodgeScript.Roll(0);

								if(updateAvatarBars)
									StartCoroutine(Tools.instance.ImageFlashToClear(avatarFlashImage, Tools.instance.avatarAwarenessFlashColour, 1f));
							}						

							if(awarenessRechargeTime > 0)
							{
								CancelInvoke("AwarenessRecharge");
								InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
							}
							return;
						}
					}
				}
				else*/ 
			#endregion
			if(awareness > 0 && theBullet.tag == "Bullet") //otherwise, dodge a bullet
			{
				if(accuracy <= awareness)
				{
					awareness = Mathf.Clamp(awareness - accuracy, 0, maxAwareness);
					UpdateAvatarAwarenessBars();
					if(awarenessRechargeTime > 0)
					{
						CancelInvoke("AwarenessRecharge");
						InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
					}
					return;
				}
				else if(accuracy > awareness)
				{
					damage *= (accuracy - awareness)/accuracy;
					awareness = 0;
					UpdateAvatarAwarenessBars();
					if(awarenessRechargeTime > 0)
					{
						CancelInvoke("AwarenessRecharge");
						InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
					}
				}
			}
			//}


			//4. If we get here, the shot will hit. apply damage

			health -= damage;

			if(awarenessRechargeTime > 0)
			{
				CancelInvoke("AwarenessRecharge");
				InvokeRepeating("AwarenessRecharge", awarenessRechargeTime, awarenessRechargeTime);
			}

			Tools.instance.SpawnExplosionMini (this.gameObject, 0.35f);

			if (theBullet.tag != "Asteroid" && theBullet.tag != "Bomb") 
			{
				theBullet.SendMessage("HitAndStop");
			}
			else if(theBullet.tag == "Bomb")
			{
				theBullet.GetComponent<Missile>().Explode();
			}
				
			//this returns at start of function if this ship doesn't display bars (i.e. if updateAvatarBars == false)
			UpdateAvatarHealthBars (); 

			//restore a mana if not dead and Snap Focus isn't zero (which it often is for low level enemies)
			if(health > 0 && snapFocusAmount > 0)
			{
				awareness += snapFocusAmount;
				UpdateAvatarAwarenessBars();

				if(updateAvatarBars)
					StartCoroutine(Tools.instance.ImageFlashToClear(avatarFlashImage, Tools.instance.avatarHitFlashColour, 1f));
			}

			if(health <= 0 && !dead)
			{
				Death();

				if(theAttacker.tag == "Asteroid")
				{
					StartCoroutine(theAttacker.GetComponent<Asteroid>().DestroyAsteroid());
				}

				if(theBullet.tag == "Bomb" && theAttacker.tag == "PlayerFighter")
				{
					Director.instance.playerMissileKills ++;
				}
			}
			else if(health > 0 && theBullet.tag == "Bomb") //happens if their health was greater than the missile damage
			{
				Vector2 pos = theBullet.transform.position + theBullet.transform.up * 0.25f;
				Tools.instance.SpawnExplosion(this.gameObject, pos, true);
			}

			//5. Flash collider off to reduce repeat hits all at once (delay if enemy)
			if (!dead)
			{
				if(myAIScript.whichSide == TargetableObject.WhichSide.Enemy)
					StartCoroutine (FlashOnInvincibility (2));
				else
					StartCoroutine (FlashOnInvincibility (0));
			}
		}
		#endregion

	}//end of YouveBeenHit


	public void UpdateAvatarHealthBars ()
	{
		if(!updateAvatarBars)
			return;

		//OLD SYSTEM
		/*for (int i = 0; i < avatarHealthBars.childCount; i++) 
		{
			avatarHealthBars.GetChild (i).gameObject.SetActive (false);
		}
		for (int j = 0; j < maxHealth; j++) 
		{
			avatarHealthBars.GetChild (j).gameObject.SetActive (true);
			avatarHealthBars.GetChild (j).GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, (float)health/maxHealth);

			if (j < maxHealth - health) 
			{
				Color inactiveColour = avatarHealthBars.GetChild (j).GetComponent<Image> ().color;
				inactiveColour.a = 0.25f;
				avatarHealthBars.GetChild (j).GetComponent<Image> ().color = inactiveColour;
			}
		}*/
		avatarRadialHealthBar.fillAmount = health/maxHealth * maxFill;
		avatarRadialHealthBar.color = Color.Lerp(Color.red, Color.green, health/maxHealth);
	}

	public void UpdateAvatarAwarenessBars ()
	{
		if(!updateAvatarBars)
			return;

		awareness = Mathf.Clamp(awareness, 0, maxAwareness);

		//OLD WAY
		/*for (int i = 0; i < avatarAwarenessBars.childCount; i++) 
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
		}*/
		avatarRadialAwarenessBar.fillAmount = awareness/maxAwareness * maxFill;

	}

	void InitialDeactivation() //player doesn't do this. just for AI
	{
		gameObject.SendMessage("HUDPointerOff");

		if(myAIScript.enemyCommander.knownEnemyFighters.Contains(gameObject))
		{
			myAIScript.enemyCommander.knownEnemyFighters.Remove(gameObject);
		}

		myAIScript.enemyCommander.RemoveFromMyAttackersWhenDead(gameObject); //removes this from whoever it was attacking
		myAIScript.myCommander.myFighters.Remove(gameObject);

		if(myAIScript.myCharacterAvatarScript)
			Destroy(myAIScript.myCharacterAvatarScript.gameObject);

		foreach(GameObject go in myAIScript.myAttackers)
		{
			//this should never return null ref because myAttackers is adjusted at death time of the attacker, but it does sometimes, hence
			//don't require receiver
			go.SendMessage("TargetDestroyed", SendMessageOptions.DontRequireReceiver);
		}
	}


	public void RetreatAndRetrieval() //moment when unit leaves the map, normally via RTB order
	{
		InitialDeactivation();

		if(updateAvatarBars)
		{
			myAIScript.myCharacterAvatarScript.avatarOutput.GetComponent<Animator>().enabled = true;
			myAIScript.myCharacterAvatarScript.avatarOutput.GetComponentInChildren<Text>().text = "R.T.B.";
			myAIScript.myCharacterAvatarScript.avatarOutput.GetComponent<Animator>().SetBool("isRTB", true);
			avatarRadialHealthBar.fillAmount = 0;
			avatarRadialAwarenessBar.fillAmount = 0;
		}

		myAIScript.myCommander.retreated++;

		if(myAIScript.whichSide == TargetableObject.WhichSide.Ally)
			Subtitles.instance.PostSubtitle(new string[]{this.name + " has Returned To Base."});

		try{
			myAIScript.flightLeadSquadronScript.retrievedWingmen.Add(gameObject);
		}catch{} 

		transform.SetParent (null);
			
		FinalDeactivation();
	}


	public void Death()
	{
		// collider is turned off by SpriteExploder

		dead = true;

		GetComponent<SpriteRenderer>().enabled = true;
		transform.FindChild("Effects/GUI").gameObject.SetActive(false);
		transform.FindChild("Effects/engine noise").GetComponent<AudioSource>().enabled = false;

		if(LayerMask.LayerToName(gameObject.layer) == "PMCFighters" && !thisIsPlayer)
		{
			Subtitles.instance.PostSubtitle(new string[]{this.name + " HAS BEEN DESTROYED!!!"});
			_battleEventManager.instance.CallWingmanDied();
		}
			
		if (dodgeScript)
		{
			dodgeScript.CancelRollForDeath ();
			dodgeScript.enabled = false;
		}

		awareness = 0;
		CancelInvoke("AwarenessRecharge");

		Tools.instance.SpawnExplosion (this.gameObject, transform.position, true);

		GetComponent<Animator>().enabled = false;
		if(radarSig)
			radarSig.SetActive (false);

		//Player-specific death
		if(thisIsPlayer)
		{
			PlayerAILogic myAIScript = GetComponent<PlayerAILogic>();

			transform.FindChild("Effects/Contrails").gameObject.SetActive(false);
			myAIScript.engineScript.engineNoise.gameObject.SetActive(false);
			myAIScript.shootScript.transform.parent.gameObject.SetActive(false);
			myAIScript.missilesScript.targetingPip.SetActive(false);
			myAIScript.engineScript.enabled = false;
			RadioCommands.instance.gameObject.SetActive(false);
			GameObject.Find("Consumables Panel").SetActive(false);
			//dodgeScript.dodgeCooldownImage.gameObject.SetActive(false);

			if(Tools.instance.barrelTempSlider)
				Tools.instance.barrelTempSlider.gameObject.SetActive(false);

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

			//TODO: Decide what wingmen whould really do in a battle
			//tell wingmen to resume fighting on their own if Player dies
			SquadronLeader squadLeadScript = GetComponentInChildren<SquadronLeader>();
			if(squadLeadScript != null && squadLeadScript.firstFlightOrders != SquadronLeader.Orders.Extraction)
			{
				squadLeadScript.EngageAtWill();
				squadLeadScript.gameObject.SetActive(false);
			}

			Explode();
		}
		//any other fighter death
		else
		{
			myAIScript.shootScript.enabled = false;
			myAIScript.engineScript.enabled = false;		
			
			if(updateAvatarBars)
			{
				myAIScript.myCharacterAvatarScript.avatarOutput.GetComponent<Animator>().enabled = true;
				myAIScript.myCharacterAvatarScript.avatarOutput.GetComponent<Animator>().SetBool("isShotDown", true);
				avatarRadialHealthBar.fillAmount = 0;
				avatarRadialAwarenessBar.fillAmount = 0;
			}

			InitialDeactivation();

			myAIScript.orders = AIFighter.Orders.NA;
			myAIScript.ChangeToNewState(myAIScript.deathStates, new float[]{1});
			myAIScript.myCommander.losses ++;
	
			if(previousAttacker != null)
			{
				previousAttacker.SendMessage("AddKill", SendMessageOptions.DontRequireReceiver);
			}

			try{
				if(myAIScript.flightLeadSquadronScript.deadWingmen.Contains(gameObject))
					myAIScript.flightLeadSquadronScript.deadWingmen.Add(gameObject);
			}catch{} 

			Invoke("FinalDeactivation", 10);

			if(Random.Range(0, 2) == 1)
			{
				//for two stage death
				gameObject.AddComponent<Rotator>();
				GetComponent<Rotator>().Mode = Rotator.myMode.RandomizedAtStart;
				GetComponent<Rotator>().randomizeDirection = true;
				smoke.gameObject.SetActive(true);
				smokeEm.rate = 150;
				smoke.startLifetime *= 3;
				flames.gameObject.SetActive(true);
				flamesEm.rate = 100;
				flames.startLifetime *= 3;
				temporarilyInvincible = true;

				Invoke("Explode", 2);
			}
			else 
				Explode();
		}	

	}//end of Death

	void Explode()
	{
		Tools.instance.SpawnExplosion (this.gameObject, transform.position, true);


		GameObject effects = transform.FindChild("Effects").gameObject;

		//transform.FindChild("Effects").gameObject.SetActive(false);
		effects.transform.FindChild("Animation").gameObject.SetActive(false);
		effects.AddComponent<DestroyAfterTime>();
		effects.GetComponent<DestroyAfterTime>().enabled = false;
		effects.GetComponent<DestroyAfterTime>().delayTime = 5;
		effects.GetComponent<DestroyAfterTime>().enabled = true;
		smoke.startLifetime = 2;
		flames.startLifetime = 2;
		Invoke("StopParticles", 0);

		effects.transform.SetParent(null);
		Rigidbody2D effectsRB = effects.AddComponent<Rigidbody2D>();
		effectsRB.gravityScale = 0;

		effectsRB.velocity = GetComponent<TargetableObject>().myRigidbody.velocity;

		//this part fixes the Sprite Exploder issues that comes with dying near the end of roll animation
		if(GetComponent<SpriteRenderer>().sprite == null)
		{
			GetComponent<SpriteRenderer>().sprite = startSprite;
		}
		SpriteExploder.instance.Explode (this.gameObject, 3, 0.5f);
	}

	void StopParticles()
	{
		smokeEm.rate = 0;
		flamesEm.rate = 0;
	}


}//Mono

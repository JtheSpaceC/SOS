using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthTransport : Health {

	AITransport myAIScript;
	EnginesFighter engineScript;

	public List<GameObject> myArmour;


	void Awake ()
	{
		StartBaseClass ();

		myAIScript = GetComponent<AITransport> ();
		engineScript = GetComponent<EnginesFighter> ();

		flames.transform.position += (Vector3)Random.insideUnitCircle * 0.5f;
		smoke.transform.position = flames.transform.position - transform.up * 0.22f;
	}
	

	void Update () 
	{
		//FOR PARTICLE DAMAGE
		
		if(health <= (maxHealth*0.66f)&& health>0)
		{
			smoke.gameObject.SetActive(true);
			smokeEm.rate = new ParticleSystem.MinMaxCurve( (1 -(health*0.01f))*150);
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


		//FOR FUNCTIONALITY
		
		if(health <= maxHealth/5 && !hasFireExtinguisher)
		{
			healthRecoveryRate = -1f;
		}
		
		if(health <= 0f && dead == false)
		{
			Death();
		}
	}


	public void YouveBeenHit(GameObject theAttacker, GameObject theBullet, float baseDamage, float critChance)
	{
		float damage = baseDamage;
		damage *= 1; //may want to adjust damage by crit or bonus damage 

		//1. apply damage
		
		//health -= (int)damage; TODO: restore this, as Transport never takes damage now.
		Tools.instance.SpawnExplosionMini (this.gameObject, .75f);
		
		if (theBullet.tag != "Asteroid" && theBullet.tag != "Bomb") 
		{
			theBullet.SendMessage("HitAndStop");
		}
		else if(theBullet.tag == "Bomb")
		{
			theBullet.GetComponent<Missile>().Explode();
		}
		
		if(gameObject.tag == "PlayerFighter")
		{
			bloodSplashImage.color = bloodSplashColour;
			myAudioSource.clip = playerHitSound;
			myAudioSource.Play();
		}
		
		if(health <= 0 && !dead)
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
		
		//2. Flah collider off to reduce repeat hits all at once
		if(GetComponent<SupportShipFunctions>().whichSide == TargetableObject.WhichSide.Enemy)
			StartCoroutine (FlashOnInvincibility (2));
		else
			StartCoroutine (FlashOnInvincibility (0));
		
	}//end of YouveBeenHit



	void Death()
	{
		dead = true;
		GetComponentInChildren<HealthTurret> ().Death (false, true);
		SpriteExploder.instance.Explode (this.gameObject, 5, 2.5f);

		//blow up all the armour I had left
		/*foreach(GameObject armour in myArmour)
		{
			armour.GetComponent<Armour>().FractureThenScatter();

			for(int i = 0; i < armour.transform.childCount; i++)
			{
				Transform effectChild = armour.transform.GetChild(i);

				effectChild.GetComponent<Armour>().FractureThenScatter();
			}
		}*/


		myAIScript.myCommander.myTransports.Remove (this.gameObject);
		engineScript.enabled = false;

		myAIScript.ReleaseFighters (false);
		if(myAIScript.sentRadioMessageToPlayerGroup)
		{
			RadioCommands.instance.timer += 60; //this should cause the radio message to fade out instantly
		}

		if(myAIScript.enemyCommander.knownEnemyTransports.Contains(this.gameObject))
		{
			myAIScript.enemyCommander.knownEnemyTransports.Remove(this.gameObject);
		}
		Invoke("FinalDeactivation", 10);
	}
}

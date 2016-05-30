using UnityEngine;
using System.Collections;

public class HealthTurret : Health {
	
	WeaponsTurret turretScript;
		
	private SpriteRenderer myRenderer;
	
	public GameObject smokeEffects;
	private GameObject radarSig;
	
	
	void Awake()
	{
		AwakeBaseClass ();
		turretScript = GetComponent<WeaponsTurret> ();		
		myRenderer = GetComponent<SpriteRenderer> ();
		
		radarSig = transform.FindChild ("RadarSig").gameObject;
	}
	
	void Update () 
	{
		UpdateBaseClass ();

		myRenderer.color = Color.Lerp (Color.white, Color.black, 1 - ((float)health/(float)maxHealth));
		
	}// end of Update
	
	
	
	public void YouveBeenHit(GameObject theAttacker, GameObject theBullet, int baseDamage, float critChance)
	{
		health -= (int)baseDamage;
		Tools.instance.SpawnExplosionMini (this.gameObject, 0.3f);
		
		if(health <= 0f && dead ==false)
		{
			Death(true, false);
			
			if(theAttacker != null && theAttacker.tag == ("Asteroid"))
			{
				theAttacker.SendMessage("DestroyAsteroid");
			}
		}

		if (theBullet.tag != "Asteroid" && theBullet.tag != "Bomb") 
		{
			theBullet.SendMessage("HitAndStop");
		}
		else if(theBullet.tag == "Bomb")
		{
			theBullet.GetComponent<Missile>().Explode();
		}
	}

	IEnumerator SmokeFade(GameObject smokeEffects)
	{
		yield return new WaitForSeconds(Random.Range(10,20));
		
		smokeEm.rate = new ParticleSystem.MinMaxCurve(0);
		
		Destroy (smokeEffects, 3f);
	}

	
	public void Death (bool makeSmoke, bool turnOffSprite) 
	{		
		GetComponent<Collider2D>().enabled = false;
		radarSig.SetActive (false);

		Tools.instance.SpawnExplosion(this.gameObject, this.transform.position, true);

		turretScript.enabled = false;

		foreach(GameObject go in turretScript.myAttackers)
		{
			go.SendMessage("TargetDestroyed");
		}
		turretScript.myAttackers.Clear();

		if(turretScript.enemyCommander.knownEnemyTurrets.Contains(gameObject))
		{
			turretScript.enemyCommander.knownEnemyTurrets.Remove(gameObject);
		}

		if(makeSmoke)
		{
			smokeEffects = Instantiate (smokeEffects, transform.position, transform.rotation) as GameObject;
			smokeEffects.transform.parent = this.transform;		
			StartCoroutine (SmokeFade(smokeEffects));
		}
		if (turnOffSprite)
			myRenderer.enabled = false;
		
		if(dead == false)
		{
			dead = true;
		}

	}
}//MONO

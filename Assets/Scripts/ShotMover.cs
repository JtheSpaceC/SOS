using UnityEngine;
using System.Collections;

public class ShotMover : MonoBehaviour {
	
	[Tooltip("This only a default value. Speed is normally set by the firing script.")]	
	public float shotSpeed = 7f;

	float defaultShotSpeed;
	ShotHit shotHit;
	public Color orangeLaserColour;
	public Color greenLaserColour;
	SpriteRenderer myRenderer;
	
	
	void Awake()
	{
		shotHit = GetComponent<ShotHit> ();
		myRenderer = GetComponent<SpriteRenderer> ();
		defaultShotSpeed = shotSpeed;
	}
	
	
	public void OkayGo (GameObject theFirer, float projectileDamage, float projectileCritChance, float projectileSpeed)
	{
		shotHit.theFirer = theFirer;

		if(theFirer.layer == LayerMask.NameToLayer("PMCFighters"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("PMCBullets");
			myRenderer.color = orangeLaserColour;
		}
		else if(theFirer.layer == LayerMask.NameToLayer("EnemyFighters"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("EnemyBullets");
			myRenderer.color = greenLaserColour;
		}
		else if(theFirer.layer == LayerMask.NameToLayer("PMCTurrets"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("PMCBullets");
			myRenderer.color = orangeLaserColour;
		}
		else if(theFirer.layer == LayerMask.NameToLayer("EnemyTurrets"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("EnemyBullets");
			myRenderer.color = greenLaserColour;
		}
		else if(theFirer.layer == LayerMask.NameToLayer("PMCTransports"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("PMCBullets");
			myRenderer.color = orangeLaserColour;
		}
		else if(theFirer.layer == LayerMask.NameToLayer("EnemyTransports"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("EnemyBullets");
			myRenderer.color = greenLaserColour;
		}

		shotSpeed = projectileSpeed;
		shotHit.averageDamage = projectileDamage;
		shotHit.chanceToCrit = projectileCritChance;

		GetComponent<AudioSource>().Play ();
		
		if(!theFirer.GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().velocity = theFirer.transform.root.GetComponent<Rigidbody2D>().velocity;
			GetComponent<Rigidbody2D>().AddForce (transform.up * shotSpeed);
		}
		
		else
		{
			//FORMER (CORRECT WAY)
			GetComponent<Rigidbody2D>().velocity = theFirer.GetComponent<Rigidbody2D>().velocity;
			GetComponent<Rigidbody2D>().AddForce (transform.up * shotSpeed);
			
			//NEW (allows for more accuracy with simplistic target lead equations
			//float platformSpeed = Mathf.Sqrt (theFirer.rigidbody2D.velocity.magnitude); //this is pretty meaningless without direction
			//rigidbody2D.AddForce (transform.up * shotSpeed);
			//rigidbody2D.AddForce (transform.up * platformSpeed);
		}
	}

	void OnDisable()
	{
		shotSpeed = defaultShotSpeed;
	}
	
	/*
	void Update()
	{
		float bulletSpeed = rigidbody2D.velocity.magnitude;
		print (bulletSpeed);
	}*/
	
} //MONO


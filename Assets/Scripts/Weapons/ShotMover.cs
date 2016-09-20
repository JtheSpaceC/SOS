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
	AudioSource myAudioSource;
	Rigidbody2D myRigidbody;
	Collider2D myCollider;
	ParticleSystem myParticleSystem;

	void Awake()
	{
		shotHit = GetComponent<ShotHit> ();
		myRenderer = GetComponent<SpriteRenderer> ();
		defaultShotSpeed = shotSpeed;
		myAudioSource = GetComponentInChildren<AudioSource>();
		myRigidbody = GetComponent<Rigidbody2D>();
		myCollider = GetComponent<Collider2D>();
		myParticleSystem = GetComponentInChildren<ParticleSystem>();
	}
	
	
	public void OkayGo (GameObject theFirer, int projectileDamage, float projectileCritChance, float projectileSpeed, float firerAccuracy)
	{
		myCollider.enabled = true;
		myRenderer.enabled = true;
		shotHit.theFirer = theFirer;
		myParticleSystem.Play();

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
		shotHit.normalDamage = projectileDamage;
		shotHit.chanceToCrit = projectileCritChance;
		shotHit.accuracy = firerAccuracy;

		myAudioSource.Play ();
		
		if(!theFirer.GetComponent<Rigidbody2D>())
		{
			myRigidbody.velocity = theFirer.transform.root.GetComponent<Rigidbody2D>().velocity;
			myRigidbody.AddForce (transform.up * shotSpeed / Time.deltaTime * Tools.instance.normalFixedDeltaTime);
		}
		
		else
		{
			//FORMER (CORRECT WAY)
			myRigidbody.velocity = theFirer.GetComponent<Rigidbody2D>().velocity;
			myRigidbody.AddForce (transform.up * shotSpeed / Time.deltaTime * Tools.instance.normalFixedDeltaTime);
			
			//NEW (allows for more accuracy with simplistic target lead equations
			//float platformSpeed = Mathf.Sqrt (theFirer.rigidbody2D.velocity.magnitude); //this is pretty meaningless without direction
			//rigidbody2D.AddForce (transform.up * shotSpeed);
			//rigidbody2D.AddForce (transform.up * platformSpeed);
		}
	}

	protected void HitAndStop()
	{
		myRigidbody.velocity = Vector2.zero;
		myCollider.enabled = false;
		myRenderer.enabled = false;
		myParticleSystem.Stop();
		Invoke("SetInactive", 1);
	}

	void SetInactive()
	{
		gameObject.SetActive(false);
	}

	void OnDisable()
	{
		shotSpeed = defaultShotSpeed;
		transform.rotation = Quaternion.Euler(Vector3.zero);
		CancelInvoke("SetInactive");
	}
	
	/*
	void Update()
	{
		float bulletSpeed = rigidbody2D.velocity.magnitude;
		print (bulletSpeed);
	}*/
	
} //MONO


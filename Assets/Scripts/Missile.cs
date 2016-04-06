using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour {

	Rigidbody2D myRigidbody;

	public enum MissileType {FireAndForget, Dumbfire, LockOn}
	public MissileType missileType;
	LayerMask targetSeekMask;
	public LayerMask validTargetsIfPMC;
	public LayerMask validTargetIfEnemy;

	public GameObject target;
	public GameObject theFirer;

	public float damage = 150;
	public float kickOffTime = 0.5f;
	public float fuelLifetime = 2f;
	public float fuseLifetime = 2.5f;
	[Header("Speed if dumbfire or FAF")]
	public float thrustPower = 10f;
	public float turningPower = 45f;
	[Header("Acceleration if LockOn")]
	[Range(0,1)] public float acceleration = 0.75f;
	float speed = 0;

	[Header("Effects")]
	public ParticleSystem smoke;
	
	float startTime;
	bool dead = false;
	

	void Start () 
	{
		startTime = Time.time;
	}

	public void OkayGo (GameObject theFirer, int leftOrRight, GameObject assignedTarget)
	{
		target = assignedTarget;
		myRigidbody = GetComponent<Rigidbody2D> ();
		StartCoroutine (TurnOnSmoke(kickOffTime, leftOrRight));

		if(theFirer.layer == LayerMask.NameToLayer("PMCFighters"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("PMCBombs");
			targetSeekMask = validTargetsIfPMC;
		}
		else if(theFirer.layer == LayerMask.NameToLayer("EnemyFighters"))
		{
			this.gameObject.layer = LayerMask.NameToLayer("EnemyBombs");
			targetSeekMask = validTargetIfEnemy;
		}
		
		GetComponent<AudioSource>().Play ();
		
		if(!theFirer.GetComponent<Rigidbody2D>())
		{
			myRigidbody.velocity = theFirer.transform.root.GetComponent<Rigidbody2D>().velocity;
		}
		
		else
		{
			//FORMER (CORRECT WAY)
			myRigidbody.velocity = theFirer.GetComponent<Rigidbody2D>().velocity;

			//NEW (allows for more accuracy with simplistic target lead equations
			//float platformSpeed = Mathf.Sqrt (theFirer.rigidbody2D.velocity.magnitude); //this is pretty meaningless without direction
			//rigidbody2D.AddForce (transform.up * shotSpeed);
			//rigidbody2D.AddForce (transform.up * platformSpeed);
		}
		myRigidbody.AddForce (transform.right * leftOrRight);

		if(assignedTarget == null)
		{
			CheckForTarget();
		}
	}

	void CheckForTarget()
	{
		Collider2D hit;
		
		hit = Physics2D.OverlapCircle(transform.position + transform.up * 12f, 10f, targetSeekMask);
		if(hit != null)
		{
			target = hit.gameObject;
		}
		else
		{
			if(target == null && Time.time < startTime + kickOffTime + fuelLifetime)
			{
				Invoke ("CheckForTarget", 0.1f);
			}
		}
	}

	IEnumerator TurnOnSmoke(float waitTime, float leftOrRight)
	{
		yield return new WaitForSeconds (waitTime);
		myRigidbody.AddForce (transform.right * -leftOrRight/2);
		smoke.gameObject.SetActive (true);
		GetComponent<CircleCollider2D> ().enabled = true;
		GetComponent<SpriteRenderer> ().sortingLayerName = "Projectiles";
	}

	void FixedUpdate () 
	{
		if (dead)
			return;

		//start looking immediately, to reduce curving, for as long as we have fuel
		if(Time.time < startTime + fuelLifetime)
		{
			if(target != null)
			{
				LookAtTarget(target.transform.position);
			}
		}

		if(missileType == MissileType.FireAndForget)
		{
			//start moving after a short delay until fuel runs out
			if(Time.time >= startTime + kickOffTime && Time.time < startTime + kickOffTime + fuelLifetime)
			{
				myRigidbody.velocity += (Vector2)transform.up * thrustPower * Time.deltaTime;
			}
			else if(Time.time >= startTime + kickOffTime + fuelLifetime)
			{
				smoke.Stop();
			}
		}
		else if(missileType == MissileType.LockOn)
		{
			//start moving after a short delay until fuel runs out
			if(Time.time >= startTime + kickOffTime && Time.time < startTime + kickOffTime + fuelLifetime)
			{
				speed += Time.deltaTime * acceleration;
				/*if(target != null && target.GetComponent<Collider2D>().enabled)
				{
					transform.position += (target.transform.position - transform.position).normalized * speed;
				}
				else
				{
					transform.position += transform.up * speed;
				}*/
			}
			//if we're out of fuel but haven't detonated yet, keep momentum
			else if(Time.time > startTime + kickOffTime + fuelLifetime)
			{
				//print(myRigidbody.velocity);
			}
			transform.position += transform.up * speed;

		}

		if(Time.time >= startTime + kickOffTime + fuseLifetime)
		{
			Explode();

			//this is here, not in Explode() because we may want the craft to spawn explosion. Otherwise there will be 2 explosions
			Tools.instance.SpawnExplosion(gameObject, transform.position, true); 
		}
	}
	
	public void LookAtTarget(Vector2 pos)
	{
		Vector3 dir = (Vector3)pos - transform.position; 
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		
		if(!System.Single.IsNaN(angle) && !ClickToPlay.instance.paused)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, (Time.deltaTime * turningPower));
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Fighter" || other.tag == "PlayerFighter")
		{
			//the other object will tell this to explode or not based on whether the missile was dodged
			other.GetComponent<HealthFighter>().YouveBeenHit(theFirer, this.gameObject, damage, 0);
		}
		else if(other.tag == "Asteroid")
		{
			other.GetComponent<Asteroid>().YouveBeenHit(damage * 10, this.gameObject);
		}
	}

	public void Explode()
	{
		dead = true;

		GetComponent<SpriteRenderer> ().enabled = false;
		GetComponent<CircleCollider2D> ().enabled = false;
		myRigidbody.velocity = Vector2.zero;
		smoke.Stop();

		Destroy (gameObject, 2); //delay for audio purposes
	}
}

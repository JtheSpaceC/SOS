using UnityEngine;
using System.Collections;

public class WeaponsTurret : TargetableObject {
	
	ObjectPoolerScript cannonShotPoolerScript;

	Collider2D[] targetArray;
	
	GameObject obj1;
	
	GameObject manualCrosshairs;

	[Tooltip("Can be null, because a default is set up, but you can set any projectile here")]
	public GameObject weaponTypeFromObjectPoolList;

	public Transform target;
	public Transform shotSpawn1;	
	
	private GameObject theFirer;
	
	//private bool targetInSight = false;
	public bool allowedToFire = true;
	
	public float fireRate = 0.33f;
	private float nextFire;
	public float weaponsRange = 17f;
	public float shotDamage = 25f;
	public float shotCritChance = 10f;
	public float projectileSpeed = 9f;
	public LayerMask targetsMask;

	public bool automaticControl = true;
	public bool manualControl = false;
	
	public float NormalRotateSpeed = 1f;
	public float FiringRotateSpeedInDegreesPerSecond = 180f;
	private int rotateDirection;
	
	//for FIRING SOLUTION
	private float bulletToTargetTime;
	private float shooterToTargetDistance;
	public Vector2 newTargetPosition;
	private float shotSpeedAvg = 18f; //for predicting target lead time

	float timer = 0;
	
	
	void Awake()
	{
		theFirer = this.transform.parent.parent.gameObject;

		if(weaponTypeFromObjectPoolList == null)
		{
			weaponTypeFromObjectPoolList = GameObject.Find("Cannon Shot Pooler");
		}
		cannonShotPoolerScript = weaponTypeFromObjectPoolList.GetComponent<ObjectPoolerScript> ();
		
		rotateDirection = Random.Range (1, 3);
		if(rotateDirection == 2)
			rotateDirection = -1;
		
		if(manualControl)
		{
			manualCrosshairs = GameObject.FindGameObjectWithTag("CrosshairCursor");
			manualCrosshairs.GetComponent<SpriteRenderer>().enabled = true;
		}		
	}
	
		
	void Update () 
	{
		if(automaticControl && !ClickToPlay.instance.paused)
		{
			timer += Time.deltaTime;

			if(target != null)
			{
				if(timer >= 0.5f)
				{
					timer = 0;
					CheckTargetStatus();
				}
				if(target != null)
				{
					LookAtTarget();
				}
			}			
			else
			{
				SelectATarget();
			}
						
			//for Aiming
			if(target != null && Time.time > nextFire && 
			   (ReadyAimFire(target.gameObject, shotSpawn1, weaponsRange) == true || TakePotshot(shotSpawn1, weaponsRange) == true))
			{
				if(allowedToFire)
				{
					FirePrimary();
				}
			}
	
		}//end of Automatic Control
		
		else if(manualControl)
		{
			target = manualCrosshairs.transform;
			LookAtTarget();
			
			if(Input.GetMouseButton(0) && Time.time >= nextFire)
			{
				nextFire = Time.time + fireRate;
				
				FirePrimary();
			}
			
		}//end of Manual Control
		
	}//end of Update
	
	
	void FixedUpdate()
	{
		if(manualControl)
		{
			float mousex = (Input.mousePosition.x);
			float mousey = (Input.mousePosition.y);
			Vector2 mouseposition = Camera.main.ScreenToWorldPoint(new Vector3 (mousex,mousey,0));
			manualCrosshairs.transform.position = mouseposition;
		}
	}
	
	void CheckTargetStatus()
	{
		if(target.tag == "Asteroid")
		{
			if(Vector2.Distance(transform.position, target.position) > weaponsRange || !target.gameObject.activeSelf)
			{
				//targetInSight = false;
				target = null;
			}
		}
		else if(target.GetComponent<HealthFighter>().dead || Vector2.Distance(transform.position, target.position) > weaponsRange)
		{
			//targetInSight = false;
			target = null;
		}
	}
	
	
	void SelectATarget()
	{
		targetArray = Physics2D.OverlapCircleAll(transform.position, weaponsRange, targetsMask); //mask is set to Friendly(orEnemy)Fighters + Asteroids
		if(targetArray.Length > 0)
		{
			foreach(Collider2D potentialTarget in targetArray)
			{
				if(potentialTarget.tag == "Fighter" || potentialTarget.tag == "PlayerFighter")
				{
					if(CheckTargetIsLegit(potentialTarget.gameObject))
					{
						CheckAndAddTargetToCommanderList(myCommander, potentialTarget.gameObject);

						if(target == null)
							target = potentialTarget.transform;
					}
				}
			}
			if(target == null)
			{
				target = targetArray[Random.Range(0,targetArray.Length)].transform;
			}
		}
		
		else if(targetArray.Length == 0)
		{
			target = null;
			//targetInSight = false;
			Idle();
		}
	}
	
	void GetFiringSolution()
	{
		if(target != null)
		{
			shooterToTargetDistance = (newTargetPosition - (Vector2)transform.position).magnitude;
			
			bulletToTargetTime = shooterToTargetDistance / shotSpeedAvg;
			
			if(System.Single.IsNaN(bulletToTargetTime))
			{
				newTargetPosition = new Vector2 (target.position.x, target.position.y);
				Debug.Log("TURRET Rotation Error Overcome");
			}
			else
			{
				newTargetPosition = (Vector2)target.position + 
					(target.GetComponent<Rigidbody2D>().velocity * bulletToTargetTime) - 
						(transform.root.GetComponent<Rigidbody2D>().velocity * bulletToTargetTime);
			}
		}
	}
	
	
	void LookAtTarget()
	{
		if (target != null && automaticControl)
		{
			if (((IList)targetArray).Contains(target.GetComponent<Collider2D>()) || target.tag == "Asteroid")		
			{
				GetFiringSolution ();
				targetLook = newTargetPosition;
			}
		}
		else if(manualControl)
		{
			targetLook = manualCrosshairs.transform.position;
		}
		
		Vector3 dir = targetLook - transform.position; 
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 45;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		
		if(!System.Single.IsNaN(angle))
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, Time.deltaTime * FiringRotateSpeedInDegreesPerSecond);
		}
	}
	
	
	void FirePrimary()
	{
		if (Time.time < nextFire || ClickToPlay.instance.paused)
			return;
		
		nextFire = Time.time + fireRate;

		if(shotSpawn1 != null)
		{
			obj1 = cannonShotPoolerScript.current.GetPooledObject();
						
			//not needed if Will Grow is true
			//if (obj1 == null) return;
			
			obj1.transform.position = shotSpawn1.position;
			obj1.transform.rotation = shotSpawn1.rotation;
			obj1.SetActive(true);
			obj1.GetComponent<ShotMover>().OkayGo(theFirer, shotDamage, shotCritChance, projectileSpeed);
		}
	}
	
	
	void Idle()
	{
		transform.Rotate (Vector3.forward, (NormalRotateSpeed/8 * rotateDirection * Random.Range(0.5f, 2)));
	}
	
	
}//MONO

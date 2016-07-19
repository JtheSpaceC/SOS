using UnityEngine;
using System.Collections;

public class Asteroid : MonoBehaviour {

	ObjectPoolerScript asteroidPoolerScript;

	public AsteroidSpawner myAsteroidSpawner;

	Rigidbody2D myRigidbody;

	public enum AsteroidSize {Large, Medium, Small};
	public AsteroidSize asteroidSize;
	AsteroidSize[] allSizes;

	public float health = 60f;
	float startingHealth;

	public int damageIfSmall = 1;
	public int damageIfMedium = 2;
	public int damageIfLarge = 3;
	int damage;
	//float brightnessLevel;

	public bool transitionFromLightToDark = true;
	public Color darkestColorAllowed;
	float newColValue = 100;
	float coloringSpeed;
	Color startColor;

	SpriteRenderer myRenderer;
	Collider2D myCollider;

	private float rotateSpeed;
	private Vector2 pushDirection;
	private int force;
	[Header("Rotation Speeds Possible")]
	public int minRange = 1000;
	public int maxRange = 8000;

	private HealthFighter FighterHealth;

	//private DefensiveGunHealth defensiveGunHealth;

	private float diceRoll;
	
	[Header("For Camera Shake")]
	private float amplitude = 0.1f;
	private float duration = 0.5f;
	
	private float startingMass;
	private Rigidbody2D otherRigidbody;
	private Vector2 otherVelocity;
	private Vector2 relativeVelocity;

	private bool asteroidDestroyed;
	

	void Awake () 
	{
		asteroidPoolerScript = GameObject.Find ("asteroid Pooler").GetComponent<ObjectPoolerScript> ();

		myRigidbody = GetComponent<Rigidbody2D> ();
		myCollider = GetComponent<Collider2D>();
		myRenderer = GetComponent<SpriteRenderer>();

		allSizes = new AsteroidSize[]{AsteroidSize.Large, AsteroidSize.Medium, AsteroidSize.Small};
		startingHealth = health;
		startingMass = myRigidbody.mass;

		startColor = myRenderer.color;
		coloringSpeed = Random.Range (10, 21);
	}

	void OnEnable()
	{
		CancelInvoke("EnableCollider");
		asteroidDestroyed = false;
		SetCharacteristics (true);
		StartCoroutine (CheckDespawn());
	}

	void EnableCollider()
	{
		myCollider.enabled = true;
	}
	
	void SetCharacteristics(bool setRandomNewSize)
	{
		myCollider.enabled =false;
		Invoke("EnableCollider", 1f);

		myRenderer.enabled =true;
		myRenderer.color = startColor;
		coloringSpeed = Mathf.Abs (coloringSpeed);
		
		rotateSpeed = Random.Range (-180, 180);
		
		pushDirection = Random.insideUnitCircle;
		force = Random.Range (minRange, maxRange);
		myRigidbody.AddForce (pushDirection * force);

		//brightnessLevel = Random.Range (100, 255);
		//brightnessLevel /= 255;

		//myRenderer.color = new Color (brightnessLevel, brightnessLevel, brightnessLevel);

		if(setRandomNewSize)
			asteroidSize = allSizes[Random.Range(0, allSizes.Length)];

		if(asteroidSize == AsteroidSize.Large)
		{
			transform.localScale = new Vector2 (3, 3);
			myRigidbody.mass = startingMass * 3; //should be squared, but I double so largest asteroids don't kill you outright
			health = startingHealth * 3; //again, should be squared, but for pity's sake... it's not
			damage = damageIfLarge;
		}
		else if(asteroidSize == AsteroidSize.Medium)
		{
			transform.localScale = new Vector2 (2, 2);
			myRigidbody.mass = startingMass * 2;
			health = startingHealth * 2;
			damage = damageIfMedium;
		}
		else if(asteroidSize == AsteroidSize.Small)
		{
			transform.localScale = new Vector2 (1, 1);
			myRigidbody.mass = startingMass;
			health = startingHealth;
			damage = damageIfSmall;
		}		
	}
	
	void Update()
	{
		transform.Rotate (new Vector3 (0, 0, rotateSpeed) * Time.deltaTime);

		//this darkens and lightens the asteroid based on its rotation
		//newCol = (100 + (Mathf.Abs (transform.rotation.z)* 150))/255;
		//myRenderer.color = new Color (newCol, newCol, newCol);

		if(transitionFromLightToDark)
		{
			newColValue += Time.deltaTime * coloringSpeed;
			if (newColValue / 255 > 1) 
			{
				newColValue = 255;
				coloringSpeed *= -1;	
			}
			else if (newColValue / 255 <= 0.4f)
			{
				newColValue = 255 * 0.4f;
				coloringSpeed *= -1;
			}

			//myRenderer.color = new Color (newCol/255, newCol/255, newCol/255);
			myRenderer.color = Color.Lerp(startColor, darkestColorAllowed, newColValue/255);
		}
		
		if(health <= 0f)
		{
			if(asteroidDestroyed == false)
			StartCoroutine(DestroyAsteroid());
		}
	}

	IEnumerator CheckDespawn()
	{
		yield return new WaitForSeconds (0.5f);

		if (Vector2.Distance (transform.position, Camera.main.transform.position) >= 60f)
		{	
			if(myAsteroidSpawner == null)
				Debug.LogError("Asteroid has no assigned Spawner");

			myAsteroidSpawner.asteroidCount--;
			gameObject.SetActive(false);
		}
		yield return new WaitForSeconds (0.5f);
		StartCoroutine (CheckDespawn());
	}
	
	public void YouveBeenHit(float damageAmount, GameObject theProjectile)
	{
		Tools.instance.SpawnAsteroidPoof(transform.position);

		health -= damageAmount;
		if(theProjectile.tag != "Bomb")
			theProjectile.SendMessage("HitAndStop");
		else
		{
			Tools.instance.SpawnExplosion(theProjectile, theProjectile.transform.position + theProjectile.transform.up * 0.5f, false);
			theProjectile.GetComponent<Missile>().Explode();
		}
	}

	public IEnumerator DestroyAsteroid()
	{
		asteroidDestroyed = true;

		myCollider.enabled =false;
		myRenderer.enabled =false;
		
		myAsteroidSpawner.asteroidCount --;

		if(asteroidSize == AsteroidSize.Large)
		{
			Tools.instance.SpawnAsteroidPoofBig(transform.position, 3, myRigidbody.velocity);
		}
		else if(asteroidSize == AsteroidSize.Medium)
		{
			Tools.instance.SpawnAsteroidPoofBig(transform.position, 2, myRigidbody.velocity);
		}
		else if(asteroidSize == AsteroidSize.Small)
		{
			Tools.instance.SpawnAsteroidPoofBig(transform.position, 1, myRigidbody.velocity);
		}	


		if (asteroidSize == AsteroidSize.Medium) 
		{
			SpawnGibs(AsteroidSize.Small, 2);
		}
		else if(asteroidSize == AsteroidSize.Large)
		{
			SpawnGibs(AsteroidSize.Medium, 2);
		}

		if(!GetComponent<AudioSource>().isPlaying)
		{
			GetComponent<AudioSource>().Play();
		}

		if(GetComponent<AudioSource>().isPlaying)
		{
			yield return new WaitForSeconds (0.5f);
		}
		gameObject.SetActive (false);		
	}

	void OnDisable()
	{
		StopAllCoroutines ();
		myRigidbody.mass = startingMass;
		health = startingHealth;
	}

	void SpawnGibs(AsteroidSize whichSize, int numberOfGibs)
	{
		for (int i = 0; i < numberOfGibs; i++) 
		{
			Vector3 spawnPosition = transform.position;
		
			GameObject obj = asteroidPoolerScript.current.GetPooledObject ();
			obj.transform.position = spawnPosition;
			obj.SetActive (true);
			obj.GetComponent<Asteroid> ().asteroidSize = whichSize;
			obj.GetComponent<Asteroid> ().SetCharacteristics(false);
			obj.GetComponent<Asteroid>().myAsteroidSpawner = this.myAsteroidSpawner;

			myAsteroidSpawner.asteroidCount ++;
		}
	}
	


	void OnTriggerEnter2D (Collider2D other) 
	{
		if (other.tag == "PlayerFighter" || other.tag == "Fighter") 
		{
			other.GetComponent<HealthFighter>().YouveBeenHit(gameObject, gameObject, damage, 0);
		}
		else if (other.tag == "Turret") 
		{
			diceRoll = Random.Range(1,11);
			if(diceRoll >=2)                  // this reflects a 10% miss rate even when the bullets meet the target. 
			{
				HealthTurret healthTurretScript = other.GetComponent<HealthTurret>();
				healthTurretScript.YouveBeenHit(gameObject, gameObject, damage, 0);
				StartCoroutine(DestroyAsteroid());
			}
			else
			{return;}			
		}

	/*	else if(other.tag == "CapShipArmour")
		{
			diceRoll = Random.Range (1,11);
			if(diceRoll >5) //50% miss rate
			{
				StartCoroutine(DestroyAsteroid());
				other.SendMessage("Crack",SendMessageOptions.DontRequireReceiver);
				other.SendMessage("BreakUp",SendMessageOptions.DontRequireReceiver);
			}
		
			else
			{return;}
		}*/

		/*else if (other.tag == "GunEmplacement") 
		{
			diceRoll = Random.Range(1,11);
			if(diceRoll >=2)                  // this reflects a 10% miss rate even when the bullets meet the target. 
			{
				defensiveGunHealth = other.GetComponent<DefensiveGunHealth>();
				defensiveGunHealth.TakeDamage(300f, gameObject);
				StartCoroutine(DestroyAsteroid());
			}
			else
			{return;}
			
		}*/

		/*else if (other.tag == "Bomb")
		{
			other.SendMessage("Detonate");
			StartCoroutine(DestroyAsteroid());
		}*/

	}//end of OnTriggerEnter2D

	public void CamShake()
	{
		CameraShake.instance.Shake(amplitude,duration);
	}

	void ReportActivity()
	{
		CameraTactical.reportedInfo = "Asteroid.\n" +
			"Size: " + asteroidSize;
	}
	

} //Mono

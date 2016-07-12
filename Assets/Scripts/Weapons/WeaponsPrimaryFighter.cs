using UnityEngine;
using System.Collections;

public class WeaponsPrimaryFighter : MonoBehaviour {

	ObjectPoolerScript cannonShotPoolerScript;
	SquadronLeader playerSquadLeaderScript;

	GameObject player;

	int maxAmmo = 1200;
	public int ammo = 900;

	public bool playerControlled = false;
	public bool allowedToFire = false;
	public GameObject weaponTypeFromObjectPoolList;

	bool hammerDown = false;

	public Transform shotSpawn1;
	public Transform shotSpawn2;
	public Transform shotSpawn3;
	public Transform shotSpawn4;

	public bool automaticWeapon = true;
	public float fireRate = 0.33f;
	[HideInInspector] public float nextFire;
	public float weaponsRange = 17;
	public int shotDamage = 1;
	public float shotCritChance = 10f;
	public float projectileSpeed = 7f;

	GameObject theFirer;

	
	void Awake()
	{
		theFirer = this.transform.parent.parent.gameObject;

		if(weaponTypeFromObjectPoolList == null)
		{
			weaponTypeFromObjectPoolList = GameObject.Find("Cannon Shot Pooler");
		}
		cannonShotPoolerScript = weaponTypeFromObjectPoolList.GetComponent<ObjectPoolerScript> ();

	}

	void OnEnable()
	{
		shotSpawn1.gameObject.SetActive(true);
		shotSpawn2.gameObject.SetActive(true);
	}

	void OnDisable()
	{
		shotSpawn1.gameObject.SetActive(false);
		shotSpawn2.gameObject.SetActive(false);
	}

	void Start()
	{
		ammo = Mathf.Clamp(ammo, 0, maxAmmo);

		if(playerControlled && ClickToPlay.instance.disablePlayerSelectButtonForMenu)
		{
			ClickToPlay.instance.playerShootScript = this.GetComponent<WeaponsPrimaryFighter>();
		}
		if(playerControlled)
		{
			player = GameObject.FindGameObjectWithTag("PlayerFighter");
			playerSquadLeaderScript = player.GetComponentInChildren<SquadronLeader>();

			Tools.instance.ammoRemainingSlider.maxValue = maxAmmo;
			Tools.instance.ammoRemainingSlider.value = ammo;
			Tools.instance.ammoRemainingText.text = "Ammo: " + ammo + " / " + maxAmmo;
		}
	}
	
	
	void Update () 
	{
		//for Shooting

		if(playerControlled && allowedToFire && Input.GetButtonDown("FirePrimary"))
		{
			hammerDown = true;
		}

		if (playerControlled && hammerDown && Input.GetButton ("FirePrimary")) 
		{
			FirePrimary(false);

			if(!automaticWeapon)
			{
				hammerDown = false;
			}
			if(playerSquadLeaderScript.firstFlightOrders == SquadronLeader.Orders.FormUp)
			{
				if(playerSquadLeaderScript.mate02)
				{
					playerSquadLeaderScript.mate02.SendMessage("PloughTheRoad", SendMessageOptions.DontRequireReceiver);
				}
				if(playerSquadLeaderScript.mate03)
				{
					playerSquadLeaderScript.mate03.SendMessage("PloughTheRoad", SendMessageOptions.DontRequireReceiver);
				}			
			}
		}

		if(playerControlled && Input.GetButtonUp("FirePrimary"))
		{
			hammerDown = false;
		}
	}
	
	
	public void FirePrimary(bool ploughTheRoad)
	{
		if (Time.time < nextFire || ClickToPlay.instance.paused || ammo <= 0)
			return;

		nextFire = Time.time + fireRate;

		if (!playerControlled && !ploughTheRoad)
			nextFire = Time.time + (fireRate * Random.Range (1, 6));
		else if (playerControlled)
		{
			CameraShake.instance.Shake (0.05f, 0.1f);
			Tools.instance.VibrateController(0, .25f, .25f, 0.1f);
		}

		if(shotSpawn1 != null)
		{
			GameObject obj1 = cannonShotPoolerScript.current.GetPooledObject();
			
			//not needed if Will Grow is true
			//if (obj1 == null) return;			
			
			obj1.transform.position = shotSpawn1.position;

			Quaternion rot = shotSpawn1.rotation;
			rot.x = 0;
			rot.y = 0;
			obj1.transform.rotation = rot;

			obj1.SetActive(true);
			obj1.GetComponent<ShotMover>().OkayGo(theFirer, shotDamage, shotCritChance, projectileSpeed);

			ammo--;
		}
		
		
		if(shotSpawn2 != null && ammo > 0)
		{
			GameObject obj2 = cannonShotPoolerScript.current.GetPooledObject();
			
			obj2.transform.position = shotSpawn2.position;

			Quaternion rot = shotSpawn2.rotation;
			rot.x = 0;
			rot.y = 0;
			obj2.transform.rotation = rot;

			obj2.SetActive(true);
			obj2.GetComponent<ShotMover>().OkayGo(theFirer, shotDamage, shotCritChance, projectileSpeed);

			ammo--;
		}

		if(shotSpawn3 != null && ammo > 0)
		{
			GameObject obj3 = cannonShotPoolerScript.current.GetPooledObject();

			obj3.transform.position = shotSpawn3.position;

			Quaternion rot = shotSpawn3.rotation;
			rot.x = 0;
			rot.y = 0;
			obj3.transform.rotation = rot;

			obj3.SetActive(true);
			obj3.GetComponent<ShotMover>().OkayGo(theFirer, shotDamage, shotCritChance, projectileSpeed);

			ammo--;
		}

		if(shotSpawn4 != null && ammo > 0)
		{
			GameObject obj4 = cannonShotPoolerScript.current.GetPooledObject();

			obj4.transform.position = shotSpawn4.position;

			Quaternion rot = shotSpawn4.rotation;
			rot.x = 0;
			rot.y = 0;
			obj4.transform.rotation = rot;

			obj4.SetActive(true);
			obj4.GetComponent<ShotMover>().OkayGo(theFirer, shotDamage, shotCritChance, projectileSpeed);

			ammo--;
		}

		if(playerControlled)
		{
			Tools.instance.ammoRemainingSlider.value = ammo;
			Tools.instance.ammoRemainingText.text = "Ammo: " + ammo + " / " + maxAmmo;
		}
	}

	public void InvokeAllowedToFire(){StartCoroutine (AllowedToFire ());}
	public IEnumerator AllowedToFire()
	{
		yield return new WaitForEndOfFrame();
		allowedToFire = true;
	}
	
}//MONO

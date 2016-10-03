using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponsSecondaryFighter : MonoBehaviour {

	public bool playerControlled = false;
	public GameObject projectilePrefab;
	public LayerMask targetSeekMask;
	public int ammo = 4;
	public Text ammoText;

	public enum LockOnMode {FireAndForget, Dumbfire, LockOn}
	public LockOnMode lockOnMode;

	public GameObject target;
	public GameObject targetingPip;
	public float rotSpeed = 50;
	GameObject theFirer;


	void Start()
	{
		theFirer = this.transform.parent.parent.gameObject;

		if(transform.root.gameObject.tag == "PlayerFighter")
		{
			ammoText = GameObject.Find("GUI Missile Ammo").GetComponent<Text>();
			ammoText.text = "Missiles: " + "<color>" + ammo + "</color>";
		}
		if (!playerControlled)
			InvokeRepeating ("LaunchProjectile", 1, 1);
	}

	void Update () 
	{
		if( ClickToPlay.instance.paused)
			return;

		if(lockOnMode == LockOnMode.FireAndForget || lockOnMode == LockOnMode.Dumbfire)
		{
			if(playerControlled && Input.GetButtonDown("FireSecondary"))
			{
				LaunchProjectile();
			}
		}
		else if(playerControlled && lockOnMode == LockOnMode.LockOn)
		{
			//TODO: Tap to lock on. Double tap to clear. Hold to fire
			if(Input.GetButtonDown("FireSecondary"))
			{
				if(target == null)
					LockOn();
				else 
					LaunchProjectile();
			}

			if(target != null)
			{
				if(target.GetComponent<Collider2D>().enabled == false
					|| Vector2.Angle(transform.up, target.transform.position - transform.position) > 60)
				{
					ResetPipAndNullifyTarget();
				}
				else
				{
					targetingPip.SetActive(true);
					targetingPip.transform.SetParent(null);
					targetingPip.transform.position = target.transform.position;
				}
			}
		}

		//REMOVE:
		//FOR PROTOTYPING ASTEROID SCREEN-LOCK
		if(target != null)
		{
			if(Input.GetKeyDown(KeyCode.E))
			{
				//Change the Main Camera's behaviour to Asteroids-box
				Tools.instance.combatAsteroidsStyleScript.itemsInZone.Add(transform.root.gameObject);
				Tools.instance.combatAsteroidsStyleScript.itemsInZone.Add(target);
				Tools.instance.combatAsteroidsStyleScript.enabled = true;
			}

		}
	}

	public void LockOn()
	{
		Collider2D hit;
		
		hit = Physics2D.OverlapCircle(transform.position + transform.up * 12f, 10f, targetSeekMask);
		if(hit != null)
		{
			target = hit.gameObject;
			GetComponent<AudioSource>().Play();
		}
	}

	public void LaunchProjectile()
	{
		if(ammo <= 0)
			return;
		
		GameObject missile = Instantiate(projectilePrefab, transform.position, transform.rotation) as GameObject;
		Missile missileScript = missile.GetComponent<Missile> ();
		missileScript.theFirer = this.theFirer;

		if (lockOnMode == LockOnMode.FireAndForget)
			missileScript.missileType = Missile.MissileType.FireAndForget;
		else if (lockOnMode == LockOnMode.LockOn)
			missileScript.missileType = Missile.MissileType.LockOn;
		else if (lockOnMode == LockOnMode.Dumbfire)
			missileScript.missileType = Missile.MissileType.Dumbfire;

		if(ammo %2 == 0)
		{
			missileScript.OkayGo (theFirer, -3, target);
		}
		else
		{
			missileScript.OkayGo (theFirer, 3, target);
		}

		//if that was our last missile, turn off the script
		ammo--;
		if(playerControlled)
		{
			Tools.instance.VibrateController(0, .25f, .25f, 0.1f);
			ammoText.text = "Missiles: " + ammo;

			if(ammo >= 1)
				StartCoroutine (Tools.instance.TextAnim(Tools.instance.missilesRemainingText, Color.red, Color.white, 0.5f));

			else if(ammo <= 0)
			{
				StartCoroutine (Tools.instance.TextAnim(Tools.instance.missilesRemainingText, Color.red, Color.red, 0.5f));
				GetComponent<WeaponsSecondaryFighter>().enabled = false;
			}
		}
		ResetPipAndNullifyTarget ();
	}

	void ResetPipAndNullifyTarget()
	{
		targetingPip.transform.SetParent (gameObject.transform);
		target = null;
		targetingPip.SetActive (false);
	}
}

using UnityEngine;
using System.Collections;

public class WeaponFunctions : MonoBehaviour {

 	protected ObjectPoolerScript cannonShotPoolerScript;

	public enum WeaponType {DualCannons, TurretFighter, TurretCapital};
	public WeaponType weaponType;

	public bool playerControlled = false;
	public bool allowedToFire = false;
	public GameObject weaponTypeFromObjectPoolList;
	
	protected bool hammerDown;

	public Transform shotSpawn1;
	public Transform shotSpawn2;
	
	public bool automaticWeapon = true;
	public float fireRate = 0.33f;
	protected float nextFire;
	public float weaponsRange = 17;


	void Awake()
	{
		if(weaponTypeFromObjectPoolList == null)
		{
			weaponTypeFromObjectPoolList = GameObject.Find("Cannon Shot Pooler");
		}
		cannonShotPoolerScript = weaponTypeFromObjectPoolList.GetComponent<ObjectPoolerScript> ();
	}
 

}//MONO


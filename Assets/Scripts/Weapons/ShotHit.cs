using UnityEngine;
using System.Collections;

public class ShotHit : MonoBehaviour {

	public GameObject theFirer;

	private float diceRoll;
	public int normalDamage = 1;
	[HideInInspector] public int defaultAvgDamage;
	public float chanceToCrit = 25;
	[HideInInspector] public float defaultChanceToCrit;

	[Header("For Camera Shake")]
	public float amplitude = 0.1f;
	public float duration = 0.5f;

	[HideInInspector] public float accuracy = 1;


	void Awake()
	{
		defaultAvgDamage = normalDamage;
		defaultChanceToCrit = chanceToCrit;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Fighter" || other.tag == "PlayerFighter")
		{
			if(other.GetComponent<HealthFighter>().enabled)
				other.GetComponent<HealthFighter>().YouveBeenHit(theFirer, this.gameObject, normalDamage, chanceToCrit, accuracy);
		}
		else if(other.tag == "Turret")
		{
			other.GetComponent<HealthTurret>().YouveBeenHit(theFirer, this.gameObject, normalDamage, chanceToCrit);
		}
		else if(other.tag == "Transport")
		{
			other.GetComponent<HealthTransport>().YouveBeenHit(theFirer, this.gameObject, normalDamage, chanceToCrit);
		}
		else if(other.tag == "Asteroid")
		{
			other.GetComponent<Asteroid>().YouveBeenHit(normalDamage, this.gameObject);
		}
	}


	public void CamShake()
	{
		CameraShake.instance.Shake(amplitude, duration);
	}


	void OnDisable()
	{
		normalDamage = defaultAvgDamage;
		chanceToCrit = defaultChanceToCrit;
	}
	
}

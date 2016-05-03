﻿using UnityEngine;
using System.Collections;

public class ShotHit : MonoBehaviour {

	public GameObject theFirer;

	private float diceRoll;
	public float averageDamage = 25f;
	[HideInInspector] public float defaultAvgDamage;
	public float chanceToCrit = 25;
	[HideInInspector] public float defaultChanceToCrit;

	[Header("For Camera Shake")]
	public float amplitude = 0.1f;
	public float duration = 0.5f;


	void Awake()
	{
		defaultAvgDamage = averageDamage;
		defaultChanceToCrit = chanceToCrit;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Fighter" || other.tag == "PlayerFighter")
		{
			other.GetComponent<HealthFighter>().YouveBeenHit(theFirer, this.gameObject, averageDamage, chanceToCrit);
		}
		else if(other.tag == "Turret")
		{
			other.GetComponent<HealthTurret>().YouveBeenHit(theFirer, this.gameObject, averageDamage, chanceToCrit);
		}
		else if(other.tag == "Transport")
		{
			other.GetComponent<HealthTransport>().YouveBeenHit(theFirer, this.gameObject, averageDamage, chanceToCrit);
		}
		else if(other.tag == "Asteroid")
		{
			other.GetComponent<Asteroid>().YouveBeenHit(averageDamage, this.gameObject);
		}
	}


	public void CamShake()
	{
		CameraShake.instance.Shake(amplitude, duration);
	}


	void OnDisable()
	{
		averageDamage = defaultAvgDamage;
		chanceToCrit = defaultChanceToCrit;
	}
	
}
using UnityEngine;
using System.Collections;

public class PlanetApproach : MonoBehaviour {

	public float approachRate;
	public float rotationRate;
	float actualRotationRate;
	
	public float startScaleX = 1;
	public float startScaleY = 1;
	public float startScaleZ = 1;
	
	public float maxScale = 30;

	public bool canApproachPlanet;
	public bool canRotate;
	public bool orbitAfterApproach;
	bool orbiting = false;
	
	void Awake ()
	{
		startScaleX = transform.localScale.x;
		startScaleY = transform.localScale.y;	
		startScaleZ = transform.localScale.z;
		GetComponent<Renderer>().sortingLayerName = "Planets";
		actualRotationRate = rotationRate / 10;
	}
	
	
	void Update () 
	{
		transform.Rotate (new Vector3 (0, actualRotationRate, 0) * Time.deltaTime);
	}
	
	void FixedUpdate ()
	{
		if(canApproachPlanet == true)
		{
			if(approachRate != 0 && transform.localScale.x < maxScale)
			{
				float realApproachRate = approachRate * Mathf.Pow(transform.localScale.x, 2) / Mathf.Pow (maxScale, 2);

				transform.localScale = (new Vector3 (startScaleX += realApproachRate * Time.deltaTime, startScaleY += realApproachRate * Time.deltaTime, startScaleZ += realApproachRate * Time.deltaTime));
			}
		}
		if(orbitAfterApproach && !orbiting && transform.localScale.x >= maxScale)
		{
			StartToOrbit();
		}
	}

	void StartToOrbit()
	{
		GameObject.Find ("Directional light (Sun)").transform.parent = this.gameObject.transform;
		actualRotationRate = rotationRate;
	}
}

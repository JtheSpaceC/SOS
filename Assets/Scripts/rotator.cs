using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

	public enum AxisMovement {Z_only, ThreeAxes};
	public AxisMovement axisMovement;

	public enum RotationMode {Constant, RandomizedAtStart, StartingRotationOnly, BasedOnStellarTime};
	public RotationMode rotationMode;

	float x = 0;
	float y = 0;
	float z = 0;
	Vector3 rot;

	[Header("If Constant")]
	public float rotSpeed = 10;

	[Header("If Randomized")]
	public float minSpeed = 10;
	public float maxSpeed = 360;

	[Tooltip("If you don't want to risk zero movement (by using negative to positive min/max values), "
	+"but do want random left/right rotation, set this to True")]
	public bool randomizeDirection = false;


	void Start()
	{
		if(rotationMode == RotationMode.RandomizedAtStart)
		{
			rotSpeed = Random.Range(minSpeed, maxSpeed);
		}
			
		if(randomizeDirection)
			rotSpeed *= Mathf.Pow(-1, Random.Range(1, 3));

		z = transform.eulerAngles.z;

		if(rotationMode == RotationMode.StartingRotationOnly)
		{
			if(axisMovement == AxisMovement.ThreeAxes)
			{
				x = Random.Range(minSpeed, maxSpeed);
				y = Random.Range(minSpeed, maxSpeed);
			}
			z = Random.Range(minSpeed, maxSpeed);
			rot = new Vector3 (x, y, z);
			transform.rotation = Quaternion.Euler (rot);	
			this.enabled = false;
		}
	}

	void FixedUpdate () 
	{
		if(rotationMode == RotationMode.Constant || rotationMode == RotationMode.RandomizedAtStart)
		{
			if(axisMovement == AxisMovement.ThreeAxes)
			{
				x += Time.deltaTime * rotSpeed;
				y += Time.deltaTime * rotSpeed;
			}
			z += Time.deltaTime * rotSpeed;
			rot = new Vector3 (x, y, z);
			transform.rotation = Quaternion.Euler (rot);
		}
		else if(rotationMode == RotationMode.BasedOnStellarTime)
		{
			z += Time.deltaTime * rotSpeed * RTSDirector.instance.gameSpeed;
			rot = new Vector3 (x, y, z);
			transform.rotation = Quaternion.Euler (rot);
		}				
	}
}

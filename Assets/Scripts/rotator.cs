using UnityEngine;
using System.Collections;

public class rotator : MonoBehaviour {

	public enum AxisMovement {Z_only};
	public AxisMovement axisMovement;

	public enum myMode {Constant, RandomizedAtStart, BasedOnStellarTime};
	public myMode Mode;

	float z;

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
		if(Mode == myMode.RandomizedAtStart)
		{
			rotSpeed = Random.Range(minSpeed, maxSpeed);
		}
			
		if(randomizeDirection)
			rotSpeed *= Mathf.Pow(-1, Random.Range(1, 3));
	}

	void FixedUpdate () 
	{
		if(axisMovement == AxisMovement.Z_only)
		{
			if(Mode == myMode.Constant || Mode == myMode.RandomizedAtStart)
			{
				z += Time.deltaTime * rotSpeed;
				Vector3 rot = new Vector3 (0, 0, z);
				transform.rotation = Quaternion.Euler (rot);
			}
			else if(Mode == myMode.BasedOnStellarTime)
			{
				z += Time.deltaTime * rotSpeed * RTSDirector.instance.gameSpeed;
				Vector3 rot = new Vector3 (0, 0, z);
				transform.rotation = Quaternion.Euler (rot);
			}
		}			
	}
}

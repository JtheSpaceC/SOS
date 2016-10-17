using UnityEngine;
using System.Collections;

public class UI_NonRotate : MonoBehaviour {
	
	public bool keepRotation = true;
	public bool rotationShouldBeZero = false;
	public bool keepRelativePosition = false;

	Quaternion rotation;
	Vector3 position;


	void Start ()
	{
		if(rotationShouldBeZero)
			rotation = Quaternion.identity;
		else
			rotation = transform.rotation;

		position = transform.localPosition;
	}
	
	void LateUpdate () 
	{
		if (keepRotation)
		transform.rotation = rotation;	

		if (keepRelativePosition)
			transform.position = transform.parent.position + position;
	}
}

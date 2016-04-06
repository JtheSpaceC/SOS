using UnityEngine;
using System.Collections;

public class UI_NonRotate : MonoBehaviour {
	
	public bool keepRotation = true;
	public bool keepRelativePosition = false;

	private Quaternion rotation;
	private Vector3 position;

	void Start ()
	{
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

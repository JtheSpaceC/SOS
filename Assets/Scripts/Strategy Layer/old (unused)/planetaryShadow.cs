using UnityEngine;
using System.Collections;

public class planetaryShadow : MonoBehaviour {

	Quaternion lookPos;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		lookPos = Quaternion.LookRotation(Vector3.zero - transform.position);
		//lookPos.x = 0;
		//lookPos.y = 0;
		//while (lookPos.z >=180)
		//	lookPos.z -= 90;
		transform.rotation = lookPos;
	}
}

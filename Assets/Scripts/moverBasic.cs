using UnityEngine;
using System.Collections;

public class moverBasic : MonoBehaviour {

	public float speed = 3;


	void FixedUpdate () 
	{
		transform.position += transform.up * Time.deltaTime * speed;
	}
}

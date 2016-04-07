using UnityEngine;
using System.Collections;

public class moverBasic : MonoBehaviour {

	[Tooltip("Apply the speed to the Rigidbody2D at start. " +
		"Doesn't use Update (cannot change speed during play). If False, FixedUpdate is used.")]
	public bool applyForceAtStart = false;
	public float speed = 3;

	void Start()
	{
		if(applyForceAtStart)
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, speed), ForceMode2D.Impulse);
	}

	void FixedUpdate () 
	{
		if(!applyForceAtStart)
			transform.position += transform.up * Time.deltaTime * speed;
	}
}

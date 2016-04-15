using UnityEngine;
using System.Collections;

public class moverBasic : MonoBehaviour {

	[Tooltip("Apply the speed to the Rigidbody2D at start. " +
		"Doesn't use Update (cannot change speed during play). If False, FixedUpdate is used.")]
	public bool applyForceAtStart = false;
	public Vector3 speed = new Vector3 (0, 1, 0);

	void Start()
	{
		if(applyForceAtStart)
			GetComponent<Rigidbody2D>().AddForce(speed, ForceMode2D.Impulse);
	}

	void FixedUpdate () 
	{
		if(!applyForceAtStart)
			transform.Translate(speed * Time.deltaTime);
	}
}

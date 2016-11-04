using UnityEngine;
using System.Collections;

public class moverBasic : MonoBehaviour {

	[Tooltip("Apply the speed to the Rigidbody2D at start. " +
		"Doesn't use Update (cannot change speed during play). If False, FixedUpdate is used.")]
	public bool applyForceOnEnable = false;
	public Vector3 speed = new Vector3 (0, 1, 0);

	void OnEnable()
	{
		if(applyForceOnEnable)
			GetComponent<Rigidbody2D>().AddForce(speed, ForceMode2D.Impulse);
	}

	void FixedUpdate () 
	{
		if(!applyForceOnEnable)
			transform.Translate(speed * Time.deltaTime);
	}
}

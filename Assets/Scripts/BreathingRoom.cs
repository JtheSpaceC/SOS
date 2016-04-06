using UnityEngine;
using System.Collections;

public class BreathingRoom : MonoBehaviour {

	private Vector2 direction;
	Rigidbody2D parentsRigidbody;

	void Awake()
	{
		parentsRigidbody = GetComponentInParent<Rigidbody2D> ();
	}


	void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("BreathingRoom"))
	    {
			direction = (transform.position - other.transform.position).normalized;
			parentsRigidbody.AddForce (direction * 200 * Time.deltaTime);
		}
	}
}

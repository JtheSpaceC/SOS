using UnityEngine;
using System.Collections;

public class BrigPerson : MonoBehaviour {

	[HideInInspector] public Collider2D myCollider;


	void Awake()
	{
		myCollider = GetComponent<Collider2D>();
	}
}

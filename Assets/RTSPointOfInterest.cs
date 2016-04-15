using UnityEngine;
using System.Collections;

public class RTSPointOfInterest : MonoBehaviour {

	SpriteRenderer myRenderer;


	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
	}

	void OnMouseEnter()
	{
		myRenderer.color = Color.white;
	}

	void OnMouseExit()
	{
		myRenderer.color = Color.grey;
	}
}

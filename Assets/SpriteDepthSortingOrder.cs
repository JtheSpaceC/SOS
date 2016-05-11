using UnityEngine;
using System.Collections;

public class SpriteDepthSortingOrder : MonoBehaviour {

	SpriteRenderer myRenderer;

	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
	}

	void Update () 
	{
		myRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.z);
	}
}

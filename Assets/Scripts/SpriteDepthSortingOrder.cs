using UnityEngine;
using System.Collections;

public class SpriteDepthSortingOrder : MonoBehaviour {

	SpriteRenderer myRenderer;
	int startingOrderInLayer;

	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
		startingOrderInLayer = myRenderer.sortingOrder;
	}

	[ExecuteInEditMode]
	void Update () 
	{
		myRenderer.sortingOrder = -Mathf.RoundToInt(100 * transform.position.z) + startingOrderInLayer;
	}
}

using UnityEngine;

public class SpriteDepthAndSortingOrder : MonoBehaviour {

	public bool changeSortingOrder = false;
	SpriteRenderer myRenderer;
	int startingOrderInLayer;

	[Tooltip("Lerp between White and Black by Z distance?")] public bool darkenWithDepth = true;
	[Tooltip("How far away is absolute black?")] public float blackDistance = 150f;

	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
		startingOrderInLayer = myRenderer.sortingOrder;
	}

	void Update () 
	{
		if(changeSortingOrder)
			myRenderer.sortingOrder = -Mathf.RoundToInt(100 * transform.position.z) + startingOrderInLayer;
		if(darkenWithDepth)
			DepthDarkening();			
	}

	[ContextMenu("Darken With Depth")]
	void DepthDarkening()
	{
		myRenderer.color = Color.Lerp(Color.white, Color.black, transform.position.z / blackDistance);
	}
}

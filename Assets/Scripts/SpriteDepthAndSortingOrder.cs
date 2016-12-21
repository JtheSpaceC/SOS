using UnityEngine;

[ExecuteInEditMode]
public class SpriteDepthAndSortingOrder : MonoBehaviour {

	SpriteRenderer myRenderer;
	int startingOrderInLayer = 0;

	public enum SortingOrderMode {DontChange, ChangeAtStart, Continuous};
	public SortingOrderMode sortingOrderMode;

	[Tooltip("Lerp between White and Black by Z distance?")] public bool darkenWithDepth = false;
	[Tooltip("How far away is absolute black?")] public float blackDistance = 500f;


	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
		this.enabled = true;
	}

	void OnEnable()
	{
		if(sortingOrderMode == SortingOrderMode.ChangeAtStart)
			ChangeSortingOrder();
	}

	void Update () 
	{
		if(sortingOrderMode == SortingOrderMode.Continuous)
			ChangeSortingOrder();
		if(darkenWithDepth)
			DepthDarkening();			
	}

	void DepthDarkening()
	{
		myRenderer.color = Color.Lerp(Color.white, Color.black, transform.position.z / blackDistance);
	}

	[ContextMenu("Change Layer")]
	public void ChangeSortingOrder()
	{
		myRenderer.sortingOrder = -Mathf.RoundToInt(100 * transform.position.z) + startingOrderInLayer;
	}
}

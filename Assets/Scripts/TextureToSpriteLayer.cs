using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextureToSpriteLayer : MonoBehaviour {

	public string desiredLayerName;
	public int desiredSortingOrder = 0;
	public bool updateSortingOrderByDepth = false;

	Renderer myRenderer;

	[ContextMenu("Change Layer")]
	void Start () 
	{
		myRenderer = GetComponent<Renderer>();
		ChangeLayer ();
	}

	void ChangeLayer()
	{
		myRenderer.sortingLayerName = desiredLayerName;
		myRenderer.sortingOrder = desiredSortingOrder;
	}

	void Update()
	{
		if(updateSortingOrderByDepth)
			myRenderer.sortingOrder = -Mathf.RoundToInt(100 * transform.position.z);
	}
}

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextureToSpriteLayer : MonoBehaviour {

	public string desiredLayerName;
	public int desiredSortingOrder = 0;

	[ContextMenu("Change Layer")]
	void Start () 
	{
		ChangeLayer ();
	}

	void ChangeLayer()
	{
		GetComponent<Renderer> ().sortingLayerName = desiredLayerName;
		GetComponent<Renderer> ().sortingOrder = desiredSortingOrder;
	}

}

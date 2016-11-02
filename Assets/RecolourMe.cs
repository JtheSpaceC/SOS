using UnityEngine;
using System.Collections;

public class RecolourMe : MonoBehaviour {

	[Tooltip("How much of the Director's scene tint should we accept into this renderer?")]
	[Range(0, 1)]
	public float recolourAmount = 0.5f;

	void Start () 
	{
		SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();

		myRenderer.color = Director.instance.AdjustColour(myRenderer.color, Director.instance.sceneTint, recolourAmount);
	}
}

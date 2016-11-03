using UnityEngine;
using System.Collections;

public class RecolourMe : MonoBehaviour {

	SpriteRenderer myRenderer;

	[Tooltip("How much of the Director's scene tint should we accept into this renderer?")]
	[Range(0, 1)]
	public float recolourAmount = 0.5f;

	public bool updateWhileEditorIsPlaying = false;
	Color startingColour;



	void Start () 
	{
		myRenderer = GetComponent<SpriteRenderer>();
		startingColour = myRenderer.color;

		myRenderer.color = Director.instance.AdjustColour(startingColour, Director.instance.sceneTint, recolourAmount);
	}

	#if UNITY_EDITOR

	void Update()
	{
		if(updateWhileEditorIsPlaying)
		{
			myRenderer.color = Director.instance.AdjustColour(startingColour, Director.instance.sceneTint, recolourAmount);
		}
	}

	#endif
}

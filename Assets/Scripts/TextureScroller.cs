using UnityEngine;
using System.Collections;

public class TextureScroller : MonoBehaviour {

	Renderer myRenderer;
	Vector3 offset;
	public float offsetRatio = 0.002f;
	public bool grow = false;
	public float maxGrowSize = 10;
	public float descentTime = 20;

	float newScale;
	float startScale;
	float growStartTime;

	[Tooltip("how fast the texture turns East to West (or vice versa) regardless of Player movement")] 
	public float planetaryRotation = 0.003f;

	void Start () 
	{
		myRenderer  = GetComponent<Renderer>();
		offset = Camera.main.transform.position - transform.position;
		startScale = transform.localScale.x;
	}
	
	void LateUpdate () 
	{
		myRenderer.material.mainTextureOffset = -(Vector2) Camera.main.transform.position * offsetRatio + new Vector2(-planetaryRotation * Time.time, 0);
		transform.position = Camera.main.transform.position - offset;
	}

	void Update()
	{
		if(!grow)
			return;

		if(grow && growStartTime == 0)
			growStartTime = Time.time;

		if(Time.time > (growStartTime + descentTime))
			grow = false;

		newScale = startScale + ((Mathf.Pow( (Time.time - growStartTime), 2)) / Mathf.Pow (descentTime, 2) )* maxGrowSize;
		transform.localScale = new Vector3 (newScale, 1, newScale);

	}
}

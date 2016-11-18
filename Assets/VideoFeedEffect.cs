using UnityEngine;
using System.Collections;

public class VideoFeedEffect : MonoBehaviour {

	Material myMaterial;
	Vector2 offset;
	Color startColor;
	Color newColor;
	float newAlpha;
	float startTime;
	[Range(0,1)]
	public float minOpacity = 0.3f;
	[Range(0,1)]
	public float maxOpacity = 0.9f;
	public float fadeTime = 1;

	void Start () 
	{
		myMaterial = GetComponent<MeshRenderer>().material;
		StartCoroutine("FadingEffect");
	}
	
	void Update () 
	{
		offset.x = Random.Range(0f, 10f);
		offset.y = Random.Range(0f, 10f);
		myMaterial.mainTextureOffset = offset;
	}

	IEnumerator FadingEffect()
	{
		startColor = myMaterial.color;
		newAlpha = Random.Range(minOpacity, maxOpacity);
		newColor = Color.white;
		newColor.a = newAlpha;
		startTime = Time.time;

		while(myMaterial.color != newColor)
		{
			myMaterial.color = Color.Lerp(startColor, newColor, (Time.time-startTime)/fadeTime);
			yield return new WaitForEndOfFrame();
		}

		StartCoroutine("FadingEffect");
	}
}

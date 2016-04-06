using UnityEngine;
using System.Collections;

public class RandomPropertiesForSprite : MonoBehaviour {

	[Header("Size")]
	public bool randomizeSize = true;

	[Range(0.01f, 100)]
	public float minSize = 0.01f;

	[Range(0.01f, 100)]
	public float maxSize = 3f;

	[Header("Brightness")]
	public bool randomizeBrightness = true;
	public Color darkest;
	public Color lightest;

	[Header("Random Sprite")]
	public bool randomizeSprite = true;
	public Sprite[] sprites;



	void Start () 
	{
		if(randomizeSize)
		{
			float newSize = Random.Range(minSize, maxSize);
			transform.localScale = new Vector3(newSize, newSize, 1);
		}
		if(randomizeBrightness)
		{
			Color newColor = Color.Lerp(darkest, lightest, Random.Range(0.01f, 1));
			GetComponent<SpriteRenderer>().color = newColor;
		}
		if(randomizeSprite)
		{
			GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
		}
	}
}

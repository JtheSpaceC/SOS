using UnityEngine;
using System.Collections;

public class SpriteBlendingAnimationExperiment : MonoBehaviour {

	public Sprite[] frames;

	public SpriteRenderer rendA;
	public SpriteRenderer rendB;

	public float fps = 12;

	int currentFrameA;
	int currentFrameB;


	void Start()
	{
		rendA.sprite = frames[0];
		currentFrameA = 0;
		rendB.sprite = frames[1];
		currentFrameB = 1;

		StartCoroutine(SpriteBlendAtoB());
	}


	IEnumerator SpriteBlendAtoB()
	{
		while(rendA.color.a > 0)
		{
			Color newColourA = rendA.color;
			newColourA.a -= 2f/fps;
			rendA.color = newColourA;

			Color newColourB = rendB.color;
			newColourB.a += 2f/fps;
			rendB.color = newColourB;

			yield return new WaitForEndOfFrame();
			print(rendA.color.a);
		}

		currentFrameA += 2;

		if(currentFrameA >= frames.Length)
			currentFrameA = 0;
		rendA.sprite = frames[currentFrameA];

		StartCoroutine(SpriteBlendBtoA());
	}

	IEnumerator SpriteBlendBtoA()
	{
		while(rendB.color.a > 0)
		{
			Color newColourA = rendA.color;
			newColourA.a += 2f/fps;
			rendA.color = newColourA;

			Color newColourB = rendB.color;
			newColourB.a -= 2f/fps;
			rendB.color = newColourB;

			yield return new WaitForEndOfFrame();
		}

		currentFrameB += 2;

		if(currentFrameB >= frames.Length)
			currentFrameB = 0;
		rendB.sprite = frames[currentFrameB];

		StartCoroutine(SpriteBlendAtoB());
	}
}

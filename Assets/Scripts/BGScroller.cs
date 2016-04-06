using UnityEngine;
using System.Collections;

public class BGScroller : MonoBehaviour {

	public bool basedOnPlayer = true;
	public bool basedOnOtherObject = false;
	public bool simulateDogfight = false;
	public Transform whichObject;
	public bool autoScrollInX = false;
	public float scrollSpeed = 1;
	public float dogfightVerticalScrollSpeed = 0.02f;
	public float dogfightHorizontalScrollSpeed = 0f;
	GameObject player;
	Rigidbody2D prb;

	float originalHorizontalScrollSpeed;
	float originalVerticalScrollSpeed;


	void Awake()
	{
		player = GameObject.FindGameObjectWithTag ("PlayerFighter");
		if(player != null)
			prb = player.GetComponent<Rigidbody2D> ();

		if(basedOnPlayer)
			basedOnOtherObject = false;

	}

	void Update ()
	{
		Vector2 finalTextureOffset = GetComponent<Renderer>().material.mainTextureOffset;

		if(basedOnPlayer)
		{
			finalTextureOffset += new Vector2 (prb.velocity.x *scrollSpeed*Time.deltaTime, prb.velocity.y *scrollSpeed*Time.deltaTime);
		}
		else if(autoScrollInX)
			finalTextureOffset += new Vector2 (scrollSpeed*Time.deltaTime, 0);
		else if(basedOnOtherObject && whichObject != null)
			finalTextureOffset += new Vector2 (whichObject.position.x *scrollSpeed*Time.deltaTime, whichObject.position.y *scrollSpeed*Time.deltaTime);
		else if(simulateDogfight)
		{
			finalTextureOffset += new Vector2 (dogfightHorizontalScrollSpeed * Time.deltaTime, dogfightVerticalScrollSpeed * Time.deltaTime);
		}

		if(finalTextureOffset.x >= 1.0f)
		{
			finalTextureOffset.x -= (float)System.Math.Truncate(finalTextureOffset.x);
		}
		while(finalTextureOffset.x < 0.0f)
		{
			finalTextureOffset.x += 1.0f;
		}
		if(finalTextureOffset.y >= 1.0f)
		{
			finalTextureOffset.y -= (float)System.Math.Truncate(finalTextureOffset.y);
		}
		while(finalTextureOffset.y < 0.0f)
		{
			finalTextureOffset.y += 1.0f;
		}
		
		GetComponent<Renderer>().material.mainTextureOffset = finalTextureOffset;
	}

	public void FastTurnRandomDirection()
	{
		originalHorizontalScrollSpeed = dogfightHorizontalScrollSpeed;
		originalVerticalScrollSpeed = dogfightVerticalScrollSpeed;

		dogfightHorizontalScrollSpeed = 0.3f * Mathf.Pow(-1, Random.Range(1, 3)); //will become either positive or negative
		dogfightVerticalScrollSpeed = 0.3f * Mathf.Pow(-1, Random.Range(1, 3));
	}

	public void ReturnToNormal()
	{
		dogfightHorizontalScrollSpeed = originalHorizontalScrollSpeed;
		dogfightVerticalScrollSpeed = originalVerticalScrollSpeed;
	}
}



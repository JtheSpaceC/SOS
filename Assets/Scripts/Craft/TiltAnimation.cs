﻿using UnityEngine;
using System.Collections;

public class TiltAnimation : MonoBehaviour {

	public bool playerControlled = false;
	[Range(-1 , 1)]
	public float tilt = 0;
	float positiveTiltValue;
	public float paddingAmount = 2.5f;
	[HideInInspector] public float targetValue;
	[HideInInspector] public float accelerationValue;
	float direction;

	public Sprite[] turnFrames;
	[Tooltip("Depending on how the sprite sheet was rendered, these may need to be flipped again.")]
	public bool needToInvertTurnFrames = false;
	SpriteRenderer myRenderer;
	int previousFrame;
	int chosenFrame;

	public Transform[] alsoRotate;
	int alsoRotateLength;
	public float maxRotation = 85;

	void OnEnable()
	{
		myRenderer = GetComponentInParent<SpriteRenderer>();
		alsoRotateLength = alsoRotate.Length;
	}

	
	void Update () 
	{
		if(playerControlled)
		{
			if(Input.GetButton("Afterburners"))
				accelerationValue = 1;
			else
				accelerationValue = 0.25f + (0.45f*Input.GetAxisRaw("Accelerate"));

			targetValue = Input.GetAxisRaw("Horizontal") * accelerationValue;
		}
		//else targetValue will be changed in the Engine Scipt of the fighter

		direction = (targetValue - tilt);
		if(direction != 0)
			direction = Mathf.Sign(direction) * 1;
		else direction = 0;

		tilt += direction * paddingAmount * Time.deltaTime;
		tilt = Mathf.Clamp(tilt, -1, 1);

		positiveTiltValue = Mathf.Abs(tilt);

		chosenFrame = Mathf.FloorToInt((turnFrames.Length-1) * positiveTiltValue);
		if(chosenFrame != previousFrame)
		{
			if(tilt < 0)
			{
				myRenderer.flipX = true;
				if(needToInvertTurnFrames)
					myRenderer.flipX = !myRenderer.flipX;
			}
			else if(tilt > 0)
			{
				myRenderer.flipX = false;
				if(needToInvertTurnFrames)
					myRenderer.flipX = !myRenderer.flipX;
			}
			previousFrame = chosenFrame;
		}

		myRenderer.sprite = turnFrames[chosenFrame];

		for(int i = 0; i < alsoRotateLength; i++)
		{
			alsoRotate[i].transform.localRotation = Quaternion.Euler(0, maxRotation * tilt ,0);
		}
	}

	void OnDisable()
	{
		tilt = 0;
		myRenderer.sprite = turnFrames[0];
		foreach(Transform trans in alsoRotate)
		{
			trans.localRotation = Quaternion.Euler(Vector3.zero);
		}
	}
}

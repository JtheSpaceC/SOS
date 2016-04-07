﻿using UnityEngine;
using System.Collections;

public class WarpDrive : MonoBehaviour {

	[HideInInspector] public SpriteRenderer warpBubble;
	[HideInInspector] public Vector2 warpLookDirection;

	void Awake () 
	{
		warpBubble = GetComponentInChildren<SpriteRenderer>();
	}

}

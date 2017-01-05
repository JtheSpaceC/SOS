using UnityEngine;
using System.Collections;

public class HoloMapObject : MonoBehaviour {

	protected SpriteRenderer myRenderer;

	protected void AwakeBaseClass()
	{
		myRenderer = GetComponent<SpriteRenderer>();


	}
}

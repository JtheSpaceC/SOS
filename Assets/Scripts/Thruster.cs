using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Thruster : MonoBehaviour {

	[HideInInspector] public SpriteRenderer myRenderer;
	[HideInInspector] public float lastTurnedOnTime;
	public float minimumFireTime = 0.1f;

	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
	}

}

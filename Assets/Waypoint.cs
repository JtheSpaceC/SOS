using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {

	[HideInInspector] public Vector2[] waypoints;
	[HideInInspector] public float activationRadius = 15f;

	public enum BehaviourType {Constant, Checkpoint, ClearArea};
	[HideInInspector] public BehaviourType behaviourType;


	void OnTriggerEnter2D(Collider2D other)
	{
		AudioMasterScript.instance.PlayChime();
	}
}

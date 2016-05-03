using UnityEngine;
using System.Collections.Generic;

public class SupportShipFunctions : TargetableObject {

	[HideInInspector]public HealthTransport healthScript;
	[HideInInspector]public EnginesFighter engineScript;

	public List<GameObject> myTurrets;


	[Header ("Warp Stuff")]
	public float warpInTime = 10;
	public AudioSource engineAudioSource;
	public AudioClip warpStart;
	public AudioClip warpLoop;
	public AudioClip warpEnd;

	[HideInInspector] public Vector2 waypoint;



	protected void HoldPosition()
	{
		if(switchingState)
		{
			switchingState = false;
		}

		if(waypoint == Vector2.zero || Vector2.Distance(transform.position, waypoint) < 0.2f)
		{			
			waypoint = transform.position + (transform.up * Random.Range(-1, 2) * 0.5f);
		}
		engineScript.MoveToTarget (waypoint, false);
	}

	protected void MoveToPosition(Vector2 pos)
	{
		if(switchingState)
		{
			switchingState = false;
		}
		engineScript.LookAtTarget(pos);
		engineScript.MoveToTarget(pos, true);

		if(Vector2.Distance(transform.position, pos) < 0.5f)
		{
			completedState = true;
		}
	}

	protected void MatchTargetVelocity(Vector2 targetPosition, Rigidbody2D targetRB, Vector2 offset)
	{
		Vector2 desiredPos = targetPosition + offset + targetRB.velocity;
		if(targetRB.velocity.magnitude < 0.25f)
			engineScript.MoveToTarget (desiredPos, true);
		else 
			engineScript.MoveToTarget (desiredPos, false);
	}

	protected void CommenceFadeout()
	{
		Tools.instance.CommenceFadeout (3);
	}
}

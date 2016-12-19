using UnityEngine;
using System.Collections.Generic;

public class SupportShipFunctions : TargetableObject {

	[HideInInspector]public HealthTransport healthScript;
	[HideInInspector]public EnginesFighter engineScript;

	[HideInInspector] public List<GameObject> myTurrets;
	[Range(0, 100)]
	public float howWellDefended = 0;

	[Header ("Warp Stuff")]
	public float warpInTime = 10;
	public AudioSource engineAudioSource;
	public AudioClip warpStart;
	public AudioClip warpLoop;
	public AudioClip warpEnd;

	[HideInInspector] public Vector2 waypoint;

	[Header ("Hangars or Docking ports (both named \"Hangars\"")]
	public Transform[] hangars;



	protected void HoldPosition()
	{
		if(switchingStates)
		{
			switchingStates = false;
		}

		if(waypoint == Vector2.zero || Vector2.Distance(transform.position, waypoint) < 0.2f)
		{			
			waypoint = transform.position + (transform.up * Random.Range(-1, 2) * 0.5f);
		}
		engineScript.MoveToTarget (waypoint, false);
	}

	protected void MoveToPosition(Vector2 pos)
	{
		if(switchingStates)
		{
			switchingStates = false;
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

	public void ChangeTurretsSide(WhichSide newSide)
	{
		foreach(GameObject turret in myTurrets)
		{
			WeaponsTurret wt = turret.GetComponent<WeaponsTurret>();
			wt.whichSide = newSide;
			wt.SetUpSideInfo();
			wt.targetsMask = wt.myCommander.turretTargetsMask;

			if(wt.myCommander == Tools.instance.pmcCommander)
				turret.layer = LayerMask.NameToLayer("PMCTurrets");
			else if(wt.myCommander == Tools.instance.pirateCommander)
				turret.layer = LayerMask.NameToLayer("EnemyTurrets");
		}
	}
}

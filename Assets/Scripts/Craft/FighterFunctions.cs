using UnityEngine;
using System.Collections.Generic;

public class FighterFunctions : TargetableObject {
	
	protected float constantThrustProportion;
	protected int killsThisBattle;
	protected int totalKills;

	enum JoustingStates {OnApproach, GainingDistance};
	JoustingStates joustingStates;

	float distanceFromTarget;
	public float joustingDistance = 20f;
	Vector2 gainingDistanceWaypoint;
	[HideInInspector] public Vector2 joustingVector;


	// not used
	protected GameObject CheckLocaleForTargets(Vector3 originPoint, float sensorRadius, LayerMask targetsMask)
	{
		Collider2D[] targets = Physics2D.OverlapCircleAll ((Vector2)originPoint, sensorRadius, targetsMask);
		float closestDist = Mathf.Infinity;
		GameObject closestGameObject = null;

		if (targets.Length == 0)
			return null;

		foreach(Collider2D target in targets)
		{
			if(Vector2.Distance(originPoint, target.transform.position) < closestDist)
			{
				closestGameObject = target.gameObject;
				closestDist = Vector2.Distance(originPoint, target.transform.position);
			}
		}
		return closestGameObject.gameObject;
	}

	protected GameObject SelectAnEnemyAttackingMe(List <GameObject> allAttackers)
	{
		if (allAttackers.Count == 0)
			return null;

		float closestDist = Mathf.Infinity;
		GameObject closestGameObject = null;

		foreach(GameObject target in allAttackers)
		{
			if(!Tools.instance.CheckTargetIsLegit(target))
			{
				//Debug.Log("BIG FUCKING ERROR WITH " + target.name + " on "+gameObject.name + "!!!!!!");
				allAttackers.Remove(target);
				break;
			}
			else if(Vector2.Distance(transform.position, target.transform.position) < closestDist)
			{
				closestGameObject = target.gameObject;
				closestDist = Vector2.Distance(transform.position, target.transform.position);
			}
		}
		return closestGameObject;
	}


	protected void TailingFunction(EnginesFighter engineScript, GameObject target, WeaponsPrimaryFighter shootScript, float constantForwardThrustProportion)
	{
		//NB: It's important that we move before look, because the firing solution (in look) requires knowing the new movement position of the target

		engineScript.LookAtTarget (target);

		if(Vector2.Angle(transform.up, target.transform.position - transform.position)%180 < 165)
		{
			engineScript.MoveToTarget (target, constantForwardThrustProportion);
		}
				
		if(Time.time > shootScript.nextFire &&
		   (ReadyAimFire(target, transform, shootScript.weaponsRange) == true || TakePotshot(transform, shootScript.weaponsRange) == true))
		{
			if(shootScript.enabled)
			{
				shootScript.FirePrimary(false);
			}
		}
	}

	protected void JoustingFunction(EnginesFighter engineScript, GameObject target, WeaponsPrimaryFighter shootScript, float constantForwardThrustProportion)
	{
		//NB: It's important that we move before look, because the firing solution (in look) requires knowing the new movement position of the target

		distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);

		if(joustingStates == JoustingStates.OnApproach)
		//we want to fly towards and look/shoot at the target
		{
			engineScript.LookAtTarget (target);

			//so the AI doesn't turn immediately after passing, but there's a slight delay as they turn first. 
			//this is more like the way a player normally moves, so the AI don't get a turning advantage.
			if(Vector2.Angle(transform.up, target.transform.position - transform.position)%180 < 30) 
			{
				//TODO: work out a new waypoint

				engineScript.MoveToTarget (target, constantForwardThrustProportion);
			}
			else //this is where we know we've passed and must gain distance
			{
				print(name +" is now Gaining Distance");
				joustingStates = JoustingStates.GainingDistance;
			}

			//SHOOTING
			if(Time.time > shootScript.nextFire &&
				(ReadyAimFire(target, transform, shootScript.weaponsRange) == true || TakePotshot(transform, shootScript.weaponsRange) == true))
			{
				if(shootScript.enabled)
				{
					shootScript.FirePrimary(false);
				}
			}
		}
		else if(joustingStates == JoustingStates.GainingDistance)
		//then we want to go to a point away from the target before turning around
		{
			//TODO: Start moving away at current velocity so you escape faster, then adjust to turn away from enemy more explicitly?
			if(distanceFromTarget < joustingDistance)
			{
				//keep gaining distance
				gainingDistanceWaypoint = (Vector2)transform.position + gainingDistanceWaypoint;

				engineScript.LookAtTarget(gainingDistanceWaypoint);
				engineScript.MoveToTarget(gainingDistanceWaypoint, false);
			}
			else
			{
				//switch switch back to OnApproach
				print(name +" is now On Approach");
				joustingStates = JoustingStates.OnApproach;

				//create a line to follow, centred on the target
				joustingVector = (target.transform.position - transform.position).normalized;
			}

			//if we can't gain distance because the target is as fast or faster than us and is targeting us
		}
	}


	protected Vector2 ChooseEvadePos(Rigidbody2D myRigidbody, LayerMask mask, AICommander myCommanderScript)
	{
		Vector3 forward = myRigidbody.velocity.magnitude == 0? transform.up : (Vector3)myRigidbody.velocity.normalized;
		Vector3 targetDir = Quaternion.AngleAxis(90*(Mathf.Pow(-1, Random.Range(1,3))) , Vector3.forward)*forward;
		return (Vector2)(transform.position + (targetDir * 2000));
	}

	protected Vector2 ChooseRetreatPosition(Rigidbody2D myRigidbody, LayerMask mask, AICommander enemyCommanderScript)
	{
		Collider2D[] enemies = Physics2D.OverlapCircleAll (transform.position, 10f, mask);
		Vector2 averagePos = Vector2.zero;
		
		if(enemies.Length == 0)
		{
			Vector2 dir = ((Vector2)transform.position - enemyCommanderScript.AverageForcesPosition()).normalized*20;
			return  ((Vector2)transform.position + myRigidbody.velocity) + dir;
		}		
		else
		{
			foreach(Collider2D col in enemies)
			{
				Vector2 enemyVel = col.tag == "Turret"? 
					col.transform.parent.parent.GetComponent<Rigidbody2D>().velocity : 
						col.GetComponent<Rigidbody2D>().velocity;
				averagePos += ((Vector2)col.transform.position + enemyVel);
			}
			averagePos /= enemies.Length;
			
			Vector2 dir = ((Vector2)transform.position + myRigidbody.velocity - averagePos).normalized*20;
			return  ((Vector2)transform.position + myRigidbody.velocity) + dir;
		}
	}


	protected void RemoveBadTargetFromLeadersAttackers(GameObject leader, GameObject badTarget)
	{
		if (leader.tag == "PlayerFighter")
			leader.GetComponent<PlayerAILogic> ().myAttackers.Remove (badTarget);
		else
			leader.GetComponent<AIFighter>().myAttackers.Remove (badTarget);
	}


	void ToggleWeaponsOnOff(bool turnOn)
	{
		if(GetComponentInChildren<WeaponsPrimaryFighter>() != null)
			GetComponentInChildren<WeaponsPrimaryFighter>().enabled = turnOn;

		if (GetComponentInChildren<WeaponsSecondaryFighter> () != null)
			GetComponentInChildren<WeaponsSecondaryFighter> ().enabled = turnOn;

		if(GetComponentInChildren<WeaponsTurret>() != null)
			GetComponentInChildren<WeaponsTurret>().enabled = turnOn;
	}

}//MONO
